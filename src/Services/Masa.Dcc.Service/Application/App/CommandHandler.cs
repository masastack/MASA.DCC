
namespace Masa.Dcc.Service.Admin.Application.App
{
    public class CommandHandler
    {
        private readonly IPublicConfigRepository _publicConfigRepository;
        private readonly IPublicConfigObjectRepository _publicConfigObjectRepository;

        public CommandHandler(IPublicConfigRepository publicConfigRepository, IPublicConfigObjectRepository publicConfigObjectRepository)
        {
            _publicConfigRepository = publicConfigRepository;
            _publicConfigObjectRepository = publicConfigObjectRepository;
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

        #region PublicConfigObject

        public async Task AddPublicConfigObjectAsync(AddPublicConfigObjectCommand command)
        {
            var publicConfigObject = command.AddPublicConfigObject;

            await _publicConfigObjectRepository.AddAsync(
                new PublicConfigObject(publicConfigObject.ConfigObjectId, publicConfigObject.EnvironmentClusterId));
        }

        public async Task RemovePublicConfigObjectAsync(RemovePublicConfigObjectCommand command)
        {
            var publicConfigEntity = await _publicConfigObjectRepository.FindAsync(p => p.Id == command.PublicConfigObjectId)
                ?? throw new UserFriendlyException("PublicConfig not exist");

            await _publicConfigObjectRepository.RemoveAsync(publicConfigEntity);
        }

        #endregion
    }
}
