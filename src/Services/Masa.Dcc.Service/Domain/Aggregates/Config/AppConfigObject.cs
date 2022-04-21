namespace Masa.Dcc.Service.Admin.Domain.Aggregates.Config
{
    [Table("AppConfigObjects")]
    public class AppConfigObject : AuditAggregateRoot<Guid, Guid>, ISoftDelete
    {
        [Comment("EnvironmentClusterId")]
        [Required]
        public int EnvironmentClusterId { get; }

        [Comment("AppId")]
        [Required]
        public int AppId { get; }

        [Comment("ConfigObjectId")]
        [Required]
        public int ConfigObjectId { get; }

        private List<ConfigObject> configObjects = new();
        public IReadOnlyCollection<ConfigObject> ConfigObjects => configObjects;
    }
}
