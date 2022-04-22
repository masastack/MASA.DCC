namespace Masa.Dcc.Service.Admin.Domain.Aggregates
{
    [Table("PublicConfigObjects")]
    public class PublicConfigObject : BaseEntity<int, Guid>
    {
        [Required]
        public int PublicConfigId { get; }

        [Required]
        public int ConfigObjectId { get; }

        public ConfigObject ConfigObject { get; } = null!;

        public PublicConfig PublicConfig { get; } = null!;

        public PublicConfigObject(int publicConfigId, int configObjectId)
        {
            PublicConfigId = publicConfigId;
            ConfigObjectId = configObjectId;
        }
    }
}
