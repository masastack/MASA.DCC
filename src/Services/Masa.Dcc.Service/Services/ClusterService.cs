// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Services;

public class ClusterService : ServiceBase
{
    private readonly IPmClient _pmClient;

    public ClusterService(IServiceCollection services, IPmClient pmClient) : base(services)
    {
        _pmClient = pmClient;
        App.MapGet("api/v1/cluster", GetListAsync);
        App.MapGet("api/v1/cluster/{Id}", GetAsync);
        App.MapGet("api/v1/envClusters", GetEnvironmentClustersAsync);
        App.MapGet("api/v1/{envId}/cluster", GetListByEnvIdAsync);
    }

    public async Task<List<ClusterModel>> GetListAsync()
    {
        var result = await _pmClient.ClusterService.GetListAsync();

        return result;
    }

    public async Task<ClusterDetailModel> GetAsync(int Id)
    {
        var result = await _pmClient.ClusterService.GetAsync(Id);

        return result;
    }

    public async Task<List<EnvironmentClusterModel>> GetEnvironmentClustersAsync()
    {
        var result = await _pmClient.ClusterService.GetEnvironmentClustersAsync();

        return result;
    }

    public async Task<List<ClusterModel>> GetListByEnvIdAsync(int envId)
    {
        var result = await _pmClient.ClusterService.GetListByEnvIdAsync(envId);

        return result;
    }
}
