// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Caller
{
    public class ProjectCaller : HttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/project";

        public ProjectCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(ProjectCaller);
        }

        protected override string BaseAddress { get; set; } = AppSettings.Get("ServiceBaseUrl");

        public async Task<List<ProjectModel>> GetListByTeamIdAsync(Guid teamId)
        {
            var result = await CallerProvider.GetAsync<List<ProjectModel>>($"{_prefix}/teamProjects/{teamId}");

            return result ?? new();
        }

        public async Task<List<ProjectModel>> GetListByEnvIdAsync(int envClusterId)
        {
            var result = await CallerProvider.GetAsync<List<ProjectModel>>($"/api/v1/{envClusterId}/project");

            return result ?? new();
        }

        public async Task<ProjectDetailModel> GetAsync(int Id)
        {
            var result = await CallerProvider.GetAsync<ProjectDetailModel>($"{_prefix}/{Id}");

            return result ?? new();
        }

        public async Task<List<ProjectTypeModel>> GetProjectTypesAsync()
        {
            var result = await CallerProvider.GetAsync<List<ProjectTypeModel>>($"{_prefix}/projectType");

            return result ?? new();
        }

        public async Task<List<ProjectModel>> GetProjectsAsync()
        {
            var result = await CallerProvider.GetAsync<List<ProjectModel>>("http://localhost:19401/api/v1/projects");

            return result ?? new();
        }
    }
}
