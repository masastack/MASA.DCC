﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.App.Queries
{
    public record RefreshConfigObjectToRedisQuery : Query<string>
    {
        public override string Result { get; set; } = "error";
    }
}
