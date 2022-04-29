﻿namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("ConfigObjects")]
    public class ConfigObject : BaseEntity<int, Guid>
    {
        [Comment("Name")]
        [Required(ErrorMessage = "Config object name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Config object name length range is [2-100]")]
        public string Name { get; private set; }

        [Comment("Format")]
        [Range(1, int.MaxValue, ErrorMessage = "Format is required")]
        public int FormatLabelId { get; private set; }

        [Comment("Type")]
        [Range(1, int.MaxValue, ErrorMessage = "Type is required")]
        public ConfigObjectType Type { get; private set; }

        [Required]
        [Column(TypeName = "ntext")]
        public string Content { get; private set; }

        [Required]
        [Column(TypeName = "ntext")]
        public string TempContent { get; private set; }

        [Comment("Relation config object Id")]
        public int RelationConfigObjectId { get; private set; }

        private readonly List<PublicConfigObject> _publicConfigObjects = new();
        public IReadOnlyCollection<PublicConfigObject> PublicConfigObjects => _publicConfigObjects;

        private readonly List<AppConfigObject> _appConfigObjects = new();
        public IReadOnlyCollection<AppConfigObject> AppConfigObjects => _appConfigObjects;

        private readonly List<ConfigObjectRelease> _configObjectRelease = new();
        public IReadOnlyCollection<ConfigObjectRelease> ConfigObjectRelease => _configObjectRelease;

        public ConfigObject(string name, int formatLabelId, ConfigObjectType type, string content, string tempContent, int relationConfigObjectId = 0)
        {
            Name = name;
            FormatLabelId = formatLabelId;
            Type = type;
            Content = content;
            TempContent = tempContent;
            RelationConfigObjectId = relationConfigObjectId;
        }

        public void UpdateContent(string content, string tempContent)
        {
            Content = content;
            TempContent = tempContent;
        }

        public void AddPublicConfigObject(PublicConfigObject publicConfigObject)
        {
            _publicConfigObjects.Add(publicConfigObject);
        }

        public void AddAppConfigObject(AppConfigObject appConfigObject)
        {
            _appConfigObjects.Add(appConfigObject);
        }
    }
}
