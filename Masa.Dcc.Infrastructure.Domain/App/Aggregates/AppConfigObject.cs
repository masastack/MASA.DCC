// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.App.Aggregates;

[Table("AppConfigObjects")]
public class AppConfigObject : ConfigObjectBase
{
    [Comment("AppId")]
    [Required]
    [Range(1, int.MaxValue)]
    public int AppId { get; private set; }

    public AppConfigObject(int appId, int environmentClusterId)
    {
        EnvironmentClusterId = environmentClusterId;
        AppId = appId;
    }
}
