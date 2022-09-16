// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.
namespace Masa.Dcc.Service.Services;

public class AppService : ServiceBase
{
    private readonly IPmClient _pmClient;

    public AppService(IServiceCollection services, IPmClient pmClient) : base(services)
    {
        _pmClient = pmClient;
        App.MapGet("api/v1/app/{Id}", GetAsync);
        App.MapPost("api/v1/projects/app", GetListByProjectIdsAsync);
        App.MapGet("api/v1/appWithEnvCluster/{Id}", GetWithEnvironmentClusterAsync);
        App.MapPost("api/v1/app/pin/{appid}", AddAppPinAsync);
        App.MapDelete("api/v1/app/pin/{appId}", RemoveAppPinAsync);
        App.MapPost("api/v1/app/pin", GetAppPinListAsync);
    }

    public async Task<AppDetailModel> GetAsync(int Id)
    {
        var result = await _pmClient.AppService.GetAsync(Id);

        return result;
    }

    public async Task<List<AppDetailModel>> GetListAsync()
    {
        var result = await _pmClient.AppService.GetListAsync();

        return result;
    }

    public async Task<List<AppDetailModel>> GetListByProjectIdsAsync([FromServices] MasaUser masaUser, [FromBody] List<int> projectIds)
    {
        var result = await _pmClient.AppService.GetListByProjectIdsAsync(projectIds);

        return result;
    }

    public async Task<AppDetailModel> GetWithEnvironmentClusterAsync(int Id)
    {
        var result = await _pmClient.AppService.GetWithEnvironmentClusterAsync(Id);

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

    public async Task<List<AppPinDto>> GetAppPinListAsync(IEventBus eventBus, List<int> appIds)
    {
        var query = new AppPinQuery(appIds);
        await eventBus.PublishAsync(query);

        return query.Result;
    }
}
