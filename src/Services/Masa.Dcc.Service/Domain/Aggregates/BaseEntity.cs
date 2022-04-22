namespace Masa.Dcc.Service.Admin.Domain.Aggregates
{
    public class BaseEntity<Tkey, TUserId> : AuditAggregateRoot<Tkey, TUserId>, ISoftDelete
    {
        public bool IsDeleted { get; }
    }
}
