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
        private readonly IMasaStackConfig _masaStackConfig;
        private readonly ConfigObjectDomainService _configObjectDomainService;
        private readonly IConfigurationApiClient _configurationApiClient;

        public QueryHandler(
            IPublicConfigRepository publicConfigRepository,
            IPublicConfigObjectRepository publicConfigObjectRepository,
            IConfigObjectRepository configObjectRepository,
            ILabelRepository labelRepository,
            IAppPinRepository appPinRepository,
            IBizConfigRepository bizConfigRepository,
            IBizConfigObjectRepository bizConfigObjectRepository,
            IAppConfigObjectRepository appConfigObjectRepository,
            IMasaStackConfig masaStackConfig,
            ConfigObjectDomainService configObjectDomainService,
            IConfigurationApiClient configurationApiClient)
        {
            _publicConfigRepository = publicConfigRepository;
            _publicConfigObjectRepository = publicConfigObjectRepository;
            _configObjectRepository = configObjectRepository;
            _labelRepository = labelRepository;
            _appPinRepository = appPinRepository;
            _bizConfigRepository = bizConfigRepository;
            _bizConfigObjectRepository = bizConfigObjectRepository;
            _appConfigObjectRepository = appConfigObjectRepository;
            _masaStackConfig = masaStackConfig;
            _configObjectDomainService = configObjectDomainService;
            _configurationApiClient = configurationApiClient;
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
        public async Task GetLatestReleaseConfigByProjectAsync(
            ProjectLatestReleaseQuery query)
        {
            var dbResult =
                await _bizConfigObjectRepository.GetProjectLatestReleaseConfigAsync(query.Projects,
                    query.EnvClusterId);


            TypeAdapterConfig<(ConfigObjectRelease Release, int ProjectId), LatestReleaseConfigModel>.NewConfig()
                .Map(dest => dest.ConfigObjectId, src => src.Release.ConfigObjectId)
                .Map(dest => dest.ProjectId, src => src.ProjectId)
                .Map(dest => dest.LastPublishTime, src => src.Release.CreationTime)
                .Map(dest => dest.LastPublisherId, src => src.Release.Creator)
                .IgnoreNullValues(true)
                .IgnoreNonMapped(true);

            query.Result = dbResult
                .Adapt<List<(ConfigObjectRelease Release, int ProjectId)>, List<LatestReleaseConfigModel>>();

        }

        [EventHandler]
        public async Task GetLatestReleaseConfigByAppAsync(
            AppLatestReleaseQuery query)
        {
            var dbResult =
                await _appConfigObjectRepository.GetAppLatestReleaseConfigAsync(query.AppIds,
                    query.EnvClusterId);

            TypeAdapterConfig<(int AppId, ConfigObjectRelease Release), LatestReleaseConfigModel>.NewConfig()
                .Map(dest => dest.ConfigObjectId, src => src.Release.ConfigObjectId)
                .Map(dest => dest.AppId, src => src.AppId)
                .Map(dest => dest.LastPublishTime, src => src.Release.CreationTime)
                .Map(dest => dest.LastPublisherId, src => src.Release.Creator)
                .IgnoreNullValues(true)
                .IgnoreNonMapped(true);

            query.Result =
                dbResult.Adapt<List<(int AppId, ConfigObjectRelease Release)>, List<LatestReleaseConfigModel>>();
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
            List<ConfigObjectDto> objectConfigObjects = new();
            var labels = await _labelRepository.GetListAsync();
            if (query.Type == ConfigObjectType.Public)
            {
                var data = await _publicConfigObjectRepository.GetListByEnvClusterIdAsync(query.EnvClusterId, query.ObjectId, query.GetLatestRelease);
                TypeAdapterConfig<PublicConfigObject, ConfigObjectDto>.NewConfig()
                    .Map(dest => dest, src => src.ConfigObject)
                    .Map(dest => dest.Id, src => src.ConfigObjectId)
                    .Map(dest => dest.EnvironmentClusterId, src => src.EnvironmentClusterId)
                    .AfterMapping((src, dest) =>
                    {
                        SetDest(dest, src.ConfigObject);
                    });
                objectConfigObjects = data.Adapt<List<PublicConfigObject>, List<ConfigObjectDto>>();
            }
            else if (query.Type == ConfigObjectType.Biz)
            {
                var data = await _bizConfigObjectRepository.GetListByEnvClusterIdAsync(query.EnvClusterId, query.ObjectId, query.GetLatestRelease);
                TypeAdapterConfig<BizConfigObject, ConfigObjectDto>.NewConfig()
                    .Map(dest => dest, src => src.ConfigObject)
                    .Map(dest => dest.Id, src => src.ConfigObjectId)
                    .Map(dest => dest.EnvironmentClusterId, src => src.EnvironmentClusterId)
                    .AfterMapping((src, dest) =>
                    {
                        SetDest(dest, src.ConfigObject);
                    });
                objectConfigObjects = data.Adapt<List<BizConfigObject>, List<ConfigObjectDto>>();
            }
            else if (query.Type == ConfigObjectType.App)
            {
                var data = await _appConfigObjectRepository.GetListByEnvClusterIdAsync(query.EnvClusterId, query.ObjectId, query.GetLatestRelease);
                TypeAdapterConfig<AppConfigObject, ConfigObjectDto>.NewConfig()
                    .Map(dest => dest, src => src.ConfigObject)
                    .Map(dest => dest.Id, src => src.ConfigObjectId)
                    .Map(dest => dest.EnvironmentClusterId, src => src.EnvironmentClusterId)
                    .AfterMapping((src, dest) =>
                    {
                        SetDest(dest, src.ConfigObject);
                    });
                objectConfigObjects = data.Adapt<List<AppConfigObject>, List<ConfigObjectDto>>();
            }

            void SetDest(ConfigObjectDto dto, ConfigObject configObject)
            {
                if (query.GetLatestRelease)
                {
                    var release = configObject.ConfigObjectRelease?.MaxBy(x => x.CreationTime);
                    if (release != null)
                    {
                        dto.LatestRelease = new()
                        {
                            LastPublishTime = release.CreationTime,
                            LastPublisherId = release.Creator,
                            ConfigObjectId = release.ConfigObjectId,
                        };
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(query.ConfigObjectName))
            {
                objectConfigObjects = objectConfigObjects
                    .Where(publicConfigObject => publicConfigObject.Name.Contains(query.ConfigObjectName))
                    .ToList();
            }

            query.Result = objectConfigObjects.Select(configObject => new ConfigObjectDto
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
                Content = configObject.Encryption ? DecryptContent(configObject.Content) : configObject.Content,
                TempContent = configObject.Encryption ? DecryptContent(configObject.TempContent) : configObject.TempContent,
                CreationTime = configObject.CreationTime,
                Creator = configObject.Creator,
                ModificationTime = configObject.ModificationTime,
                Modifier = configObject.Modifier,
                LatestRelease = configObject.LatestRelease
            }).ToList();
        }

        private string DecryptContent(string content)
        {
            if (!string.IsNullOrEmpty(content) && content != "{}" && content != "[]")
            {
                var secret = _masaStackConfig.DccSecret;
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

            TypeAdapterConfig<ConfigObject, ConfigObjectDto>.NewConfig()
                .Map(dest => dest.Content, src => src.Encryption ? DecryptContent(src.Content) : src.Content)
                .Map(dest => dest.TempContent, src => src.Encryption ? DecryptContent(src.TempContent) : src.TempContent);

            query.Result = result.ToList().Adapt<List<ConfigObject>, List<ConfigObjectDto>>();
        }

        [EventHandler]
        public async Task GetConfigObjectReleaseHistoryAsync(ConfigObjectReleaseQuery query)
        {
            var configObjectReleases = await _configObjectRepository.GetConfigObjectWithReleaseHistoriesAsync(query.ConfigObjectId);

            TypeAdapterConfig<ConfigObject, ConfigObjectWithReleaseHistoryDto>.NewConfig()
                .Map(dest => dest.ConfigObjectReleases, src => src.ConfigObjectRelease)
                .Map(dest => dest.Content, src => src.Encryption ? DecryptContent(configObjectReleases.Content) : src.Content)
                .Map(dest => dest.TempContent, src => src.Encryption ? DecryptContent(configObjectReleases.TempContent) : src.TempContent);

            var result = configObjectReleases?.Adapt<ConfigObject, ConfigObjectWithReleaseHistoryDto>();
            result?.ConfigObjectReleases.ForEach(config =>
            {
                var content = result.Encryption ? DecryptContent(config.Content) : config.Content;
                config.Content = content;
            });

            query.Result = result ?? new();
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

        [EventHandler]
        public async Task RefreshConfigObjectToRedisAsync(RefreshConfigObjectToRedisQuery query)
        {
            query.Result = await _configObjectDomainService.RefreshConfigObjectToRedisAsync();
        }

        [EventHandler]
        public async Task GetConfigObjectsAsync(ConfigObjectsByDynamicQuery query)
        {
            query.Result = await _configObjectDomainService.GetConfigObjectsAsync(query.environment, query.cluster, query.appId, query.configObjects);
        }

        [EventHandler]
        public async Task GetPublicConfigAsync(PublicConfigQuery query)
        {
            query.Result = await _configurationApiClient.GetAsync<Dictionary<string, string>>(
               query.Environment,
               query.Cluster,
               "public-$Config",
               query.ConfigObject);
        }
    }
}
