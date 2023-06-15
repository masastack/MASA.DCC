﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Repositories
{
    public interface IBizConfigObjectRepository : IRepository<BizConfigObject>
    {
        Task<List<BizConfigObject>> GetListByEnvClusterIdAsync(int envClusterId, int bizConfigId,
            bool getLatestRelease = false);

        Task<List<BizConfigObject>> GetListByBizConfigIdAsync(int bizConfigId);

        Task<List<(ConfigObjectRelease Release, int ProjectId)>> GetProjectLatestReleaseConfigAsync(
            List<ProjectModel> projects, int? envClusterId = null);
    }
}
