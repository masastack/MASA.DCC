// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos;

public class LatestReleaseConfigModel
{
    public Guid LastPublisherId { get; set; }

    public string? LastPublisher { get; set; }

    public DateTime LastPublishTime { get; set; }

    public int? AppId { get; set; }

    public int? EnvClusterId { get; set; }

    public int? ProjectId { get; set; }

    public int ConfigObjectId { get; set; }
}
