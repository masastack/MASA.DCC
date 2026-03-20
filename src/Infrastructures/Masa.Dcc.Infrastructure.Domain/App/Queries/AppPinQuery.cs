// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.ReadWriteSplitting.Cqrs.Queries;
using Masa.Dcc.Contracts.Admin.App.Dtos;

namespace Masa.Dcc.Infrastructure.Domain.Queries
{
    public record AppPinQuery(List<int> AppIds) : Query<List<AppPinDto>>
    {
        public override List<AppPinDto> Result { get; set; } = new();
    }
}
