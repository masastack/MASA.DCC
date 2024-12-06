// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Queries
{
    public record PublicConfigsQuery : Query<List<PublicConfigDto>>
    {
        public override List<PublicConfigDto> Result { get; set; } = new();
    }
}
