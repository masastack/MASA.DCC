namespace Masa.Dcc.Service.Admin.Domain.Aggregates
{
    [Table("AppConfigObjects")]
    public class AppConfigObject : BaseEntity<int, Guid>
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

        private readonly List<ConfigObject> configObjects = new();
        public IReadOnlyCollection<ConfigObject> ConfigObjects => configObjects;

        public AppConfigObject(int environmentClusterId, int appId, int configObjectId)
        {
            EnvironmentClusterId = environmentClusterId;
            AppId = appId;
            ConfigObjectId = configObjectId;
        }

        public AppConfigObject(int environmentClusterId, int appId, int configObjectId, List<ConfigObject> configObjects)
        {
            EnvironmentClusterId = environmentClusterId;
            AppId = appId;
            ConfigObjectId = configObjectId;
            this.configObjects = configObjects;
        }
    }
}
