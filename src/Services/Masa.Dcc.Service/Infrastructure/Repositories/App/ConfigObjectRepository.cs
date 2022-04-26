
namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories
{
    public class ConfigObjectRepository : Repository<DccDbContext, ConfigObject>, IConfigObjectRepository
    {
        public ConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}
