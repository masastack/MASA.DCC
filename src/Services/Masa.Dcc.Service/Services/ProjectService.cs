namespace Masa.Dcc.Service.Services;

public class ProjectService : ServiceBase
{
    private readonly IPmClient _pmClient;

    public ProjectService(IServiceCollection services, IPmClient pmClient) : base(services)
    {
        _pmClient = pmClient;
        App.MapGet("api/v1/projectwithapps/{envName}", GetProjectListAsync);
        App.MapGet("api/v1/project/{Id}", GetAsync);
        App.MapGet("api/v1/{envClusterId}/project", GetListByEnvironmentClusterIdAsync);
        App.MapGet("api/v1/project/projectType", GetProjectTypes);
    }

    public async Task<List<ProjectAppsModel>> GetProjectListAsync(string envName)
    {
        var result = await _pmClient.ProjectService.GetProjectAppsAsync(envName);

        return result;
    }

    public async Task<ProjectDetailModel> GetAsync(int Id)
    {
        var result = await _pmClient.ProjectService.GetAsync(Id);

        return result;
    }

    public async Task<List<ProjectModel>> GetListByEnvironmentClusterIdAsync(int envClusterId)
    {
        var result = await _pmClient.ProjectService.GetListByEnvironmentClusterIdAsync(envClusterId);

        return result;
    }

    public async Task<List<ProjectTypeModel>> GetProjectTypes()
    {
        var result = await _pmClient.ProjectService.GetProjectTypesAsync();

        return result;
    }
}
