// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Shared;

[Table("PublicConfigObjects")]
public class PublicConfigObject : ConfigObjectBase
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PublicConfigId { get; private set; }

    public PublicConfig PublicConfig { get; private set; } = null!;

    public PublicConfigObject(int publicConfigId, int environmentClusterId)
    {
        PublicConfigId = publicConfigId;
        EnvironmentClusterId = environmentClusterId;
    }
}
