
namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories
{
    public class ConfigObjectRepository : Repository<DccDbContext, ConfigObject>, IConfigObjectRepository
    {
        public ConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }

        public async Task<ConfigObject> GetConfigObjectWhitReleaseHistoriesAsync(int Id)
        {
            var configObject = await Context.Set<ConfigObject>()
                .Where(configObject => configObject.Id == Id)
                .Include(configObject => configObject.ConfigObjectRelease)
                .FirstOrDefaultAsync();

            return configObject ?? throw new UserFriendlyException("config object does not exist");
        }
    }
}
