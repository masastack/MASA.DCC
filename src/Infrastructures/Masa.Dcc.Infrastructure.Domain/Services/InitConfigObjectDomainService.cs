// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Services;

public class InitConfigObjectDomainService : DomainService
{
    private readonly IConfigObjectReleaseRepository _configObjectReleaseRepository;
    private readonly IConfigObjectRepository _configObjectRepository;
    private readonly IPublicConfigRepository _publicConfigRepository;
    private readonly IMultilevelCacheClient _memoryCacheClient;
    private readonly IMasaStackConfig _masaStackConfig;
    private readonly IUnitOfWork _unitOfWork;

    public InitConfigObjectDomainService(
        IDomainEventBus eventBus,
        IConfigObjectReleaseRepository configObjectReleaseRepository,
        IConfigObjectRepository configObjectRepository,
        IPublicConfigRepository publicConfigRepository,
        IMultilevelCacheClient memoryCacheClient,
        IMasaStackConfig masaStackConfig,
        IUnitOfWork unitOfWork) : base(eventBus)
    {
        _configObjectReleaseRepository = configObjectReleaseRepository;
        _configObjectRepository = configObjectRepository;
        _publicConfigRepository = publicConfigRepository;
        _memoryCacheClient = memoryCacheClient;
        _masaStackConfig = masaStackConfig;
        _unitOfWork = unitOfWork;
    }

    private string EncryptContent(string content)
    {
        var secret = _masaStackConfig.DccSecret;
        var encryptContent = AesUtils.Encrypt(content, secret, FillType.Left);
        return encryptContent;
    }

    private async Task AddConfigObjectReleaseAsync(AddConfigObjectReleaseDto dto)
    {
        var configObject = (await _configObjectRepository.FindAsync(configObject => configObject.Id == dto.ConfigObjectId)) ?? throw new Exception("Config object does not exist");

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
        int envClusterId,
        string appId,
        Dictionary<string, string> configObjects,
        ConfigObjectType configObjectType = ConfigObjectType.App,
        bool isEncryption = false)
    {
        foreach (var configObject in configObjects)
        {
            var configObjectName = configObject.Key;
            string content = configObject.Value;
            if (isEncryption)
                content = EncryptContent(content);

            var newConfigObject = new ConfigObject(
                configObjectName,
                "JSON",
                configObjectType,
                content,
                "{}",
                encryption: isEncryption);

            var publicConfig = await _publicConfigRepository.FindAsync(publicConfig => publicConfig.Identity == appId);
            if (publicConfig == null)
                throw new UserFriendlyException("只能初始化public配置");

            int objectId = 0;
            var existsConfigObject = await _configObjectRepository.FindAsync(configObject => configObject.Type == configObjectType && configObject.Name == configObjectName);
            if (existsConfigObject == null)
            {
                newConfigObject.SetConfigObjectType(ConfigObjectType.App);
                newConfigObject.SetPublicConfigObject(publicConfig.Id, envClusterId);
                await _configObjectRepository.AddAsync(newConfigObject);
                await _unitOfWork.SaveChangesAsync();
                objectId = newConfigObject.Id;
            }
            else
            {
                objectId = existsConfigObject.Id;
                var value = configObject.Value;
                if (isEncryption)
                {
                    value = EncryptContent(value);
                }
                existsConfigObject.UpdateContent(value);
                await _configObjectRepository.UpdateAsync(existsConfigObject);
            }

            var key = $"{environmentName}-{clusterName}-{appId}-{configObjectName}".ToLower();
            var redisData = await _memoryCacheClient.GetAsync<PublishReleaseModel?>(key);
            if (redisData != null)
            {
                continue;
            }

            var releaseModel = new AddConfigObjectReleaseDto
            {
                Type = ReleaseType.MainRelease,
                ConfigObjectId = objectId,
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
