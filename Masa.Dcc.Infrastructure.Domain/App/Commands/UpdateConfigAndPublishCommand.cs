﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Commands
{
    public record UpdateConfigAndPublishCommand(string Environment, string Cluster, string AppId,
        string ConfigObject,
        string Value) : Command
    {
    }
}
