namespace Masa.Dcc.Service.Admin.Application.App
{
    public class CommandHandler
    {
        private readonly IPublicConfigRepository _publicConfigRepository;
        private readonly IPublicConfigObjectRepository _publicConfigObjectRepository;
        private readonly IConfigObjectRepository _configObjectRepository;

        public CommandHandler(
            IPublicConfigRepository publicConfigRepository,
            IPublicConfigObjectRepository publicConfigObjectRepository,
            IConfigObjectRepository configObjectRepository)
        {
            _publicConfigRepository = publicConfigRepository;
            _publicConfigObjectRepository = publicConfigObjectRepository;
            _configObjectRepository = configObjectRepository;
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
            var configObject = command.ConfigObjectDto;

            ConfigObject configObjectEntity = await _configObjectRepository.AddAsync(
                new ConfigObject(configObject.Name, configObject.FormatLabelId, configObject.TypeLabelId));
            await _configObjectRepository.UnitOfWork.SaveChangesAsync();

            command.ConfigObject = configObjectEntity;
        }

        [EventHandler(2)]
        public async Task AddPublicConfigObjectAsync(AddConfigObjectCommand command)
        {
            var configObjectDto = command.ConfigObjectDto;
            var configObject = command.ConfigObject;

            var publicConfigObject = new PublicConfigObject(configObject.Id, configObjectDto.PublicConfigId, configObjectDto.EnvironmentClusterId);
            configObject.AddPublicConfigObject(publicConfigObject);
            await _publicConfigObjectRepository.AddAsync(publicConfigObject);

            await _publicConfigObjectRepository.UnitOfWork.SaveChangesAsync();
        }

        [EventHandler]
        public async Task RemovePublicConfigObjectAsync(RemoveConfigObjectCommand command)
        {
            var configEntity = await _configObjectRepository.FindAsync(p => p.Id == command.ConfigObjectId)
                ?? throw new UserFriendlyException("config object not exist");

            await _configObjectRepository.RemoveAsync(configEntity);
        }

        #endregion
    }
}
