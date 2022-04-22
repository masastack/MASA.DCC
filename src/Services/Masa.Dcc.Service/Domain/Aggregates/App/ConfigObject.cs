namespace Masa.Dcc.Service.Admin.Domain.Aggregates
{
    [Table("ConfigObjects")]
    public class ConfigObject : BaseEntity<int, Guid>
    {
        [Comment("Name")]
        [Required(ErrorMessage = "Config object name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Config object name length range is [2-100]")]
        public string Name { get; }

        [Comment("Format")]
        [Range(1, int.MaxValue, ErrorMessage = "Format is required")]
        public int FormatLabelId { get; }

        [Comment("Type")]
        [Range(1, int.MaxValue, ErrorMessage = "Type is required")]
        public int TypeLabelId { get; }

        [Comment("Relation config object Id")]
        public int RelationConfigObjectId { get; }

        public ConfigObject(string name, int formatLabelId, int typeLabelId, int relationConfigObjectId = 0)
        {
            Name = name;
            FormatLabelId = formatLabelId;
            TypeLabelId = typeLabelId;
            RelationConfigObjectId = relationConfigObjectId;
        }
    }
}
