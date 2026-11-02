// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Queries
{
    public record ConfigObjectSearchQuery(SearchConfigObjectDto Condition) : Query<PaginatedListDto<ConfigObjectSearchItemDto>>
    {
        public override PaginatedListDto<ConfigObjectSearchItemDto> Result { get; set; } = new();
    }
}
