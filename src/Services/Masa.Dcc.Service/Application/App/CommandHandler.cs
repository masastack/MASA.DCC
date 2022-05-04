namespace Masa.Dcc.Service.Admin.Application.App
{
    public class CommandHandler
    {
        private readonly IPublicConfigRepository _publicConfigRepository;
        private readonly IConfigObjectRepository _configObjectRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly IConfigObjectReleaseRepository _configObjectReleaseRepository;
        private readonly ConfigObjectReleaseDomainService _configObjectReleaseDomainService;

        public CommandHandler(
            IPublicConfigRepository publicConfigRepository,
            IConfigObjectRepository configObjectRepository,
            ILabelRepository labelRepository,
            IConfigObjectReleaseRepository configObjectReleaseRepository,
            ConfigObjectReleaseDomainService configObjectReleaseDomainService)
        {
            _publicConfigRepository = publicConfigRepository;
            _configObjectRepository = configObjectRepository;
            _labelRepository = labelRepository;
            _configObjectReleaseRepository = configObjectReleaseRepository;
            _configObjectReleaseDomainService = configObjectReleaseDomainService;
        }

        #region PublicConfig

        [EventHandler]
        public async Task AddPublicConfigAsync(AddPublicConfigCommand command)
        {
            var publicConfig = command.AddPublicConfigDto;

            await _publicConfigRepository.AddAsync(
                new PublicConfig(publicConfig.Name, publicConfig.Identity, publicConfig.Description));
        }

        [EventHandler]
        public async Task UpdatePublicConfigAsync(UpdatePublicConfigCommand command)
        {
            var publicConfig = command.UpdatePublicConfigDto;
            var publicConfigEntity = await _publicConfigRepository.FindAsync(p => p.Id == publicConfig.Id)
                ?? throw new UserFriendlyException("Public config does not exist");

            publicConfigEntity.Update(publicConfig.Name, publicConfig.Description);
            await _publicConfigRepository.UpdateAsync(publicConfigEntity);
        }

        [EventHandler]
        public async Task RemovePublicConfigAsync(RemovePublicConfigCommand command)
        {
            var publicConfigEntity = await _publicConfigRepository.FindAsync(p => p.Id == command.PublicConfigId)
                ?? throw new UserFriendlyException("Public config does not exist");

            await _publicConfigRepository.RemoveAsync(publicConfigEntity);
        }

        #endregion

        #region ConfigObject

        [EventHandler]
        public async Task AddConfigObjectAsync(AddConfigObjectCommand command)
        {
            var configObjectDto = command.ConfigObjectDto;

            var formatLabel = await _labelRepository.FindAsync(label => label.Id == configObjectDto.FormatLabelId);
            string initialContent = (formatLabel?.Name.ToLower()) switch
            {
                "json" => "{}",
                "properties" => "[]",
                _ => "",
            };
            ConfigObject configObject = await _configObjectRepository.AddAsync(
                new ConfigObject(
                    configObjectDto.Name,
                    configObjectDto.FormatLabelId,
                    configObjectDto.Type,
                    initialContent,
                    initialContent)
                );

            if (configObjectDto.Type == ConfigObjectType.Public)
            {
                var publicConfigObject = new PublicConfigObject(configObject.Id, configObjectDto.PublicConfigId, configObjectDto.EnvironmentClusterId);
                configObject.AddPublicConfigObject(publicConfigObject);
            }
            else if (configObjectDto.Type == ConfigObjectType.App)
            {
                var publicConfigObject = new AppConfigObject(configObject.Id, configObjectDto.AppId, configObjectDto.EnvironmentClusterId);
                configObject.AddAppConfigObject(publicConfigObject);
            }

            command.Result = configObject.Adapt<ConfigObjectDto>();
        }

        [EventHandler]
        public async Task RemoveConfigObjectAsync(RemoveConfigObjectCommand command)
        {
            var configEntity = await _configObjectRepository.FindAsync(p => p.Id == command.ConfigObjectId)
                ?? throw new UserFriendlyException("Config object does not exist");

            await _configObjectRepository.RemoveAsync(configEntity);
        }

        [EventHandler]
        public async Task RevokeConfigObjectAsync(RevokeConfigObjectCommand command)
        {
            var configObject = await _configObjectRepository.FindAsync(configObject => configObject.Id == command.ConfigObjectId);

            if (configObject == null)
            {
                throw new UserFriendlyException("Config object does not exist");
            }

            configObject.Revoke();

            await _configObjectRepository.UpdateAsync(configObject);
        }

        #endregion

        #region AddConfigObjectContent

        [EventHandler]
        public async Task UpdateConfigObjectContentAsync(UpdateConfigObjectContentCommand command)
        {
            var configObjectContentDto = command.ConfigObjectContent;
            var configObject = await _configObjectRepository.FindAsync(configObject => configObject.Id == configObjectContentDto.ConfigObjectId)
                ?? throw new UserFriendlyException("Config object does not exist");

            configObject.UpdateContent(configObjectContentDto.Content, configObject.Content);

            var newConfigObject = await _configObjectRepository.UpdateAsync(configObject);

            command.Result = newConfigObject.Adapt<ConfigObjectDto>();
        }

        #endregion

        #region Release

        [EventHandler]
        public async Task AddConfigObjectRelease(AddConfigObjectReleaseCommand command)
        {
            await _configObjectReleaseDomainService.AddConfigObjectRelease(command.ConfigObjectRelease);
        }

        [EventHandler]
        public async Task RollbackConfigObjectReleaseAsync(RollbackConfigObjectReleaseCommand command)
        {
            await _configObjectReleaseDomainService.RollbackConfigObjectReleaseAsync(command.RollbackConfigObjectRelease);
        }

        #endregion
    }
}
