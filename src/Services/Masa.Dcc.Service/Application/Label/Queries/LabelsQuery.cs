﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.Label.Queries
{
    public record LabelsQuery(string TypeCode) : Query<List<LabelDto>>
    {
        public override List<LabelDto> Result { get; set; } = new();
    }
}
