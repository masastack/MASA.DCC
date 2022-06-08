﻿// Copyright (c) MASA Stack All rights reserved.
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

        public ConfigObjectDomainService(
            IDomainEventBus eventBus,
            DccDbContext context,
            IConfigObjectReleaseRepository configObjectReleaseRepository,
            IConfigObjectRepository configObjectRepository,
            ILabelRepository labelRepository,
            IAppConfigObjectRepository appConfigObjectRepository,
            IMemoryCacheClient memoryCacheClient) : base(eventBus)
        {
            _context = context;
            _configObjectReleaseRepository = configObjectReleaseRepository;
            _configObjectRepository = configObjectRepository;
            _labelRepository = labelRepository;
            _appConfigObjectRepository = appConfigObjectRepository;
            _memoryCacheClient = memoryCacheClient;
        }

        public async Task UpdateConfigObjectContentAsync(UpdateConfigObjectContentDto dto)
        {
            var configObject = await _configObjectRepository.FindAsync(configObject => configObject.Id == dto.ConfigObjectId)
                ?? throw new UserFriendlyException("Config object does not exist");

            if (dto.FormatLabelCode.Trim().ToLower() != "properties")
            {
                configObject.UpdateContent(dto.Content);
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
            List<ConfigObject> cloneConfigObjects = new();
            foreach (var configObjectDto in dto.ConfigObjects)
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
                    configObject.SetPublicConfigObject(dto.ToAppId, configObjectDto.EnvironmentClusterId);
                }
                else if (configObjectDto.Type == ConfigObjectType.App)
                {
                    configObject.SetAppConfigObject(dto.ToAppId, configObjectDto.EnvironmentClusterId);
                }
                else if (configObjectDto.Type == ConfigObjectType.Biz)
                {
                    configObject.SetBizConfigObject(dto.ToAppId, configObjectDto.EnvironmentClusterId);
                }
            }

            await _configObjectRepository.AddRangeAsync(cloneConfigObjects);
        }

        public async Task AddConfigObjectRelease(AddConfigObjectReleaseDto dto)
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

            //add redis cache
            //TODO: encryption value
            var key = $"{dto.EnvironmentName}-{dto.ClusterName}-{dto.Identity}-{configObject.Name}";
            await _memoryCacheClient.SetAsync<PublishReleaseDto>(key.ToLower(), new PublishReleaseDto
            {
                ConfigObjectType = configObject.Type,
                Content = configObject.Content,
                FormatLabelName = (await _labelRepository.FindAsync(label => label.Code == configObject.FormatLabelCode))?.Name ?? ""
            });
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
