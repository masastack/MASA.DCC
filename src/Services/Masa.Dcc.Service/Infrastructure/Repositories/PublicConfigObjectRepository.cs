namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories
{
    public class PublicConfigObjectRepository : Repository<DccDbContext, PublicConfigObject>, IPublicConfigObjectRepository
    {
        public PublicConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}
