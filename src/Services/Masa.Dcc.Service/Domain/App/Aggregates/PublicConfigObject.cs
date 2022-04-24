namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("PublicConfigObjects")]
    public class PublicConfigObject : BaseEntity<int, Guid>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int PublicConfigId { get; private set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ConfigObjectId { get; private set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EnvironmentClusterId { get; private set; }

        public ConfigObject? ConfigObject { get; private set; }

        public PublicConfig? PublicConfig { get; private set; }

        [NotMapped]
        public EnvironmentClusterModel? EnvironmentClusterModel { get; private set; }

        public PublicConfigObject(int configObjectId, int envClusterId)
        {
            ConfigObjectId = configObjectId;
            EnvironmentClusterId = envClusterId;
        }
    }
}
