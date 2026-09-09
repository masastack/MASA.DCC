// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.Caching;
using Masa.BuildingBlocks.StackSdks.Dcc.Contracts.Model;
using Masa.Dcc.Contracts.Admin;
using Masa.Dcc.Infrastructure.Domain.Services;

namespace Masa.Dcc.Service.Admin.Infrastructure;

public class RedisConfigPublishStore : IConfigPublishStore
{
    private readonly IMultilevelCacheClient _cacheClient;

    public RedisConfigPublishStore(IMultilevelCacheClient cacheClient)
    {
        _cacheClient = cacheClient;
    }

    public Task SetAsync(
        string environment,
        string cluster,
        string appId,
        string configObjectName,
        PublishReleaseModel model,
        CancellationToken cancellationToken = default)
    {
        var key = DccDaprConfigKeys.BuildCacheKey(environment, cluster, appId, configObjectName);
        return _cacheClient.SetAsync(key, model);
    }

    public Task RemoveAsync(
        string environment,
        string cluster,
        string appId,
        string configObjectName,
        CancellationToken cancellationToken = default)
    {
        var key = DccDaprConfigKeys.BuildCacheKey(environment, cluster, appId, configObjectName);
        return _cacheClient.RemoveAsync<PublishReleaseModel>(key);
    }

    public Task<PublishReleaseModel?> GetAsync(
        string environment,
        string cluster,
        string appId,
        string configObjectName,
        CancellationToken cancellationToken = default)
    {
        var key = DccDaprConfigKeys.BuildCacheKey(environment, cluster, appId, configObjectName);
        return _cacheClient.GetAsync<PublishReleaseModel?>(key);
    }
}
