// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components.Web;

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Team
    {
        [Inject]
        public IPopupService PopupService { get; set; } = default!;

        [Inject]
        public EnvironmentCaller EnvironmentCaller { get; set; } = default!;

        [Inject]
        public ClusterCaller ClusterCaller { get; set; } = default!;

        [Inject]
        public ProjectCaller ProjectCaller { get; set; } = default!;

        [Inject]
        public AppCaller AppCaller { get; set; } = default!;

        private StringNumber _curTab = 0;
        private bool _teamDetailDisabled = true;
        private List<ProjectModel> _projects = new();
        private List<AppDetailModel> _apps = new();
        private string _projectName = "";
        private ProjectDetailModel _projectDetail = new();
        private List<EnvironmentClusterModel> allEnvClusters = new();
        private int _selectProjectId;
        private int _appCount;
        private List<AppDetailModel> _projectApps = new();
        private string _appName = "";
        private List<EnvironmentClusterModel> _projectEnvClusters = new();
        private AppDetailModel _appDetail = new();
        private List<ProjectTypeModel> _projectTypes = new();

        public Guid TeamId { get; set; } = Guid.Empty;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InitDataAsync();
                StateHasChanged();
            }
        }

        private async Task InitDataAsync()
        {
            _projects = await ProjectCaller.GetListByTeamIdAsync(TeamId);
            var projectIds = _projects.Select(project => project.Id).ToList();
            _apps = await AppCaller.GetListByProjectIdAsync(projectIds);
        }

        private async Task SearchProject(KeyboardEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(_projectName))
            {
                _projects = _projects.Where(project => project.Name.ToLower().Contains(_projectName.ToLower())).ToList();
            }
            else
            {
                await InitDataAsync();
            }
        }

        private async Task<ProjectDetailModel> GetProjectAsync(int projectId)
        {
            _projectDetail = await ProjectCaller.GetAsync(projectId);

            return _projectDetail;
        }

        private async Task GetProjectDetailAsync(int projectId, int appCount)
        {
            _teamDetailDisabled = false;
            _curTab = 1;

            allEnvClusters = await ClusterCaller.GetEnvironmentClusters();
            _selectProjectId = projectId;
            _appCount = appCount;
            _projectApps = _apps.Where(app => app.ProjectId == projectId).ToList();
            await GetProjectAsync(projectId);
        }

        private void SearchApp()
        {
            if (!string.IsNullOrWhiteSpace(_appName))
            {
                _projectApps = _projectApps.Where(app => app.Name.ToLower().Contains(_appName.ToLower())).ToList();
            }
            else
            {
                _projectApps = _apps.Where(app => app.ProjectId == _selectProjectId).ToList();
            }
        }
    }
}
