// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Shared;

[Table("BizConfigObjects")]
public class BizConfigObject : ConfigObjectBase
{
    [Required]
    [Range(1, int.MaxValue)]
    public int BizConfigId { get; private set; }

    public BizConfig BizConfig { get; private set; } = null!;

    public BizConfigObject(int bizConfigId, int environmentClusterId)
    {
        BizConfigId = bizConfigId;
        EnvironmentClusterId = environmentClusterId;
    }
}
