namespace Masa.Dcc.Service.Admin.Domain.Label.Aggregates
{
    [Table("Labels")]
    [Index(nameof(TypeCode), nameof(IsDeleted), Name = "IX_TypeCode")]
    public class Label : BaseEntity<int, Guid>
    {
        [Comment("Name")]
        [Required(ErrorMessage = "Label name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Label name length range is [2-100]")]
        public string Name { get; private set; }

        [Comment("TypeCode")]
        [Required(ErrorMessage = "TypeCode is required")]
        [StringLength(255, MinimumLength = 0, ErrorMessage = "TypeCode length range is [0-255]")]
        public string TypeCode { get; private set; }

        [Comment("TypeName")]
        [Required(ErrorMessage = "TypeName is required")]
        [StringLength(255, MinimumLength = 0, ErrorMessage = "TypeName length range is [0-255]")]
        public string TypeName { get; private set; }

        [Comment("Description")]
        [Required(ErrorMessage = "Description is required")]
        [StringLength(255, MinimumLength = 0, ErrorMessage = "Description length range is [0-255]")]
        public string Description { get; private set; }

        public Label(string name, string typeCode, string typeName, string description = "")
        {
            Name = name;
            TypeCode = typeCode;
            TypeName = typeName;
            Description = description;
        }
    }
}
