// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

[Authorize]
public class ClusterService : ServiceBase
{
    public ClusterService(IPmClient pmClient)
    {
        RouteOptions.DisableAutoMapRoute = true;
        App.MapGet("api/v1/cluster", GetListAsync).RequireAuthorization();
        App.MapGet("api/v1/cluster/{Id}", GetAsync).RequireAuthorization();
        App.MapGet("api/v1/envClusters", GetEnvironmentClustersAsync).RequireAuthorization();
        App.MapGet("api/v1/{envId}/cluster", GetListByEnvIdAsync).RequireAuthorization();
    }

    public async Task<List<ClusterModel>> GetListAsync(IPmClient pmClient)
    {
        var result = await pmClient.ClusterService.GetListAsync();

        return result;
    }

    public async Task<ClusterDetailModel> GetAsync(IPmClient pmClient, int Id)
    {
        var result = await pmClient.ClusterService.GetAsync(Id);

        return result;
    }

    public async Task<List<EnvironmentClusterModel>> GetEnvironmentClustersAsync(IPmClient pmClient)
    {
        var result = await pmClient.ClusterService.GetEnvironmentClustersAsync();

        return result;
    }

    public async Task<List<ClusterModel>> GetListByEnvIdAsync(IPmClient pmClient, int envId)
    {
        var result = await pmClient.ClusterService.GetListByEnvIdAsync(envId);

        return result;
    }
}
