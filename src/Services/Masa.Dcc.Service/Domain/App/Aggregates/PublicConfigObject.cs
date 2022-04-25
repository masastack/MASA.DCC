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

        public ConfigObject? ConfigObject { get; set; }

        public PublicConfig? PublicConfig { get; set; }

        [NotMapped]
        public EnvironmentClusterModel? EnvironmentClusterModel { get; set; }

        public PublicConfigObject(int publicConfigId, int configObjectId)
        {
            PublicConfigId = publicConfigId;
            ConfigObjectId = configObjectId;
        }
    }
}
