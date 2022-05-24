﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.App.Commands
{
    public record AddPublicConfigCommand(AddObjectConfigDto AddPublicConfigDto) : Command
    {
        public PublicConfigDto PublicConfigDto { get; set; } = new();
    }
}
