// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Services
{
    public class ConfigObjectDomainService : DomainService
    {
        private readonly DccDbContext _context;
        private readonly IConfigObjectReleaseRepository _configObjectReleaseRepository;
        private readonly IConfigObjectRepository _configObjectRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly IAppConfigObjectRepository _appConfigObjectRepository;
        private readonly IMemoryCacheClient _memoryCacheClient;
        private readonly IPmClient _pmClient;

        public ConfigObjectDomainService(
            IDomainEventBus eventBus,
            DccDbContext context,
            IConfigObjectReleaseRepository configObjectReleaseRepository,
            IConfigObjectRepository configObjectRepository,
            ILabelRepository labelRepository,
            IAppConfigObjectRepository appConfigObjectRepository,
            IMemoryCacheClient memoryCacheClient,
            IPmClient pmClient) : base(eventBus)
        {
            _context = context;
            _configObjectReleaseRepository = configObjectReleaseRepository;
            _configObjectRepository = configObjectRepository;
            _labelRepository = labelRepository;
            _appConfigObjectRepository = appConfigObjectRepository;
            _memoryCacheClient = memoryCacheClient;
            _pmClient = pmClient;
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
                    configObjectDto.FromRelation);

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
                configObject.UpdateContent(dto.Content);
                configObject.UnRelation();
            }
            else
            {
                var propertyEntities = JsonSerializer.Deserialize<List<ConfigObjectPropertyContentDto>>(configObject.Content) ?? new();
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

                var content = JsonSerializer.Serialize(propertyEntities);
                configObject.UpdateContent(content);
            }

            await _configObjectRepository.UpdateAsync(configObject);
        }

        public async Task CloneConfigObjectAsync(CloneConfigObjectDto dto)
        {
            //add
            await CloneConfigObjectsAsync(dto.ConfigObjects, dto.ToAppId);

            //update
            var envClusterIds = dto.CoverConfigObjects.Select(c => c.EnvironmentClusterId);
            var appConfigObjects = await _appConfigObjectRepository.GetListAsync(
                app => app.AppId == dto.ToAppId && envClusterIds.Contains(app.EnvironmentClusterId));
            var needRemove = await _configObjectRepository.GetListAsync(c => appConfigObjects.Select(app => app.ConfigObjectId).Contains(c.Id));
            await _configObjectRepository.RemoveRangeAsync(needRemove);
            await CloneConfigObjectsAsync(dto.CoverConfigObjects, dto.ToAppId);
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
                   configObject.Content,
                   null)
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
                var releaseContent = JsonSerializer.Serialize(new PublishReleaseDto
                {
                    ConfigObjectType = configObject.Type,
                    Content = dto.Content,
                    FormatLabelCode = configObject.FormatLabelCode
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

            var canRollbackEntity = await _configObjectReleaseRepository.FindAsync(
                ocr => ocr.Id == rollbackDto.RollbackToReleaseId);

            if (latestConfigObjectRelease == null || canRollbackEntity == null)
            {
                throw new Exception("要回滚的版本不存在");
            }

            if (rollbackDto.RollbackToReleaseId == latestConfigObjectRelease.FromReleaseId)
            {
                throw new UserFriendlyException("该版本已作废");
            }
            if (rollbackDto.RollbackToReleaseId == latestConfigObjectRelease.ToReleaseId)
            {
                throw new UserFriendlyException("两个版本相同");
            }

            //rollback
            //add
            await _configObjectReleaseRepository.AddAsync(new ConfigObjectRelease(
                     canRollbackEntity.ConfigObjectId,
                     canRollbackEntity.Name,
                     $"由 {latestConfigObjectRelease.Name} 回滚至 {canRollbackEntity.Name}",
                     canRollbackEntity.Content,
                     canRollbackEntity.Version,
                     latestConfigObjectRelease.Id
                 ));

            //Invalid rollback entity
            latestConfigObjectRelease.Invalid();
            await _configObjectReleaseRepository.UpdateAsync(latestConfigObjectRelease);

            //Update ConfigObject entity
            var configObject = (await _configObjectRepository.FindAsync(config => config.Id == rollbackDto.ConfigObjectId))!;
            configObject.AddContent(configObject.Content, canRollbackEntity.Content);
            await _configObjectRepository.UpdateAsync(configObject);
        }
    }
}
