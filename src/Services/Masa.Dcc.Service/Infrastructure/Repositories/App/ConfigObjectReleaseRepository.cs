namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories.App
{
    public class ConfigObjectReleaseRepository : Repository<DccDbContext, ConfigObjectRelease>, IConfigObjectReleaseRepository
    {
        public ConfigObjectReleaseRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}
