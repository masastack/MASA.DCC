namespace Masa.Dcc.Service.Admin.Domain.App.Services
{
    public class ConfigObjectReleaseDomainService : DomainService
    {
        private readonly IConfigObjectReleaseRepository _configObjectReleaseRepository;
        private readonly IConfigObjectRepository _configObjectRepository;

        public ConfigObjectReleaseDomainService(
            IDomainEventBus eventBus,
            IConfigObjectReleaseRepository configObjectReleaseRepository,
            IConfigObjectRepository configObjectRepository) : base(eventBus)
        {
            _configObjectReleaseRepository = configObjectReleaseRepository;
            _configObjectRepository = configObjectRepository;
        }

        public async Task AddConfigObjectRelease(AddConfigObjectReleaseDto configObjectReleaseDto)
        {
            await _configObjectReleaseRepository.AddAsync(new ConfigObjectRelease(
                   configObjectReleaseDto.ConfigObjectId,
                   configObjectReleaseDto.Name,
                   configObjectReleaseDto.Comment,
                   configObjectReleaseDto.Content)
               );

            var configObject = (await _configObjectRepository.FindAsync(
                configObject => configObject.Id == configObjectReleaseDto.ConfigObjectId)) ?? throw new Exception("config object does not exist");

            configObject.UpdateContent(configObject.TempContent, configObject.TempContent);
            await _configObjectRepository.UpdateAsync(configObject);
        }
    }
}