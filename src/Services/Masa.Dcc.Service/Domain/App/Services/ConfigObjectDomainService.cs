// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Services
{
    public class ConfigObjectDomainService : DomainService
    {
        private readonly DccDbContext _context;
        private readonly IConfigObjectReleaseRepository _configObjectReleaseRepository;
        private readonly IConfigObjectRepository _configObjectRepository;
        private readonly IAppConfigObjectRepository _appConfigObjectRepository;
        private readonly IMemoryCacheClient _memoryCacheClient;
        private readonly IPmClient _pmClient;
        private readonly DaprClient _daprClient;

        public ConfigObjectDomainService(
            IDomainEventBus eventBus,
            DccDbContext context,
            IConfigObjectReleaseRepository configObjectReleaseRepository,
            IConfigObjectRepository configObjectRepository,
            IAppConfigObjectRepository appConfigObjectRepository,
            IMemoryCacheClient memoryCacheClient,
            IPmClient pmClient,
            DaprClient daprClient) : base(eventBus)
        {
            _context = context;
            _configObjectReleaseRepository = configObjectReleaseRepository;
            _configObjectRepository = configObjectRepository;
            _appConfigObjectRepository = appConfigObjectRepository;
            _memoryCacheClient = memoryCacheClient;
            _pmClient = pmClient;
            _daprClient = daprClient;
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

        public async Task RemoveConfigObjectAsync(int configObjectId)
        {
            var configObjectEntity = await _configObjectRepository.FindAsync(p => p.Id == configObjectId)
                ?? throw new UserFriendlyException("Config object does not exist");

            await _configObjectRepository.RemoveAsync(configObjectEntity);
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
                    content = await EncryptContentAsync(dto.Content);
                }

                configObject.UpdateContent(content);
                configObject.UnRelation();
            }
            else
            {
                string content = configObject.Content;
                if (configObject.Encryption && configObject.Content != "[]")
                {
                    content = await DecryptContentAsync(configObject.Content);
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

                content = JsonSerializer.Serialize(propertyEntities);
                if (configObject.Encryption)
                {
                    content = await EncryptContentAsync(content);
                }
                configObject.UpdateContent(content);
            }

            await _configObjectRepository.UpdateAsync(configObject);
        }

        private async Task<string> EncryptContentAsync(string content)
        {
            var config = await _daprClient.GetSecretAsync("local-secret-store", "dcc-config");
            var secret = config["dcc-config-secret"];

            var encryptContent = AesUtils.Encrypt(content, secret, FillType.Left);
            return encryptContent;
        }

        private async Task<string> DecryptContentAsync(string content)
        {
            var config = await _daprClient.GetSecretAsync("local-secret-store", "dcc-config");
            var secret = config["dcc-config-secret"];

            var encryptContent = AesUtils.Decrypt(content, secret, FillType.Left);
            return encryptContent;
        }

        public async Task CloneConfigObjectAsync(CloneConfigObjectDto dto)
        {
            //add
            await CloneConfigObjectsAsync(dto.ConfigObjects, dto.ToObjectId);

            //update
            var envClusterIds = dto.CoverConfigObjects.Select(c => c.EnvironmentClusterId);
            var appConfigObjects = await _appConfigObjectRepository.GetListAsync(
                app => app.AppId == dto.ToObjectId && envClusterIds.Contains(app.EnvironmentClusterId));
            var needRemove = await _configObjectRepository.GetListAsync(c => appConfigObjects.Select(app => app.ConfigObjectId).Contains(c.Id));
            await _configObjectRepository.RemoveRangeAsync(needRemove);
            await CloneConfigObjectsAsync(dto.CoverConfigObjects, dto.ToObjectId);
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
                    configObjectDto.TempContent);
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
            }
            await _configObjectRepository.AddRangeAsync(cloneConfigObjects);
        }

        public async Task AddConfigObjectReleaseAsync(AddConfigObjectReleaseDto dto)
        {
            var configObject = (await _configObjectRepository.FindAsync(
                configObject => configObject.Id == dto.ConfigObjectId)) ?? throw new Exception("Config object does not exist");

            configObject.AddContent(configObject.Content, configObject.Content);
            await _configObjectRepository.UpdateAsync(configObject);

            await _configObjectReleaseRepository.AddAsync(new ConfigObjectRelease(
                   dto.ConfigObjectId,
                   dto.Name,
                   dto.Comment,
                   configObject.Content)
               );

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
                            await _memoryCacheClient.SetAsync<PublishReleaseDto>(key.ToLower(), new PublishReleaseDto
                            {
                                ConfigObjectType = configObject.Type,
                                Content = dto.Content,
                                FormatLabelCode = configObject.FormatLabelCode
                            });
                        }
                        else
                        {
                            //compare
                            var publicContents = JsonSerializer.Deserialize<List<ConfigObjectPropertyContentDto>>(dto.Content) ?? new();
                            var appContents = JsonSerializer.Deserialize<List<ConfigObjectPropertyContentDto>>(appRelease.Content) ?? new();

                            var exceptContent = publicContents.ExceptBy(appContents.Select(c => c.Key), content => content.Key).ToList();
                            var content = appContents.Union(exceptContent).ToList();

                            var releaseContent = JsonSerializer.Serialize(new PublishReleaseDto
                            {
                                ConfigObjectType = configObject.Type,
                                Content = JsonSerializer.Serialize(content),
                                FormatLabelCode = configObject.FormatLabelCode
                            });
                            await _memoryCacheClient.SetAsync<string>(key.ToLower(), releaseContent);
                        }
                    }
                    else
                    {
                        var releaseContent = JsonSerializer.Serialize(new PublishReleaseDto
                        {
                            ConfigObjectType = configObject.Type,
                            Content = dto.Content,
                            FormatLabelCode = configObject.FormatLabelCode
                        });
                        await _memoryCacheClient.SetAsync<string>(key.ToLower(), releaseContent);
                    }
                }
            }
            else
            {
                //add redis cache
                //TODO: encryption value
                var key = $"{dto.EnvironmentName}-{dto.ClusterName}-{dto.Identity}-{configObject.Name}";
                if (configObject.Encryption)
                {
                    dto.Content = await EncryptContentAsync(dto.Content);
                }
                var releaseContent = JsonSerializer.Serialize(new PublishReleaseDto
                {
                    ConfigObjectType = configObject.Type,
                    Content = dto.Content,
                    FormatLabelCode = configObject.FormatLabelCode,
                    Encryption = configObject.Encryption
                });
                await _memoryCacheClient.SetAsync<string>(key.ToLower(), releaseContent);
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
        }

        public async Task UpdateConfigObjectAsync(string environmentName, string clusterName, string appId,
            string configObjectName,
            string value)
        {
            var configObject = await _configObjectRepository.FindAsync(config => config.Name == configObjectName);
            if (configObject == null)
                throw new UserFriendlyException("ConfigObject does not exist");

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

        public async Task InitConfigObjectAsync(string environmentName, string clusterName, string appId, Dictionary<string, string> configObjects)
        {
            var envs = await _pmClient.EnvironmentService.GetListAsync();
            var env = envs.FirstOrDefault(e => e.Name.ToLower() == environmentName.ToLower());
            if (env == null)
                throw new UserFriendlyException("Environment does not exist");

            var clusters = await _pmClient.ClusterService.GetListByEnvIdAsync(env.Id);
            var cluster = clusters.FirstOrDefault(c => c.Name.ToLower() == clusterName.ToLower());
            if (cluster == null)
                throw new UserFriendlyException("Cluster does not exist");

            var app = await _pmClient.AppService.GetByIdentityAsync(appId);

            foreach (var configObject in configObjects)
            {
                var newConfigObject = new ConfigObject(
                    configObject.Key,
                    "Json",
                    ConfigObjectType.App,
                    configObject.Value,
                    "{}");

                newConfigObject.SetAppConfigObject(app.Id, cluster.EnvironmentClusterId);

                await _configObjectRepository.AddAsync(newConfigObject);
                await _configObjectRepository.UnitOfWork.SaveChangesAsync();

                var releaseModel = new AddConfigObjectReleaseDto
                {
                    Type = ReleaseType.MainRelease,
                    ConfigObjectId = newConfigObject.Id,
                    Name = "通过Sdk发布",
                    EnvironmentName = environmentName,
                    ClusterName = clusterName,
                    Identity = appId,
                    Content = configObject.Value,
                };

                await AddConfigObjectReleaseAsync(releaseModel);
            }
        }
    }
}
