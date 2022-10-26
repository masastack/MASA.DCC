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
        private readonly IBizConfigRepository _bizConfigRepository;
        private readonly IBizConfigObjectRepository _bizConfigObjectRepository;
        private readonly IAppConfigObjectRepository _appConfigObjectRepository;
        private readonly DaprClient _daprClient;

        public QueryHandler(
            IPublicConfigRepository publicConfigRepository,
            IPublicConfigObjectRepository publicConfigObjectRepository,
            IConfigObjectRepository configObjectRepository,
            ILabelRepository labelRepository,
            IAppPinRepository appPinRepository,
            IBizConfigRepository bizConfigRepository,
            IBizConfigObjectRepository bizConfigObjectRepository,
            IAppConfigObjectRepository appConfigObjectRepository,
            DaprClient daprClient)
        {
            _publicConfigRepository = publicConfigRepository;
            _publicConfigObjectRepository = publicConfigObjectRepository;
            _configObjectRepository = configObjectRepository;
            _labelRepository = labelRepository;
            _appPinRepository = appPinRepository;
            _bizConfigRepository = bizConfigRepository;
            _bizConfigObjectRepository = bizConfigObjectRepository;
            _appConfigObjectRepository = appConfigObjectRepository;
            _daprClient = daprClient;
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
        public async Task GetBizConfigsAsync(BizConfigsQuery query)
        {
            var result = await _bizConfigRepository.FindAsync(biz => biz.Identity == query.Identity);

            if (result != null)
            {
                query.Result = result.Adapt<BizConfigDto>();
            }
        }

        [EventHandler]
        public async Task GetConfigObjectsAsync(ConfigObjectsQuery query)
        {
            List<ConfigObjectDto> objectConfigObjects = new List<ConfigObjectDto>();
            var labels = await _labelRepository.GetListAsync();
            if (query.Type == ConfigObjectType.Public)
            {
                var data = await _publicConfigObjectRepository.GetListByEnvClusterIdAsync(query.EnvClusterId, query.ObjectId);
                TypeAdapterConfig<PublicConfigObject, ConfigObjectDto>.NewConfig()
                    .Map(dest => dest, src => src.ConfigObject)
                    .Map(dest => dest.Id, src => src.ConfigObjectId)
                    .Map(dest => dest.EnvironmentClusterId, src => src.EnvironmentClusterId);
                objectConfigObjects = TypeAdapter.Adapt<List<PublicConfigObject>, List<ConfigObjectDto>>(data);
            }
            else if (query.Type == ConfigObjectType.Biz)
            {
                var data = await _bizConfigObjectRepository.GetListByEnvClusterIdAsync(query.EnvClusterId, query.ObjectId);
                TypeAdapterConfig<BizConfigObject, ConfigObjectDto>.NewConfig()
                    .Map(dest => dest, src => src.ConfigObject)
                    .Map(dest => dest.Id, src => src.ConfigObjectId)
                    .Map(dest => dest.EnvironmentClusterId, src => src.EnvironmentClusterId);
                objectConfigObjects = TypeAdapter.Adapt<List<BizConfigObject>, List<ConfigObjectDto>>(data);
            }
            else if (query.Type == ConfigObjectType.App)
            {
                var data = await _appConfigObjectRepository.GetListByEnvClusterIdAsync(query.EnvClusterId, query.ObjectId);
                TypeAdapterConfig<AppConfigObject, ConfigObjectDto>.NewConfig()
                    .Map(dest => dest, src => src.ConfigObject)
                    .Map(dest => dest.Id, src => src.ConfigObjectId)
                    .Map(dest => dest.EnvironmentClusterId, src => src.EnvironmentClusterId);
                objectConfigObjects = TypeAdapter.Adapt<List<AppConfigObject>, List<ConfigObjectDto>>(data);
            }

            if (!string.IsNullOrWhiteSpace(query.ConfigObjectName))
            {
                objectConfigObjects = objectConfigObjects
                    .Where(publicConfigObject => publicConfigObject.Name.Contains(query.ConfigObjectName))
                    .ToList();
            }

            query.Result = objectConfigObjects.Select(async configObject => new ConfigObjectDto
            {
                Id = configObject.Id,
                Name = configObject.Name,
                EnvironmentClusterId = configObject.EnvironmentClusterId,
                FormatLabelCode = configObject.FormatLabelCode,
                FormatName = labels.FirstOrDefault(label => label.Code == configObject.FormatLabelCode)?.Name ?? "",
                Type = configObject.Type,
                RelationConfigObjectId = configObject.RelationConfigObjectId,
                FromRelation = configObject.FromRelation,
                Encryption = configObject.Encryption,
                Content = configObject.Encryption ? await DecryptContentAsync(configObject.Content) : configObject.Content,
                TempContent = configObject.Encryption ? await DecryptContentAsync(configObject.TempContent) : configObject.TempContent,
                CreationTime = configObject.CreationTime,
                Creator = configObject.Creator,
                ModificationTime = configObject.ModificationTime,
                Modifier = configObject.Modifier
            }).Select(c => c.Result)
            .ToList();
        }

        private async Task<string> DecryptContentAsync(string content)
        {
            if (!string.IsNullOrEmpty(content) && content != "{}" && content != "[]")
            {
                var config = await _daprClient.GetSecretAsync("localsecretstore", "dcc-config");
                var secret = config["dcc-config-secret"];
                string encryptContent = AesUtils.Decrypt(content, secret, FillType.Left);

                return encryptContent;
            }
            else
            {
                return content;
            }
        }

        [EventHandler]
        public async Task GetConfigObjectsByIdsAsync(ConfigObjectListQuery query)
        {
            var result = await _configObjectRepository.GetListAsync(c => query.Ids.Contains(c.Id));

            query.Result = result.Adapt<List<ConfigObjectDto>>();
        }

        [EventHandler]
        public async Task GetConfigObjectReleaseHistoryAsync(ConfigObjectReleaseQuery query)
        {
            var configObjectReleases = await _configObjectRepository.GetConfigObjectWithReleaseHistoriesAsync(query.ConfigObejctId);

            TypeAdapterConfig<ConfigObject, ConfigObjectWithReleaseHistoryDto>.NewConfig()
                .Map(dest => dest.ConfigObjectReleases, src => src.ConfigObjectRelease)
                .Map(dest => dest.Content, src => src.Encryption ? DecryptContentAsync(configObjectReleases.Content).Result : src.Content)
                .Map(dest => dest.TempContent, src => src.Encryption ? DecryptContentAsync(configObjectReleases.TempContent).Result : src.TempContent);

            query.Result = TypeAdapter.Adapt<ConfigObject, ConfigObjectWithReleaseHistoryDto>(configObjectReleases);
            query.Result.ConfigObjectReleases.ForEach(async config =>
            {
                config.Content = query.Result.Encryption ? await DecryptContentAsync(config.Content) : config.Content;
            });
        }

        [EventHandler]
        public async Task GetAppPinAsync(AppPinQuery query)
        {
            var appPins = await _appPinRepository.GetListAsync(query.AppIds);

            query.Result = appPins.Adapt<List<AppPinDto>>();
        }

        [EventHandler]
        public async Task GetByConfigObjectIdAsync(PublicConfigObjectQuery query)
        {
            var configObject = await _publicConfigObjectRepository.GetByConfigObjectIdAsync(query.ConfigObjectId);

            query.Result = configObject.Adapt<PublicConfigObjectDto>();
        }
    }
}
