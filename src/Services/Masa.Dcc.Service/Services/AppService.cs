using Microsoft.AspNetCore.Mvc;

namespace Masa.Dcc.Service.Services;

public class AppService : ServiceBase
{
    private readonly IPmClient _pmClient;

    public AppService(IServiceCollection services, IPmClient pmClient) : base(services)
    {
        _pmClient = pmClient;
        App.MapGet("api/v1/app/{Id}", GetAsync);
        App.MapPost("api/v1/projects/app", GetListByProjectIdsAsync);
        App.MapGet("api/v1/appWhitEnvCluster/{Id}", GetWithEnvironmentClusterAsync);
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

    public async Task<List<AppDetailModel>> GetListByProjectIdsAsync([FromBody] List<int> projectIds)
    {
        var result = await _pmClient.AppService.GetListByProjectIdsAsync(projectIds);

        return result;
    }

    public async Task<AppDetailModel> GetWithEnvironmentClusterAsync(int Id)
    {
        var result = await _pmClient.AppService.GetWithEnvironmentClusterAsync(Id);

        return result;
    }
}
