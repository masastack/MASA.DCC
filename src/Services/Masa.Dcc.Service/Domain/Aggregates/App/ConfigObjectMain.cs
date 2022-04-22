namespace Masa.Dcc.Service.Admin.Domain.Aggregates.App
{
    [Table("ConfigObjectMains")]
    public class ConfigObjectMain : BaseEntity<int, Guid>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ConfigObjectId { get; }

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 1)]
        [Column(TypeName = "ntext")]
        public string Content { get; }

        [Required]
        [Column(TypeName = "ntext")]
        public string TempContent { get; }

        public ConfigObjectMain(int configObjectId, string content, string tempContent)
        {
            ConfigObjectId = configObjectId;
            Content = content;
            TempContent = tempContent;
        }
    }
}
