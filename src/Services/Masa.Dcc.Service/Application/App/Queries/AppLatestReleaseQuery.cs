// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.App.Queries
{
    public record AppLatestReleaseQuery(List<int> AppIds, int? EnvClusterId = null) : Query<List<LatestReleaseConfigModel>>
    {
        public override List<LatestReleaseConfigModel> Result { get; set; } = new();
    }
}
