// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.App.Queries;

public record LatestReleaseByProjectQuery(List<BuildingBlocks.StackSdks.Pm.Model.ProjectModel> Projects, int? EnvClusterId = null) : Query<List<LatestReleaseConfigModel>>
{
    public override List<LatestReleaseConfigModel> Result { get; set; } = new();
}
