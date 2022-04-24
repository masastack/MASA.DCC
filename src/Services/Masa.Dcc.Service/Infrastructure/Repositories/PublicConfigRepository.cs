namespace Masa.Dcc.Service.Infrastructure.Repositories
{
    public class PublicConfigRepository : Repository<DccDbContext, PublicConfig>, IPublicConfigRepository
    {
        public PublicConfigRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}