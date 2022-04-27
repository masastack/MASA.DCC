namespace Masa.Dcc.Service.Admin.Application.App
{
    public class CommandHandler
    {
        private readonly IPublicConfigRepository _publicConfigRepository;
        private readonly IPublicConfigObjectRepository _publicConfigObjectRepository;
        private readonly IConfigObjectRepository _configObjectRepository;
        private readonly ILabelRepository _labelRepository;

        public CommandHandler(
            IPublicConfigRepository publicConfigRepository,
            IPublicConfigObjectRepository publicConfigObjectRepository,
            IConfigObjectRepository configObjectRepository,
            ILabelRepository labelRepository)
        {
            _publicConfigRepository = publicConfigRepository;
            _publicConfigObjectRepository = publicConfigObjectRepository;
            _configObjectRepository = configObjectRepository;
            _labelRepository = labelRepository;
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
                ?? throw new UserFriendlyException("PublicConfig not exist");

            publicConfigEntity.Update(publicConfig.Name, publicConfig.Description);
            await _publicConfigRepository.UpdateAsync(publicConfigEntity);
        }

        [EventHandler]
        public async Task RemovePublicConfigAsync(RemovePublicConfigCommand command)
        {
            var publicConfigEntity = await _publicConfigRepository.FindAsync(p => p.Id == command.PublicConfigId)
                ?? throw new UserFriendlyException("PublicConfig not exist");

            await _publicConfigRepository.RemoveAsync(publicConfigEntity);
        }

        #endregion

        #region ConfigObject

        [EventHandler(1)]
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
        public async Task RemovePublicConfigObjectAsync(RemoveConfigObjectCommand command)
        {
            var configEntity = await _configObjectRepository.FindAsync(p => p.Id == command.ConfigObjectId)
                ?? throw new UserFriendlyException("config object not exist");

            await _configObjectRepository.RemoveAsync(configEntity);
        }

        #endregion

        #region ConfigObjectContent

        [EventHandler]
        public async Task UpdateConfigObjectContentAsync(UpdateConfigObjectContentCommand command)
        {
            var configObjectContentDto = command.ConfigObjectContent;
            var configObject = await _configObjectRepository.FindAsync(configObject => configObject.Id == configObjectContentDto.ConfigObjectId)
                ?? throw new UserFriendlyException("config object not exist ");

            configObject.UpdateContent(configObject.TempContent, configObjectContentDto.TempContent);

            var newConfigObject = await _configObjectRepository.UpdateAsync(configObject);

            command.Result = newConfigObject.Adapt<ConfigObjectDto>();
        }

        #endregion
    }
}
