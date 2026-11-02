// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.App;

public interface IConfigObjectRepository : IRepositoryExtensition<ConfigObject>
{
    Task<ConfigObject> GetConfigObjectWithReleaseHistoriesAsync(int Id);

    Task<List<ConfigObject>> GetRelationConfigObjectWithReleaseHistoriesAsync(int Id);

    Task<List<(ConfigObject ConfigObject, int ObjectId, int EnvironmentClusterId)>> GetNewestConfigObjectReleaseWithAppInfo();

    Task<List<ConfigObject>> GetConfigObjectsByNameAsync(string Name);

    Task<int> GetIdAsync(string Name, ConfigObjectType Type);

    /// <summary>
    /// Paged search. Returned entities include their public / biz / app relation (with PublicConfig / BizConfig).
    /// </summary>
    Task<(long Total, List<ConfigObject> Items)> GetPaginatedListAsync(ConfigObjectSearchCondition condition, int page, int pageSize);
}
