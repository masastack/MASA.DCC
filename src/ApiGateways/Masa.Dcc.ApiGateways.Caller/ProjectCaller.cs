// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Caller
{
    public class ProjectCaller : DccHttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/project";

        public ProjectCaller(DccApiGatewayOptions options) : base(options)
        {
        }

        public async Task<List<ProjectModel>> GetListByTeamIdAsync(IEnumerable<Guid> teamIds)
        {
            var result = await Caller.PostAsync<List<ProjectModel>>($"{_prefix}/teamsProject", teamIds);

            return result ?? new();
        }

        public async Task<List<ProjectModel>> GetListByEnvIdAsync(int envClusterId)
        {
            var result = await Caller.GetAsync<List<ProjectModel>>($"/api/v1/{envClusterId}/project");

            return result ?? new();
        }

        public async Task<ProjectDetailModel> GetAsync(int Id)
        {
            var result = await Caller.GetAsync<ProjectDetailModel>($"{_prefix}/{Id}");

            return result ?? new();
        }

        public async Task<List<ProjectTypeModel>> GetProjectTypesAsync()
        {
            var result = await Caller.GetAsync<List<ProjectTypeModel>>($"{_prefix}/projectType");

            return result ?? new();
        }

        public async Task<List<ProjectModel>> GetProjectsAsync()
        {
            var result = await Caller.GetAsync<List<ProjectModel>>(_prefix);

            return result ?? new();
        }
    }
}
