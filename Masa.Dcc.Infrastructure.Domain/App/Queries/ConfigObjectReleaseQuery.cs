// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Queries
{
    public record ConfigObjectReleaseQuery(int ConfigObjectId) : Query<ConfigObjectWithReleaseHistoryDto>
    {
        public override ConfigObjectWithReleaseHistoryDto Result { get; set; } = new();
    }
}
