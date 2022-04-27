namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories.Label
{
    public class LabelRepository : Repository<DccDbContext, Domain.Label.Aggregates.Label>, ILabelRepository
    {
        public LabelRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}
