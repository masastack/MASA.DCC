namespace Masa.Dcc.Service.Admin.Domain.App.Services
{
    public class ConfigObjectReleaseDomainService : DomainService
    {
        private readonly DccDbContext _context;
        private readonly IConfigObjectReleaseRepository _configObjectReleaseRepository;
        private readonly IConfigObjectRepository _configObjectRepository;

        public ConfigObjectReleaseDomainService(
            IDomainEventBus eventBus,
            DccDbContext context,
            IConfigObjectReleaseRepository configObjectReleaseRepository,
            IConfigObjectRepository configObjectRepository) : base(eventBus)
        {
            _context = context;
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

            configObject.UpdateContent(configObjectReleaseDto.Content, configObjectReleaseDto.Content);
            await _configObjectRepository.UpdateAsync(configObject);
        }

        public async Task RollbackConfigObjectReleaseAsync(RollbackConfigObjectReleaseDto rollbackDto)
        {
            if (rollbackDto.RollbackToReleaseId == 0)
            {
                await RollbackAsync(rollbackDto.ConfigObjectId);
            }
            else
            {
                await RollbackToAsync(rollbackDto);
            }

        }

        private async Task RollbackAsync(int configObjectId)
        {
            List<ConfigObjectRelease> configObjectReleases = (await _configObjectReleaseRepository.GetListAsync(
                cor => cor.ConfigObjectId == configObjectId && cor.IsInvalid == false))//Remove the rolled back version
                    .OrderByDescending(cor => cor.Id)
                    .ToList();

            if (configObjectReleases.Count < 2)
            {
                throw new UserFriendlyException("没有可回滚的版本");
            }

            var latestConfigObjectRelease = configObjectReleases.First();

            //Excluding the same version and the latest version is the version that can be rolled back
            var canRollbackEntity = configObjectReleases
                .Where(cor => cor.ToReleaseId != latestConfigObjectRelease.Id && cor.Id != latestConfigObjectRelease.Id)
                .OrderByDescending(cor => cor.Id)
                .FirstOrDefault();

            if (canRollbackEntity == null)
            {
                throw new UserFriendlyException("没有可回滚的版本");
            }

            //rollback
            //add
            await _configObjectReleaseRepository.AddAsync(new ConfigObjectRelease(
                     canRollbackEntity.ConfigObjectId,
                     canRollbackEntity.Name,
                     $"由 {latestConfigObjectRelease.Name} 回滚至 {canRollbackEntity.Name}",
                     canRollbackEntity.Content,
                     latestConfigObjectRelease.Id,
                     canRollbackEntity.Id
                 ));

            //Invalid rollback entity
            canRollbackEntity.Invalid();
            await _configObjectReleaseRepository.UpdateAsync(canRollbackEntity);
        }

        private async Task RollbackToAsync(RollbackConfigObjectReleaseDto rollbackDto)
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
                     latestConfigObjectRelease.Id,
                     canRollbackEntity.Id
                 ));

            //Invalid rollback entity
            canRollbackEntity.Invalid();
            await _configObjectReleaseRepository.UpdateAsync(canRollbackEntity);
        }
    }
}