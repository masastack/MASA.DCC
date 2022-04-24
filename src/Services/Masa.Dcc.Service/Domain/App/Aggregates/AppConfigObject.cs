namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("AppConfigObjects")]
    public class AppConfigObject : BaseEntity<int, Guid>
    {
        [Comment("EnvironmentClusterId")]
        [Required]
        [Range(1, int.MaxValue)]
        public int EnvironmentClusterId { get; private set; }

        [Comment("AppId")]
        [Required]
        [Range(1, int.MaxValue)]
        public int AppId { get; private set; }

        [Comment("ConfigObjectId")]
        [Required]
        [Range(1, int.MaxValue)]
        public int ConfigObjectId { get; private set; }

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
