// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Services;

public class InitConfigObjectDomainService(
    IDomainEventBus eventBus,
    IConfigObjectReleaseRepository configObjectReleaseRepository,
    IConfigObjectRepository configObjectRepository,
    IPublicConfigRepository publicConfigRepository,
    IMultilevelCacheClient memoryCacheClient,
    IPmClient pmClient,
    IMasaStackConfig masaStackConfig,
    ILogger<InitConfigObjectDomainService> logger,
    IUnitOfWork unitOfWork) : DomainService(eventBus)
{
    private readonly IConfigObjectReleaseRepository _configObjectReleaseRepository = configObjectReleaseRepository;
    private readonly IConfigObjectRepository _configObjectRepository = configObjectRepository;
    private readonly IPublicConfigRepository _publicConfigRepository = publicConfigRepository;
    private readonly IMultilevelCacheClient _memoryCacheClient = memoryCacheClient;
    private readonly IPmClient _pmClient = pmClient;
    private readonly IMasaStackConfig _masaStackConfig = masaStackConfig;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger _logger = logger;

    private string EncryptContent(string content)
    {
        var secret = _masaStackConfig.DccSecret;

        var encryptContent = AesUtils.Encrypt(content, secret, FillType.Left);
        return encryptContent;
    }

    private async Task AddConfigObjectReleaseAsync(AddConfigObjectReleaseDto dto)
    {
        var configObject = await _configObjectRepository.FindAsync(configObject => configObject.Id == dto.ConfigObjectId);
        if (configObject == null)
            throw new Exception($"Config object ConfigObjectId:{dto.ConfigObjectId} does not exist");

        configObject.AddContent(configObject.Content, configObject.Content);
        await _configObjectRepository.UpdateAsync(configObject);

        var configObjectRelease = new ConfigObjectRelease(
               dto.ConfigObjectId,
               dto.Name,
               dto.Comment,
               configObject.Content);
        await _configObjectReleaseRepository.AddAsync(configObjectRelease);

        //add redis cache
        var key = $"{dto.EnvironmentName}-{dto.ClusterName}-{dto.Identity}-{configObject.Name}";
        if (configObject.Encryption)
        {
            dto.Content = EncryptContent(dto.Content);
        }
        var releaseContent = new PublishReleaseModel
        {
            Content = dto.Content,
            FormatLabelCode = configObject.FormatLabelCode,
            Encryption = configObject.Encryption
        };
        await _memoryCacheClient.SetAsync(key.ToLower(), releaseContent);
    }

    public async Task InitConfigObjectAsync(
        string environmentName,
        string clusterName,
        string appId,
        Dictionary<string, string> configObjects,
        ConfigObjectType configObjectType = ConfigObjectType.App,
        bool isEncryption = false)
    {
        var envs = await _pmClient.EnvironmentService.GetListAsync();
        var env = envs.FirstOrDefault(e => e.Name.ToLower() == environmentName.ToLower()) ?? throw new UserFriendlyException("Environment does not exist");
        var clusters = await _pmClient.ClusterService.GetListByEnvIdAsync(env.Id);
        var cluster = clusters.FirstOrDefault(c => c.Name.ToLower() == clusterName.ToLower()) ?? throw new UserFriendlyException("Cluster does not exist");

        var publicConfig = await _publicConfigRepository.FindAsync(publicConfig => publicConfig.Identity == appId);
        if (publicConfig == null)
            throw new ArgumentException($"dcc init failed: Identity {appId} is not exists in PublicConfigs");

        foreach (var configObject in configObjects)
        {
            var configObjectName = configObject.Key;
            string content = configObject.Value;
            if (isEncryption)
                content = EncryptContent(content);

            var newConfigObject = await _configObjectRepository.FindAsync(x => x.Name == configObjectName && x.Type == configObjectType);

            if (newConfigObject == null)
            {
                newConfigObject = new ConfigObject(
                   configObjectName,
                   "JSON",
                   configObjectType,
                   content,
                   "{}",
                   encryption: isEncryption);

                newConfigObject.SetConfigObjectType(ConfigObjectType.App);
                newConfigObject.SetPublicConfigObject(publicConfig.Id, cluster.EnvironmentClusterId);

                await _configObjectRepository.AddAsync(newConfigObject);
                await _unitOfWork.SaveChangesAsync();
            }

            var key = $"{environmentName}-{clusterName}-{appId}-{configObjectName}".ToLower();
            var redisData = await _memoryCacheClient.GetAsync<PublishReleaseModel?>(key);
            if (redisData != null)
                continue;

            _logger.LogInformation("InitConfigObjectAsync add redis cache key:{Key}", key);

            var releaseModel = new AddConfigObjectReleaseDto
            {
                Type = ReleaseType.MainRelease,
                ConfigObjectId = newConfigObject.Id,
                Name = "通过Sdk发布",
                EnvironmentName = environmentName,
                ClusterName = clusterName,
                Identity = appId,
                Content = configObject.Value
            };

            await AddConfigObjectReleaseAsync(releaseModel);
        }
    }
}
