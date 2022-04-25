namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("ConfigObjectMains")]
    public class ConfigObjectMain : BaseEntity<int, Guid>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ConfigObjectId { get; private set; }

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 1)]
        [Column(TypeName = "ntext")]
        public string Content { get; private set; }

        [Required]
        [Column(TypeName = "ntext")]
        public string TempContent { get; private set; }

        public ConfigObject ConfigObject { get; private set; } = null!;

        public ConfigObjectMain(int configObjectId, string content, string tempContent)
        {
            ConfigObjectId = configObjectId;
            Content = content;
            TempContent = tempContent;
        }
    }
}
