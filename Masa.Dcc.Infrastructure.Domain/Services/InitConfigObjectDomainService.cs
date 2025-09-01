// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Services;

public class InitConfigObjectDomainService(
    IDomainEventBus eventBus,
    IConfigObjectReleaseRepository configObjectReleaseRepository,
    IConfigObjectRepository configObjectRepository,
    IAppConfigObjectRepository appConfigObjectRepository,
    IPublicConfigRepository publicConfigRepository,
    IMultilevelCacheClient memoryCacheClient,
    IPmClient pmClient,
    IMasaStackConfig masaStackConfig,
    IUnitOfWork unitOfWork) : DomainService(eventBus)
{
    private readonly IConfigObjectReleaseRepository _configObjectReleaseRepository = configObjectReleaseRepository;
    private readonly IConfigObjectRepository _configObjectRepository = configObjectRepository;
    private readonly IAppConfigObjectRepository _appConfigObjectRepository = appConfigObjectRepository;
    private readonly IPublicConfigRepository _publicConfigRepository = publicConfigRepository;
    private readonly IMultilevelCacheClient _memoryCacheClient = memoryCacheClient;
    private readonly IPmClient _pmClient = pmClient;
    private readonly IMasaStackConfig _masaStackConfig = masaStackConfig;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

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

        var relationConfigObjects = await _configObjectRepository.GetRelationConfigObjectWithReleaseHistoriesAsync(configObject.Id);
        if (relationConfigObjects != null && relationConfigObjects.Count > 0)
        {
            // var allEnvClusters = await _pmClient.ClusterService.GetEnvironmentClustersAsync();
            var appConfigObjects = await _appConfigObjectRepository.GetListAsync(app => relationConfigObjects.Select(c => c.Id).Contains(app.ConfigObjectId));
            var apps = await _pmClient.AppService.GetListAsync();

            foreach (var item in appConfigObjects)
            {
                // var envCluster = allEnvClusters.FirstOrDefault(c => c.Id == item.EnvironmentClusterId) ?? new();
                var ClusterName = _masaStackConfig.Cluster;
                var app = apps.FirstOrDefault(a => a.Id == item.AppId) ?? new();
                var relationConfigObject = relationConfigObjects.First(c => c.Id == item.ConfigObjectId);
                var key = $"{_masaStackConfig.Environment}-{_masaStackConfig.Cluster}-{app.Identity}-{relationConfigObject.Name}";

                if (relationConfigObject.FormatLabelCode.ToLower() == "properties")
                {
                    var appRelease = relationConfigObject.ConfigObjectRelease.OrderByDescending(c => c.Id).FirstOrDefault();
                    if (appRelease == null)
                    {
                        await _memoryCacheClient.SetAsync(key.ToLower(), new PublishReleaseModel
                        {
                            Content = dto.Content,
                            FormatLabelCode = configObject.FormatLabelCode,
                            Encryption = configObject.Encryption
                        });
                    }
                    else
                    {
                        //compare
                        var publicContents = JsonSerializer.Deserialize<List<ConfigObjectPropertyContentDto>>(dto.Content) ?? new();
                        var appContents = JsonSerializer.Deserialize<List<ConfigObjectPropertyContentDto>>(appRelease.Content) ?? new();

                        var exceptContent = publicContents.ExceptBy(appContents.Select(c => c.Key), content => content.Key).ToList();
                        var content = appContents.Union(exceptContent).ToList();

                        var releaseContent = new PublishReleaseModel
                        {
                            Content = JsonSerializer.Serialize(content, _jsonSerializerOptions),
                            FormatLabelCode = configObject.FormatLabelCode,
                            Encryption = configObject.Encryption
                        };
                        await _memoryCacheClient.SetAsync(key.ToLower(), releaseContent);
                    }
                }
                else
                {
                    var releaseContent = new PublishReleaseModel
                    {
                        Content = dto.Content,
                        FormatLabelCode = configObject.FormatLabelCode,
                        Encryption = configObject.Encryption
                    };
                    await _memoryCacheClient.SetAsync(key.ToLower(), releaseContent);
                }
            }
        }
        else
        {
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
                throw new ArgumentException($"dcc init failed: Identity {appId} is not exists in PublicConfigs");

            newConfigObject.SetConfigObjectType(ConfigObjectType.App);
            newConfigObject.SetPublicConfigObject(publicConfig.Id, cluster.EnvironmentClusterId);

            //already init success
            if (await _configObjectRepository.FindAsync(x => x.Name == newConfigObject.Name && x.Type == newConfigObject.Type) == null)
            {
                await _configObjectRepository.AddAsync(newConfigObject);
                await _unitOfWork.SaveChangesAsync();
            }

            var key = $"{environmentName}-{clusterName}-{appId}-{configObjectName}".ToLower();
            var redisData = await _memoryCacheClient.GetAsync<PublishReleaseModel?>(key);
            if (redisData != null)
                continue;

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
