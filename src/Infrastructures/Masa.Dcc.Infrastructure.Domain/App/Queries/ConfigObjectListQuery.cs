// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Queries
{
    public record ConfigObjectListQuery(List<int> Ids) : Query<List<ConfigObjectDto>>
    {
        public override List<ConfigObjectDto> Result { get; set; } = new();
    }
}
