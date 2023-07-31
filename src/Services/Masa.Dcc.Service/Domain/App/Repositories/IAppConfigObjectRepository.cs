﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Repositories
{
    public interface IAppConfigObjectRepository : IRepository<AppConfigObject>
    {
        Task<List<AppConfigObject>> GetListByAppIdAsync(int appId);

        Task<List<AppConfigObject>> GetListByEnvClusterIdAsync(int envClusterId, int appId, bool getLatestRelease = false);

        Task<List<(int AppId, ConfigObjectRelease Release)>> GetAppLatestReleaseConfigAsync(IEnumerable<int> appIds,
            int? envClusterId = null);

        Task<AppConfigObject> GetbyConfigObjectIdAsync(int configObjectId);
    }
}
