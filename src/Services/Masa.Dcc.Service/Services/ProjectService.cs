// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

public class ProjectService : ServiceBase
{
    public ProjectService(IPmClient pmClient)
    {
        RouteOptions.DisableAutoMapRoute = true;
        App.MapGet("api/v1/projectwithapps/{envName}", GetProjectListAsync);
        App.MapGet("api/v1/project/{id}", GetAsync);
        App.MapGet("api/v1/{envClusterId}/project", GetListByEnvironmentClusterIdAsync);
        App.MapGet("api/v1/project/projectType", GetProjectTypes);
        App.MapGet("api/v1/project", GetListAsync);
        App.MapPost("api/v1/project/teamsProject", GetListByTeamIdsAsync);
    }

    public async Task<List<ProjectAppsModel>> GetProjectListAsync(IPmClient pmClient, string envName)
    {
        var result = await pmClient.ProjectService.GetProjectAppsAsync(envName);

        return result;
    }

    public async Task<ProjectDetailModel> GetAsync(IPmClient pmClient, int id)
    {
        var result = await pmClient.ProjectService.GetAsync(id);

        return result;
    }

    public async Task<List<ProjectModel>> GetListAsync(IPmClient pmClient)
    {
        var result = await pmClient.ProjectService.GetListAsync();

        return result;
    }

    public async Task<List<ProjectModel>> GetListByEnvironmentClusterIdAsync(IPmClient pmClient, int envClusterId)
    {
        var result = await pmClient.ProjectService.GetListByEnvironmentClusterIdAsync(envClusterId);

        return result;
    }

    public async Task<List<ProjectTypeModel>> GetProjectTypes(IPmClient pmClient)
    {
        var result = await pmClient.ProjectService.GetProjectTypesAsync();

        return result;
    }

    public async Task<List<ProjectModel>> GetListByTeamIdsAsync(IPmClient pmClient, [FromBody] List<Guid> teamIds, IMultiEnvironmentUserContext multiEnvironmentUserContext)
    {
        var result = await pmClient.ProjectService.GetListByTeamIdsAsync(teamIds, multiEnvironmentUserContext.Environment ?? "");

        return result;
    }
}
