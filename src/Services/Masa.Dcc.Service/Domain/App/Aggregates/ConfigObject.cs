// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("ConfigObjects")]
    public class ConfigObject : FullAggregateRoot<int, Guid>
    {
        [Comment("Name")]
        [Required(ErrorMessage = "Config object name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Config object name length range is [2-100]")]
        public string Name { get; private set; }

        [Comment("Format")]
        [Required(ErrorMessage = "Format Label Code is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Format Label Code length range is [2-100]")]
        public string FormatLabelCode { get; private set; }

        [Comment("Type")]
        [Range(1, int.MaxValue, ErrorMessage = "Type is required")]
        public ConfigObjectType Type { get; private set; }

        [Required]
        public bool Encryption { get; private set; }

        [Required]
        [Column(TypeName = "ntext")]
        public string Content { get; private set; }

        [Required]
        [Column(TypeName = "ntext")]
        public string TempContent { get; private set; }

        [Comment("Relation config object Id")]
        public int RelationConfigObjectId { get; private set; }

        public bool FromRelation { get; private set; }

        public PublicConfigObject PublicConfigObject { get; private set; } = null!;

        public BizConfigObject BizConfigObject { get; set; } = null!;

        public AppConfigObject AppConfigObject { get; private set; } = null!;

        private readonly List<ConfigObjectRelease> _configObjectRelease = new();
        public IReadOnlyCollection<ConfigObjectRelease> ConfigObjectRelease => _configObjectRelease;

        public ConfigObject(string name, string formatLabelCode, ConfigObjectType type, string content, string tempContent, int relationConfigObjectId = 0, bool fromRelation = false, bool encryption = false)
        {
            Name = name;
            FormatLabelCode = formatLabelCode;
            Type = type;
            Content = content;
            TempContent = tempContent;
            RelationConfigObjectId = relationConfigObjectId;
            FromRelation = fromRelation;
            Encryption = encryption;
        }

        public void SetConfigObjectType(ConfigObjectType type)
        {
            Type = type;
        }

        public void SetConfigObjectRelease(IEnumerable<ConfigObjectRelease> configObjectReleases)
        {
            _configObjectRelease.Clear();
            _configObjectRelease.TryAddRange(configObjectReleases);
        }

        public void SetConfigObjectRelease(ConfigObjectRelease configObjectRelease)
        {
            _configObjectRelease.Clear();
            _configObjectRelease.TryAdd(configObjectRelease);
        }

        public void UpdateContent(string content)
        {
            Content = content;
        }

        public void AddContent(string content, string tempContent)
        {
            Content = content;
            TempContent = tempContent;
        }

        public void SetPublicConfigObject(int publicConfigId, int environmentClusterId)
        {
            PublicConfigObject = new PublicConfigObject(publicConfigId, environmentClusterId);
        }

        public void SetBizConfigObject(int bizId, int environmentClusterId)
        {
            BizConfigObject = new BizConfigObject(bizId, environmentClusterId);
        }

        public void SetAppConfigObject(int appId, int environmentClusterId)
        {
            AppConfigObject = new AppConfigObject(appId, environmentClusterId);
        }

        public void Revoke()
        {
            Content = TempContent;
        }

        public void Relation(int relationConfigObjectId)
        {
            RelationConfigObjectId = relationConfigObjectId;
            FromRelation = true;
        }

        public void UnRelation()
        {
            RelationConfigObjectId = 0;
        }
    }
}
