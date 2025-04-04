﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Shared;

public class AppPin : FullAggregateRoot<int, Guid>
{
    [Required]
    [Range(1, int.MaxValue)]
    public int AppId { get; set; }

    public AppPin(int appId)
    {
        AppId = appId;
    }
}
