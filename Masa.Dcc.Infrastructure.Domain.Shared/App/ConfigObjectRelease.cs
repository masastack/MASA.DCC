﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Shared;

[Table("ConfigObjectReleases")]
public class ConfigObjectRelease : FullAggregateRoot<int, Guid>
{
    [Comment("Release type")]
    public ReleaseType Type { get; set; }

    [Comment("Config object Id")]
    [Required(ErrorMessage = "Config object Id is required")]
    [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "Config object Id is required")]
    public int ConfigObjectId { get; set; }

    [Comment("Rollback From Release Id")]
    public int FromReleaseId { get; set; }

    [Comment("If it is rolled back, it will be true")]
    public bool IsInvalid { get; set; }

    [Comment("Version format is yyyyMMddHHmmss")]
    [Required(ErrorMessage = "Version is required")]
    public string Version { get; set; }

    [Comment("Name")]
    [Required(ErrorMessage = "Name is required", AllowEmptyStrings = true)]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name length range is [2-100]")]
    public string Name { get; set; }

    [Comment("Comment")]
    [Required(ErrorMessage = "Comment is required", AllowEmptyStrings = true)]
    [StringLength(500, MinimumLength = 0, ErrorMessage = "Comment length range is [0-500]")]
    public string Comment { get; set; }

    [Comment("Content")]
    [Required(ErrorMessage = "Content is required", AllowEmptyStrings = true)]
    [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Content length range is [1-2147483647]")]
    public string Content { get; set; }

    public ConfigObjectRelease(int configObjectId, string name, string comment, string content, string? version = null, int fromReleaseId = 0, ReleaseType type = ReleaseType.MainRelease)
    {
        ConfigObjectId = configObjectId;
        Name = name;
        Comment = comment;
        Content = content;
        Version = version ?? DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        FromReleaseId = fromReleaseId;
        Type = type;
    }

    public void Invalid()
    {
        IsInvalid = true;
    }
}
