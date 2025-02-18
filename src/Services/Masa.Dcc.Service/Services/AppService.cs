// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

public class AppService : ServiceBase
{
    public AppService()
    {
        RouteOptions.DisableAutoMapRoute = true;
        App.MapGet("api/v1/app/{id}", GetAsync);
        App.MapPost("api/v1/projects/app", GetListByProjectIdsAsync);
        App.MapGet("api/v1/appWithEnvCluster/{id}", GetWithEnvironmentClusterAsync);
        App.MapPost("api/v1/app/pin/{appId}", AddAppPinAsync);
        App.MapDelete("api/v1/app/pin/{appId}", RemoveAppPinAsync);
        App.MapGet("api/v1/app/pin", GetAppPinListAsync);
        App.MapPost("api/v1/app/latestReleaseConfig", GetLatestReleaseConfigByAppAsync);
    }

    public Task<AppDetailModel> GetAsync(IPmClient pmClient, int id)
    {
        return pmClient.AppService.GetAsync(id);
    }

    public async Task<List<LatestReleaseConfigModel>> GetLatestReleaseConfigByAppAsync(IEventBus eventBus, IAuthClient authClient, LatestReleaseConfigRequestDto<int> request)
    {
        var query = new AppLatestReleaseQuery(request.Items, request.EnvClusterId);
        await eventBus.PublishAsync(query);
        return await authClient.FillUserNameAsync(query.Result);
    }

    public async Task<List<AppDetailModel>> GetListAsync(IPmClient pmClient)
    {
        return await pmClient.AppService.GetListAsync();
    }

    public async Task<List<AppDetailModel>> GetListByProjectIdsAsync(IPmClient pmClient, [FromBody] List<int> projectIds)
    {
        return await pmClient.AppService.GetListByProjectIdsAsync(projectIds);
    }

    public async Task<AppDetailModel> GetWithEnvironmentClusterAsync(IPmClient pmClient, int id)
    {
        return await pmClient.AppService.GetWithEnvironmentClusterAsync(id);
    }

    public async Task AddAppPinAsync(IEventBus eventBus, int appId)
    {
        await eventBus.PublishAsync(new AddAppPinCommand(appId));
    }

    public async Task RemoveAppPinAsync(IEventBus eventBus, int appId)
    {
        await eventBus.PublishAsync(new RemoveAppPinCommand(appId));
    }

    public async Task<List<AppPinDto>> GetAppPinListAsync(IEventBus eventBus, [FromQuery] int[] appIds)
    {
        var query = new AppPinQuery(appIds.ToList());
        await eventBus.PublishAsync(query);
        return query.Result;
    }
}
