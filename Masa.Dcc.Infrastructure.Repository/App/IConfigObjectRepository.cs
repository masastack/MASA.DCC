// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.App;

public interface IConfigObjectRepository : IRepository<ConfigObject>
{
    Task<ConfigObject> GetConfigObjectWithReleaseHistoriesAsync(int Id);

    Task<List<ConfigObject>> GetRelationConfigObjectWithReleaseHistoriesAsync(int Id);

    Task<List<(ConfigObject ConfigObject, int ObjectId, int EnvironmentClusterId)>> GetNewestConfigObjectReleaseWithAppInfo();
}
