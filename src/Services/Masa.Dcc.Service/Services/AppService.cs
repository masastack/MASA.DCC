// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

public class AppService : ServiceBase
{
    private readonly IPmClient _pmClient;
    private readonly IAuthClient _authClient;

    public AppService(IPmClient pmClient, IAuthClient authClient)
    {
        _pmClient = pmClient;
        _authClient = authClient;
        App.MapGet("api/v1/app/{id}", GetAsync);
        App.MapPost("api/v1/projects/app", GetListByProjectIdsAsync);
        App.MapGet("api/v1/appWithEnvCluster/{id}", GetWithEnvironmentClusterAsync);
        App.MapPost("api/v1/app/pin/{appId}", AddAppPinAsync);
        App.MapDelete("api/v1/app/pin/{appId}", RemoveAppPinAsync);
        App.MapGet("api/v1/app/pin", GetAppPinListAsync);
        App.MapPost("api/v1/app/latestReleaseConfig", GetLatestReleaseConfigByAppAsync);
    }

    public async Task<AppDetailModel> GetAsync(int id)
    {
        var result = await _pmClient.AppService.GetAsync(id);
        return result;
    }

    public async Task<List<LatestReleaseConfigModel>> GetLatestReleaseConfigByAppAsync(IEventBus eventBus, LatestReleaseConfigRequestDto<int> request)
    {
        var query = new AppLatestReleaseQuery(request.Items, request.EnvClusterId);
        await eventBus.PublishAsync(query);
        return await _authClient.FillUserNameAsync(query.Result);
    }

    public async Task<List<AppDetailModel>> GetListAsync()
    {
        var result = await _pmClient.AppService.GetListAsync();

        return result;
    }

    public async Task<List<AppDetailModel>> GetListByProjectIdsAsync([FromBody] List<int> projectIds)
    {
        var result = await _pmClient.AppService.GetListByProjectIdsAsync(projectIds);

        return result;
    }

    public async Task<AppDetailModel> GetWithEnvironmentClusterAsync(int id)
    {
        var result = await _pmClient.AppService.GetWithEnvironmentClusterAsync(id);

        return result;
    }

    public async Task AddAppPinAsync(IEventBus eventBus, int appId)
    {
        await eventBus.PublishAsync(new AddAppPinCommand(appId));
    }

    public async Task RemoveAppPinAsync(IEventBus eventBus, int appId)
    {
        await eventBus.PublishAsync(new RemoveAppPinCommand(appId));
    }

    public async Task<List<AppPinDto>> GetAppPinListAsync(IEventBus eventBus, [FromBody] List<int> appIds)
    {
        var query = new AppPinQuery(appIds);
        await eventBus.PublishAsync(query);

        return query.Result;
    }
}
