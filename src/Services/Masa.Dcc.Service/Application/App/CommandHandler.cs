
namespace Masa.Dcc.Service.Admin.Application.App
{
    public class CommandHandler
    {
        private readonly IPublicConfigRepository _publicConfigRepository;

        public CommandHandler(IPublicConfigRepository publicConfigRepository)
        {
            _publicConfigRepository = publicConfigRepository;
        }

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
        public async Task RemovePublicConfigAsync(UpdatePublicConfigCommand command)
        {
            var publicConfig = command.UpdatePublicConfigDto;
            var publicConfigEntity = await _publicConfigRepository.FindAsync(p => p.Id == publicConfig.Id)
                ?? throw new UserFriendlyException("PublicConfig not exist");

            await _publicConfigRepository.RemoveAsync(publicConfigEntity);
        }
    }
}
