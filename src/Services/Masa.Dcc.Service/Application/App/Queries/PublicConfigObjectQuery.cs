﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.App.Queries
{
    public record PublicConfigObjectQuery(int ConfigObjectId) : Query<PublicConfigObjectDto>
    {
        public override PublicConfigObjectDto Result { get; set; } = new();
    }
}
