﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Commands
{
    public record InitConfigObjectCommand(string Environment, string Cluster, string AppId,
        Dictionary<string, string> ConfigObjects, ConfigObjectType ConfigObjectType, bool IsEncryption) : Command
    {
    }
}
