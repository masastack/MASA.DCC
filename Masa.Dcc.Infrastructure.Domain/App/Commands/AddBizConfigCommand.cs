﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Commands
{
    [AllowedEvent]
    public record AddBizConfigCommand(AddObjectConfigDto AddBizConfigDto) : Command
    {
        public BizConfigDto BizConfigDto { get; set; } = new();
    }
}
