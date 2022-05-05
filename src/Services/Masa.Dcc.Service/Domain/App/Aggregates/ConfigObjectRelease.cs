// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("ConfigObjectReleases")]
    public class ConfigObjectRelease : BaseEntity<int, Guid>
    {
        [Comment("Release type")]
        [Column(TypeName = "tinyint")]
        public ReleaseType Type { get; set; }

        [Comment("Config object Id")]
        [Required(ErrorMessage = "Config object Id is required")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "Config object Id is required")]
        public int ConfigObjectId { get; set; }

        [Comment("Rollback From Release Id")]
        public int FromReleaseId { get; set; }

        [Comment("Rollback To Release Id")]
        public int ToReleaseId { get; set; }

        [Comment("If it is rolled back, it will be true")]
        public bool IsInvalid { get; set; }

        [Comment("Version foramt is YYYYMMDDHHmmss")]
        [Required(ErrorMessage = "Version is required")]
        [Column(TypeName = "varchar(20)")]
        public string Version { get; set; }

        [Comment("Name")]
        [Required(ErrorMessage = "Name is required", AllowEmptyStrings = true)]
        [Column(TypeName = "nvarchar(100)")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name length range is [2-100]")]
        public string Name { get; set; }

        [Comment("Comment")]
        [Required(ErrorMessage = "Comment is required", AllowEmptyStrings = true)]
        [Column(TypeName = "nvarchar(500)")]
        [StringLength(500, MinimumLength = 0, ErrorMessage = "Comment length range is [0-500]")]
        public string Comment { get; set; }

        [Comment("Content")]
        [Required(ErrorMessage = "Content is required", AllowEmptyStrings = true)]
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Content length range is [1-2147483647]")]
        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        public ConfigObjectRelease(int configObjectId, string name, string comment, string content, int fromReleaseId = 0, int toReleaseId = 0, ReleaseType type = ReleaseType.MainRelease)
        {
            ConfigObjectId = configObjectId;
            Name = name;
            Comment = comment;
            Content = content;
            Version = DateTime.Now.ToString("yyyyMMddHHmmss");
            FromReleaseId = fromReleaseId;
            ToReleaseId = toReleaseId;
            Type = type;
        }

        public void Invalid()
        {
            IsInvalid = true;
        }
    }
}
