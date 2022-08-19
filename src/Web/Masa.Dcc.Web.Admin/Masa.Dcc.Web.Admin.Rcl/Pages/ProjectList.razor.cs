// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class ProjectList
    {
        [Parameter]
        public EventCallback<int> OnNameClick { get; set; }

        [Parameter]
        public EventCallback<NavigateToConfigModel> OnAppCardClick { get; set; }

        [Parameter]
        public EventCallback<Model.AppModel> OnAppPinClick { get; set; }

        [Parameter]
        public bool LandscapePage { get; set; } = true;

        [Parameter]
        public int EnvironmentClusterId { get; set; }

        [Parameter]
        public Guid TeamId { get; set; }

        [Inject]
        public ProjectCaller ProjectCaller { get; set; } = default!;

        [Inject]
        public AppCaller AppCaller { get; set; } = default!;

        private int _internalEnvironmentClusterId;
        private Guid _internalTeamId;
        public List<ProjectModel> _projects = new();
        private List<ProjectModel> _backupProjects = new();
        public List<TeamModel> _allTeams = new();
        private List<Model.AppModel> _apps = new();
        private ProjectDetailModel _projectDetail = new();
        private UserPortraitModel _userInfo = new();
        private List<int> _disableEnvironmentClusterIds = new();
        private bool _showProcess;

        protected override async Task OnParametersSetAsync()
        {
            if (EnvironmentClusterId != _internalEnvironmentClusterId || TeamId != _internalTeamId)
            {
                _showProcess = true;
                _internalEnvironmentClusterId = EnvironmentClusterId;
                _internalTeamId = TeamId;

                try
                {
                    await InitDataAsync();
                }
                finally
                {
                    _showProcess = false;
                }
            }
        }

        private async Task InitDataAsync()
        {
            if (LandscapePage)
            {
                if (EnvironmentClusterId == 0)
                {
                    _projects.Clear();
                    _backupProjects.Clear();
                }
                else
                {
                    _projects = await ProjectCaller.GetListByEnvIdAsync(EnvironmentClusterId);
                }
            }
            else
            {
                _projects = await ProjectCaller.GetListByTeamIdAsync(new List<Guid> { TeamId });
            }

            _allTeams = await AuthClient.TeamService.GetAllAsync();
            _backupProjects = new List<ProjectModel>(_projects.ToArray());
            _apps = await GetAppByProjectIdAsync(_projects.Select(p => p.Id));
        }

        private async Task<List<Model.AppModel>> GetAppByProjectIdAsync(IEnumerable<int> projectIds)
        {
            var apps = await AppCaller.GetListByProjectIdsAsync(projectIds.ToList());
            var appPins = await AppCaller.GetAppPinListAsync();

            var result = from app in apps
                         join appPin in appPins on app.Id equals appPin.AppId into appGroup
                         from newApp in appGroup.DefaultIfEmpty<AppPinDto>()
                         select new Model.AppModel
                         {
                             ProjectId = app.ProjectId,
                             Id = app.Id,
                             Name = app.Name,
                             Identity = app.Identity,
                             Description = app.Description,
                             Type = app.Type,
                             ServiceType = app.ServiceType,
                             Url = app.Url,
                             SwaggerUrl = app.SwaggerUrl,
                             EnvironmentClusters = app.EnvironmentClusters,
                             IsPinned = newApp != null
                         };

            return result.OrderByDescending(app => app.IsPinned).ThenByDescending(app => app.ModificationTime).ToList();
        }

        private async Task HandleProjectNameClick(int projectId)
        {
            if (OnNameClick.HasDelegate)
                await OnNameClick.InvokeAsync(projectId);
        }

        private async Task HandleAppCardClick(NavigateToConfigModel model)
        {
            if (OnAppCardClick.HasDelegate)
                await OnAppCardClick.InvokeAsync(model);
        }

        private async Task HandleAppPinClick(Model.AppModel model)
        {
            if (OnAppPinClick.HasDelegate)
                await OnAppPinClick.InvokeAsync(model);
        }
    }
}
