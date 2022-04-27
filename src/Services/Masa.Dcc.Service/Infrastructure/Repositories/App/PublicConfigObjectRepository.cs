namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories
{
    public class PublicConfigObjectRepository : Repository<DccDbContext, PublicConfigObject>, IPublicConfigObjectRepository
    {
        public PublicConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }

        public async Task<List<PublicConfigObject>> GetListByEnvClusterIdAsync(int envClusterId)
        {
            var configData = await Context.Set<PublicConfigObject>()
                .Where(publicConfigObject => publicConfigObject.EnvironmentClusterId == envClusterId)
                .Include(publicConfigObject => publicConfigObject.ConfigObject)
                .ThenInclude(configObject => configObject.ConfigObjectMain)
                .ToListAsync();

            return configData;
        }
    }
}
