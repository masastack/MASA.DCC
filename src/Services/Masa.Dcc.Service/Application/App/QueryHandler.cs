// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.App
{
    public class QueryHandler
    {
        private readonly IPublicConfigRepository _publicConfigRepository;
        private readonly IPublicConfigObjectRepository _publicConfigObjectRepository;
        private readonly IConfigObjectRepository _configObjectRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly IAppPinRepository _appPinRepository;

        public QueryHandler(
            IPublicConfigRepository publicConfigRepository,
            IPublicConfigObjectRepository publicConfigObjectRepository,
            IConfigObjectRepository configObjectRepository,
            ILabelRepository labelRepository,
            IAppPinRepository appPinRepository)
        {
            _publicConfigRepository = publicConfigRepository;
            _publicConfigObjectRepository = publicConfigObjectRepository;
            _configObjectRepository = configObjectRepository;
            _labelRepository = labelRepository;
            _appPinRepository = appPinRepository;
        }

        [EventHandler]
        public async Task GetPublicConfigsAsync(PublicConfigsQuery query)
        {
            IEnumerable<PublicConfig> result = await _publicConfigRepository.GetListAsync();
            query.Result = result.Select(p => new PublicConfigDto
            {
                Id = p.Id,
                Name = p.Name,
                Identity = p.Identity,
                Description = p.Description,
                CreationTime = p.CreationTime,
                Creator = p.Creator,
                Modifier = p.Modifier,
                ModificationTime = p.ModificationTime
            }).ToList();
        }

        [EventHandler]
        public async Task GetConfigObjectsAsync(ConfigObjectsQuery query)
        {
            var publicConfigObjects = await _publicConfigObjectRepository.GetListByEnvClusterIdAsync(query.EnvClusterId);
            if (!string.IsNullOrWhiteSpace(query.ConfigObjectName))
            {
                publicConfigObjects = publicConfigObjects
                    .Where(publicConfigObject => publicConfigObject.ConfigObject.Name.Contains(query.ConfigObjectName))
                    .ToList();
            }
            var labels = await _labelRepository.GetListAsync();
            query.Result = publicConfigObjects.Select(publicConfigObject => new ConfigObjectDto
            {
                Name = publicConfigObject.ConfigObject.Name,
                FormatName = labels.FirstOrDefault(label => label.Id == publicConfigObject.ConfigObject.FormatLabelId)?.Name ?? "",
                Type = publicConfigObject.ConfigObject.Type,
                RelationConfigObjectId = publicConfigObject.ConfigObject.RelationConfigObjectId,
                Content = publicConfigObject.ConfigObject.Content,
                TempContent = publicConfigObject.ConfigObject.TempContent,
                CreationTime = publicConfigObject.ConfigObject.CreationTime,
                Creator = publicConfigObject.ConfigObject.Creator,
                ModificationTime = publicConfigObject.ConfigObject.ModificationTime,
                Modifier = publicConfigObject.ConfigObject.Modifier
            }).ToList();
        }

        [EventHandler]
        public async Task GetConfigObjectReleaseHistoryAsync(ConfigObjectReleaseQuery query)
        {
            var configObjectReleases = await _configObjectRepository.GetConfigObjectWhitReleaseHistoriesAsync(query.ConfigObejctId);

            TypeAdapterConfig<ConfigObject, ConfigObjectWithReleaseHistoryDto>.NewConfig()
                .Map(dest => dest.ConfigObjectReleases, src => src.ConfigObjectRelease);

            query.Result = TypeAdapter.Adapt<ConfigObject, ConfigObjectWithReleaseHistoryDto>(configObjectReleases);
        }

        public async Task GetAppPinAsync(AppPinQuery query)
        {
            var appPins = await _appPinRepository.GetListAsync();

            query.Result = appPins.Adapt<List<AppPinDto>>();
        }
    }
}
