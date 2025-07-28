// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Services;

public class ConfigObjectDomainService : DomainService
{
    private readonly DccDbContext _context;
    private readonly IConfigObjectReleaseRepository _configObjectReleaseRepository;
    private readonly IConfigObjectRepository _configObjectRepository;
    private readonly IAppConfigObjectRepository _appConfigObjectRepository;
    private readonly IBizConfigObjectRepository _bizConfigObjectRepository;
    private readonly IBizConfigRepository _bizConfigRepository;
    private readonly IPublicConfigObjectRepository _publicConfigObjectRepository;
    private readonly IPublicConfigRepository _publicConfigRepository;
    private readonly IMultilevelCacheClient _memoryCacheClient;
    private readonly IPmClient _pmClient;
    private readonly IMasaStackConfig _masaStackConfig;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    public ConfigObjectDomainService(
        IDomainEventBus eventBus,
        DccDbContext context,
        IConfigObjectReleaseRepository configObjectReleaseRepository,
        IConfigObjectRepository configObjectRepository,
        IAppConfigObjectRepository appConfigObjectRepository,
        IBizConfigObjectRepository bizConfigObjectRepository,
        IBizConfigRepository bizConfigRepository,
        IPublicConfigObjectRepository publicConfigObjectRepository,
        IPublicConfigRepository publicConfigRepository,
        IMultilevelCacheClient memoryCacheClient,
        IPmClient pmClient,
        IMasaStackConfig masaStackConfig,
        IUnitOfWork unitOfWork) : base(eventBus)
    {
        _context = context;
        _configObjectReleaseRepository = configObjectReleaseRepository;
        _configObjectRepository = configObjectRepository;
        _appConfigObjectRepository = appConfigObjectRepository;
        _bizConfigObjectRepository = bizConfigObjectRepository;
        _bizConfigRepository = bizConfigRepository;
        _publicConfigObjectRepository = publicConfigObjectRepository;
        _publicConfigRepository = publicConfigRepository;
        _memoryCacheClient = memoryCacheClient;
        _pmClient = pmClient;
        _masaStackConfig = masaStackConfig;
        _unitOfWork = unitOfWork;
    }

    public async Task AddConfigObjectAsync(List<AddConfigObjectDto> configObjectDtos)
    {
        List<ConfigObject> configObjects = new();
        foreach (var configObjectDto in configObjectDtos)
        {
            var configObject = new ConfigObject(
                configObjectDto.Name,
                configObjectDto.FormatLabelCode,
                configObjectDto.Type,
                configObjectDto.Content,
                configObjectDto.TempContent,
                configObjectDto.RelationConfigObjectId,
                configObjectDto.FromRelation,
                configObjectDto.Encryption);

            configObjects.Add(configObject);

            if (configObjectDto.Type == ConfigObjectType.Public)
            {
                configObject.SetPublicConfigObject(configObjectDto.ObjectId, configObjectDto.EnvironmentClusterId);
            }
            else if (configObjectDto.Type == ConfigObjectType.App)
            {
                configObject.SetAppConfigObject(configObjectDto.ObjectId, configObjectDto.EnvironmentClusterId);
            }
            else if (configObjectDto.Type == ConfigObjectType.Biz)
            {
                configObject.SetBizConfigObject(configObjectDto.ObjectId, configObjectDto.EnvironmentClusterId);
            }
        }

        await _configObjectRepository.AddRangeAsync(configObjects);
    }

    public async Task RemoveConfigObjectAsync(RemoveConfigObjectDto dto)
    {
        var configObjectEntity = await _configObjectRepository.FindAsync(p => p.Id == dto.ConfigObjectId)
            ?? throw new UserFriendlyException("Config object does not exist");

        await _configObjectRepository.RemoveAsync(configObjectEntity);

        var key = $"{dto.EnvironmentName}-{dto.ClusterName}-{dto.AppId}-{configObjectEntity.Name}";
        await _memoryCacheClient.RemoveAsync<PublishReleaseModel>(key.ToLower());
    }

    public async Task UpdateConfigObjectContentAsync(UpdateConfigObjectContentDto dto)
    {
        var configObject = await _configObjectRepository.FindAsync(configObject => configObject.Id == dto.ConfigObjectId)
            ?? throw new UserFriendlyException("Config object does not exist");

        if (dto.FormatLabelCode.Trim().ToLower() != "properties")
        {
            string content = dto.Content;
            if (configObject.Encryption)
            {
                content = EncryptContent(dto.Content);
            }

            configObject.UpdateContent(content);
            configObject.UnRelation();
        }
        else
        {
            string content = configObject.Content;
            if (configObject.Encryption && configObject.Content != "[]")
            {
                content = DecryptContent(configObject.Content);
            }

            var propertyEntities = JsonSerializer.Deserialize<List<ConfigObjectPropertyContentDto>>(content) ?? new();
            if (dto.AddConfigObjectPropertyContent.Any())
            {
                propertyEntities.AddRange(dto.AddConfigObjectPropertyContent);
            }
            if (dto.DeleteConfigObjectPropertyContent.Any())
            {
                propertyEntities.RemoveAll(prop => dto.DeleteConfigObjectPropertyContent.Select(prop => prop.Key).Contains(prop.Key));
            }
            if (dto.EditConfigObjectPropertyContent.Any())
            {
                propertyEntities.RemoveAll(prop => dto.EditConfigObjectPropertyContent.Select(prop => prop.Key).Contains(prop.Key));
                propertyEntities.AddRange(dto.EditConfigObjectPropertyContent);
            }

            content = JsonSerializer.Serialize(propertyEntities, _jsonSerializerOptions);
            if (configObject.Encryption)
            {
                content = EncryptContent(content);
            }
            configObject.UpdateContent(content);
        }

        await _configObjectRepository.UpdateAsync(configObject);
    }

    private string EncryptContent(string content)
    {
        var secret = _masaStackConfig.DccSecret;

        var encryptContent = AesUtils.Encrypt(content, secret, FillType.Left);
        return encryptContent;
    }

    private string DecryptContent(string content)
    {
        var secret = _masaStackConfig.DccSecret;

        var encryptContent = AesUtils.Decrypt(content, secret, FillType.Left);
        return encryptContent;
    }

    public async Task CloneConfigObjectAsync(CloneConfigObjectDto dto)
    {
        //add
        await CheckConfigObjectDuplication(dto.ConfigObjects, dto.ToObjectId);
        await CloneConfigObjectsAsync(dto.ConfigObjects, dto.ToObjectId);

        //update
        var envClusterIds = dto.CoverConfigObjects.Select(c => c.EnvironmentClusterId);
        var configNames = dto.CoverConfigObjects.Select(c => c.Name).Distinct().ToList();

        IEnumerable<ConfigObject> needEditConfig = new List<ConfigObject>();
        if (dto.ConfigObjectType == ConfigObjectType.App)
        {
            var appConfigObjects = await _appConfigObjectRepository.GetListAsync(
                app => app.AppId == dto.ToObjectId && envClusterIds.Contains(app.EnvironmentClusterId));
            needEditConfig = await _configObjectRepository.GetListAsync(c => appConfigObjects.Select(app => app.ConfigObjectId).Contains(c.Id));
        }
        else if (dto.ConfigObjectType == ConfigObjectType.Biz)
        {
            var bizConfigObjects = await _bizConfigObjectRepository.GetListAsync(
                biz => biz.BizConfigId == dto.ToObjectId && envClusterIds.Contains(biz.EnvironmentClusterId));
            needEditConfig = await _configObjectRepository.GetListAsync(c => bizConfigObjects.Select(biz => biz.ConfigObjectId).Contains(c.Id) && configNames.Contains(c.Name));
        }
        else if (dto.ConfigObjectType == ConfigObjectType.Public)
        {
            var publicConfigObjects = await _publicConfigObjectRepository.GetListAsync(
                publicConfig => publicConfig.PublicConfigId == dto.ToObjectId && envClusterIds.Contains(publicConfig.EnvironmentClusterId));
            var ss = publicConfigObjects.Select(publicConfig => publicConfig.ConfigObjectId);
            needEditConfig = await _configObjectRepository.GetListAsync(c => ss.Contains(c.Id) && configNames.Contains(c.Name));
        }

        if (needEditConfig != null && needEditConfig.Any() && configNames.Any())
        {
            needEditConfig = needEditConfig.Where(item => configNames.Any(name => string.Equals(name, item.Name, StringComparison.CurrentCultureIgnoreCase))).ToList();
        }

        foreach (var editConfig in needEditConfig)
        {
            dto.CoverConfigObjects.ForEach(configObject =>
            {
                if (editConfig.Type == configObject.Type && editConfig.Name.Equals(configObject.Name))
                {
                    string tempontent = editConfig.FormatLabelCode.ToLower() switch
                    {
                        "json" => "{}",
                        "properties" => "[]",
                        _ => "",
                    };
                    editConfig.AddContent(configObject.Content, tempontent);
                }
            });
        }
        await _configObjectRepository.UpdateRangeAsync(needEditConfig);
    }

    private async Task CloneConfigObjectsAsync(List<AddConfigObjectDto> configObjects, int appId)
    {
        List<ConfigObject> cloneConfigObjects = new();
        foreach (var configObjectDto in configObjects)
        {
            var configObject = new ConfigObject(
                configObjectDto.Name,
                configObjectDto.FormatLabelCode,
                configObjectDto.Type,
                configObjectDto.Content,
                configObjectDto.TempContent,
                encryption: configObjectDto.Encryption);
            cloneConfigObjects.Add(configObject);

            if (configObjectDto.Type == ConfigObjectType.Public)
            {
                configObject.SetPublicConfigObject(appId, configObjectDto.EnvironmentClusterId);
            }
            else if (configObjectDto.Type == ConfigObjectType.App)
            {
                configObject.SetAppConfigObject(appId, configObjectDto.EnvironmentClusterId);
            }
            else if (configObjectDto.Type == ConfigObjectType.Biz)
            {
                configObject.SetBizConfigObject(appId, configObjectDto.EnvironmentClusterId);
            }

            if (configObject.Encryption)
            {
                var encryptConten = EncryptContent(configObject.Content);
                configObject.UpdateContent(encryptConten);
            }
        }
        await _configObjectRepository.AddRangeAsync(cloneConfigObjects);
    }

    private async Task CheckConfigObjectDuplication(List<AddConfigObjectDto> configObjects, int appId)
    {
        if (configObjects?.Count > 0)
        {
            var configType = configObjects.First().Type;
            var configObjectNames = configObjects.Select(e => e.Name);
            switch (configType)
            {
                case ConfigObjectType.Public:
                    var allPublicConfigs = await _publicConfigObjectRepository.GetListByPublicConfigIdAsync(appId);
                    foreach (var item in configObjects)
                    {
                        if (allPublicConfigs.Any(e => e.EnvironmentClusterId == item.EnvironmentClusterId && e.ConfigObject.Name == item.Name))
                        {
                            throw new UserFriendlyException($"Configuration Name '{item.Name}' already exist in the environment cluster '{item.EnvironmentClusterId}'.");
                        }
                    }
                    break;
                case ConfigObjectType.Biz:
                    var bizConfigObjects = await _bizConfigObjectRepository.GetListByBizConfigIdAsync(appId);
                    foreach (var item in configObjects)
                    {
                        if (bizConfigObjects.Any(e => e.EnvironmentClusterId == item.EnvironmentClusterId && e.ConfigObject.Name == item.Name))
                        {
                            throw new UserFriendlyException($"Configuration Name '{item.Name}' already exist in the environment cluster '{item.EnvironmentClusterId}'.");
                        }
                    }
                    break;
                case ConfigObjectType.App:
                    var allAppConfigObjects = await _appConfigObjectRepository.GetListByAppIdAsync(appId);
                    foreach (var item in configObjects)
                    {
                        if (allAppConfigObjects.Any(e => e.EnvironmentClusterId == item.EnvironmentClusterId && e.ConfigObject.Name == item.Name))
                        {
                            throw new UserFriendlyException($"Configuration Name '{item.Name}' already exist for environment cluster's '{item.EnvironmentClusterId}'.  ");
                        }
                    }
                    break;
            }
        }
    }

    public async Task AddConfigObjectReleaseAsync(AddConfigObjectReleaseDto dto)
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
        if (relationConfigObjects.Any())
        {
            var allEnvClusters = await _pmClient.ClusterService.GetEnvironmentClustersAsync();
            var appConfigObjects = await _appConfigObjectRepository.GetListAsync(app => relationConfigObjects.Select(c => c.Id).Contains(app.ConfigObjectId));
            var apps = await _pmClient.AppService.GetListAsync();

            foreach (var item in appConfigObjects)
            {
                var envCluster = allEnvClusters.FirstOrDefault(c => c.Id == item.EnvironmentClusterId) ?? new();
                var app = apps.FirstOrDefault(a => a.Id == item.AppId) ?? new();
                var relationConfigObject = relationConfigObjects.First(c => c.Id == item.ConfigObjectId);
                var key = $"{envCluster.EnvironmentName}-{envCluster.ClusterName}-{app.Identity}-{relationConfigObject.Name}";

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

    public async Task RollbackConfigObjectReleaseAsync(RollbackConfigObjectReleaseDto rollbackDto)
    {
        var latestConfigObjectRelease = await _context.Set<ConfigObjectRelease>()
            .Where(cor => cor.ConfigObjectId == rollbackDto.ConfigObjectId)
            .OrderByDescending(cor => cor.Id)
            .FirstOrDefaultAsync();

        var rollbackToEntity = await _configObjectReleaseRepository.FindAsync(
            ocr => ocr.Id == rollbackDto.RollbackToReleaseId);

        if (latestConfigObjectRelease == null || rollbackToEntity == null)
        {
            throw new Exception("要回滚的版本不存在");
        }

        if (rollbackDto.RollbackToReleaseId == latestConfigObjectRelease.FromReleaseId)
        {
            throw new UserFriendlyException("该版本已作废");
        }
        if (rollbackToEntity.Version == latestConfigObjectRelease.Version)
        {
            throw new UserFriendlyException("两个版本相同");
        }

        //rollback
        //add
        await _configObjectReleaseRepository.AddAsync(new ConfigObjectRelease(
                 rollbackToEntity.ConfigObjectId,
                 rollbackToEntity.Name,
                 $"由 {latestConfigObjectRelease.Name} 回滚至 {rollbackToEntity.Name}",
                 rollbackToEntity.Content,
                 rollbackToEntity.Version,
                 latestConfigObjectRelease.Id
             ));

        //Invalid rollback entity
        latestConfigObjectRelease.Invalid();
        await _configObjectReleaseRepository.UpdateAsync(latestConfigObjectRelease);

        //Update ConfigObject entity
        var configObject = (await _configObjectRepository.FindAsync(config => config.Id == rollbackDto.ConfigObjectId))!;
        configObject.AddContent(configObject.Content, rollbackToEntity.Content);
        await _configObjectRepository.UpdateAsync(configObject);

        string key = string.Empty;
        var envClusters = await _pmClient.ClusterService.GetEnvironmentClustersAsync();
        if (configObject.Type == ConfigObjectType.Public)
        {
            var publicConfigObject = await _publicConfigObjectRepository.GetByConfigObjectIdAsync(configObject.Id);
            var publicConfig = await _publicConfigRepository.FindAsync(c => c.Id == publicConfigObject.PublicConfigId) ?? throw new MasaException();
            var envCluster = envClusters.First(e => e.Id == publicConfigObject.EnvironmentClusterId);
            key = $"{envCluster.EnvironmentName}-{envCluster.ClusterName}-{publicConfig.Identity}-{configObject.Name}";
        }
        else if (configObject.Type == ConfigObjectType.Biz)
        {
            var bizConfigObject = await _bizConfigObjectRepository.GetByConfigObjectIdAsync(configObject.Id);
            var bizConfig = await _bizConfigRepository.FindAsync(c => c.Id == bizConfigObject.BizConfigId) ?? throw new MasaException();
            var envCluster = envClusters.First(e => e.Id == bizConfigObject.EnvironmentClusterId);
            key = $"{envCluster.EnvironmentName}-{envCluster.ClusterName}-{bizConfig.Identity}-{configObject.Name}";
        }
        else if (configObject.Type == ConfigObjectType.App)
        {
            var appConfigObject = await _appConfigObjectRepository.GetbyConfigObjectIdAsync(configObject.Id);
            var app = await _pmClient.AppService.GetAsync(appConfigObject.AppId) ?? throw new MasaException(); ;
            var envCluster = envClusters.First(e => e.Id == appConfigObject.EnvironmentClusterId);
            key = $"{envCluster.EnvironmentName}-{envCluster.ClusterName}-{app.Identity}-{configObject.Name}";
        }

        var releaseContent = new PublishReleaseModel
        {
            Content = configObject.Encryption ? EncryptContent(rollbackToEntity.Content) : rollbackToEntity.Content,
            FormatLabelCode = configObject.FormatLabelCode,
            Encryption = configObject.Encryption
        };
        await _memoryCacheClient.SetAsync(key.ToLower(), releaseContent);
    }

    public async Task UpdateConfigObjectAsync(
        string environmentName,
        string clusterName,
        string appId,
        string configObjectName,
        string value)
    {
        var configObjects = await _configObjectRepository.GetConfigObjectsByNameAsync(configObjectName);
        if (configObjects.Count == 0)
        {
            throw new UserFriendlyException("ConfigObject does not exist");
        }

        var environmentClusters = await _pmClient.ClusterService.GetEnvironmentClustersAsync();
        var environmentCluster = environmentClusters.FirstOrDefault(ec => ec.EnvironmentName.ToLower() == environmentName.ToLower() && ec.ClusterName.ToLower() == clusterName.ToLower())
            ?? throw new UserFriendlyException("Environment cluster does not exist");

        ConfigObject? configObject = null;

        var publicConfig = await _publicConfigRepository.FindAsync(p => p.Identity.ToLower() == appId.ToLower());
        if (publicConfig == null)
        {
            var appDetail = await _pmClient.AppService.GetByIdentityAsync(appId);
            var appConfigObjects = await _appConfigObjectRepository.GetListByEnvClusterIdAsync(environmentCluster.Id, appDetail.Id);
            configObject = configObjects.FirstOrDefault(c => appConfigObjects.Select(ac => ac.ConfigObjectId).Contains(c.Id)) ?? throw new UserFriendlyException("ConfigObject does not exist");
        }
        else
        {
            var publicConfigObjects = await _publicConfigObjectRepository.GetListByEnvClusterIdAsync(environmentCluster.Id, publicConfig.Id);
            configObject = configObjects.FirstOrDefault(c => publicConfigObjects.Select(ac => ac.ConfigObjectId).Contains(c.Id)) ?? throw new UserFriendlyException("ConfigObject does not exist");
        }

        if (configObject.Encryption)
        {
            value = EncryptContent(value);
        }
        configObject.UpdateContent(value);
        await _configObjectRepository.UpdateAsync(configObject);

        var releaseModel = new AddConfigObjectReleaseDto
        {
            Type = ReleaseType.MainRelease,
            ConfigObjectId = configObject.Id,
            Name = "通过Sdk发布",
            EnvironmentName = environmentName,
            ClusterName = clusterName,
            Identity = appId,
            Content = value
        };

        await AddConfigObjectReleaseAsync(releaseModel);
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
            if (publicConfig != null)
            {
                newConfigObject.SetConfigObjectType(ConfigObjectType.App);
                newConfigObject.SetPublicConfigObject(publicConfig.Id, cluster.EnvironmentClusterId);
            }
            else
            {
                var bizConfig = await _bizConfigRepository.FindAsync(bizConfig => bizConfig.Identity == appId);
                if (bizConfig != null)
                {
                    newConfigObject.SetConfigObjectType(ConfigObjectType.Biz);
                    newConfigObject.SetBizConfigObject(bizConfig.Id, cluster.EnvironmentClusterId);
                }
                else
                {
                    newConfigObject.SetConfigObjectType(ConfigObjectType.App);
                    var app = await _pmClient.AppService.GetByIdentityAsync(appId);
                    if (app != null)
                    {
                        newConfigObject.SetAppConfigObject(app.Id, cluster.EnvironmentClusterId);
                    }
                    else
                    {
                        throw new UserFriendlyException("AppId Error");
                    }
                }
            }
            var objectId = await _configObjectRepository.GetIdAsync(newConfigObject.Name, newConfigObject.Type);
            if (objectId == 0)
            {
                await _configObjectRepository.AddAsync(newConfigObject);
                await _unitOfWork.SaveChangesAsync();
                objectId = newConfigObject.Id;
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

    public async Task<string> RefreshConfigObjectToRedisAsync()
    {
        var configObjectInfo = await _configObjectRepository.GetNewestConfigObjectReleaseWithAppInfo();
        var apps = await _pmClient.AppService.GetListAsync();
        var envClusters = await _pmClient.ClusterService.GetEnvironmentClustersAsync();
        var publicConfig = (await _publicConfigRepository.GetListAsync()).FirstOrDefault()
            ?? throw new UserFriendlyException("PublicConfig is null");

        configObjectInfo.ForEach(config =>
        {
            if (config.ConfigObject.Type == ConfigObjectType.App)
            {
                apps.Where(app => app.Id == config.ObjectId).ToList().ForEach(app =>
                {
                    app.EnvironmentClusters.ForEach(async envCluster =>
                    {
                        if (envCluster.Id == config.EnvironmentClusterId)
                        {
                            var key = $"{envCluster.EnvironmentName}-{envCluster.ClusterName}-{app.Identity}-{config.ConfigObject.Name}";
                            await _memoryCacheClient.SetAsync(key.ToLower(), new PublishReleaseModel
                            {
                                Content = config.ConfigObject.Content,
                                FormatLabelCode = config.ConfigObject.FormatLabelCode,
                                Encryption = config.ConfigObject.Encryption
                            });
                        }
                    });
                });
            }
            else if (config.ConfigObject.Type == ConfigObjectType.Public)
            {
                envClusters.Where(ec => ec.Id == config.EnvironmentClusterId).ToList().ForEach(async envCluster =>
                {
                    var key = $"{envCluster.EnvironmentName}-{envCluster.ClusterName}-{publicConfig.Identity}-{config.ConfigObject.Name}";
                    await _memoryCacheClient.SetAsync(key.ToLower(), new PublishReleaseModel
                    {
                        Content = config.ConfigObject.Content,
                        FormatLabelCode = config.ConfigObject.FormatLabelCode,
                        Encryption = config.ConfigObject.Encryption
                    });
                });
            }
        });

        return "success";
    }

    public async Task<Dictionary<string, PublishReleaseModel>> GetConfigObjectsAsync(string environmentName,
        string clusterName, string appId, List<string>? configObjects)
    {
        var resultDic = new Dictionary<string, PublishReleaseModel>();

        var envs = await _pmClient.EnvironmentService.GetListAsync();
        var env = envs.FirstOrDefault(e => e.Name.ToLower() == environmentName.ToLower()) ?? throw new UserFriendlyException("Environment does not exist");
        var clusters = await _pmClient.ClusterService.GetListByEnvIdAsync(env.Id);
        var cluster = clusters.FirstOrDefault(c => c.Name.ToLower() == clusterName.ToLower()) ?? throw new UserFriendlyException("Cluster does not exist");
        var apps = await _pmClient.AppService.GetListAsync();
        var app = apps.FirstOrDefault(apps => apps.Identity.ToLower() == appId.ToLower()) ?? throw new UserFriendlyException("AppId does not exist");

        Expression<Func<ConfigObject, bool>> configObjectFilter = configObject =>
                    configObject.AppConfigObject.EnvironmentClusterId == cluster.Id &&
                    configObject.AppConfigObject.AppId == app.Id;
        var configObjectList = await _configObjectRepository.GetListAsync(configObjectFilter);
        if (configObjectList != null && configObjectList.Any() && configObjects != null && configObjects.Count > 0)
        {
            configObjectList = configObjectList.Where(configObject => configObjects.Contains(configObject.Name, StringComparer.OrdinalIgnoreCase)).ToList();
        }
        if (configObjects == null || configObjects.Count == 0)
        {
            var configObjectPublicList = await _configObjectRepository.GetListAsync(configObject => configObject.PublicConfigObject.EnvironmentClusterId == cluster.Id);
            configObjectList = configObjectList.Union(configObjectPublicList);
        }

        foreach (var configObject in configObjectList)
        {
            var key = $"{configObject.Name}".ToLower();//{environmentName}-{clusterName}-{appId}-
            if (resultDic.ContainsKey(key)) continue;
            resultDic.Add(key, new PublishReleaseModel()
            {
                Encryption = configObject.Encryption,
                FormatLabelCode = configObject.FormatLabelCode,
                Content = configObject.Content,
            });
        }

        return resultDic;
    }
}
