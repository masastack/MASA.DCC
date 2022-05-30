// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

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

        [Inject]
        public ConfigObjectCaller ConfigObjecCaller { get; set; } = default!;

        [Inject]
        public LabelCaller LabelCaller { get; set; } = default!;

        private StringNumber _curTab = 0;
        private bool _teamDetailDisabled = true;
        private bool _configDisabled = true;
        private List<ProjectModel> _projects = new();
        private List<Model.AppModel> _apps = new();
        private string _projectName = "";
        private ConfigComponentModel _configModel = new();
        private AppComponentModel _appModel = new();
        private App? _app;
        private Config? _config;

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
            _apps = await GetAppByProjectIdAsync(projectIds);
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

        private async Task TabValueChangedAsync(StringNumber value)
        {
            _curTab = value;

            StateHasChanged();

            if (_curTab == 1 && _app != null)
            {
                await _app.InitDataAsync();
            }
            else if (_curTab == 2 && _config != null)
            {
                await _config.InitDataAsync();
            }
        }

        private async Task NavigateToAppAsync(int projectId)
        {
            _teamDetailDisabled = false;

            var apps = _apps.Where(app => app.ProjectId == projectId);
            _appModel = new(projectId, apps.Count());

            await TabValueChangedAsync(1);
        }

        public async Task AppNavigateToConfigAsync(NavigateToConfigModel model)
        {
            _teamDetailDisabled = false;
            _configDisabled = false;
            var project = _projects.First(project => project.Id == model.ProjectId);
            _configModel = new(model.AppId, model.EnvClusterId, model.ConfigObjectType, project.Identity, project.Id);

            await TabValueChangedAsync(2);
        }

        public async Task NavigateToConfigAsync(NavigateToConfigModel model)
        {
            _teamDetailDisabled = false;
            _configDisabled = false;

            var apps = _apps.Where(app => app.ProjectId == model.ProjectId);
            _appModel = new(model.ProjectId, apps.Count());

            var project = _projects.First(project => project.Id == model.ProjectId);
            _configModel = new(model.AppId, model.EnvClusterId, model.ConfigObjectType, project.Identity, project.Id);

            await TabValueChangedAsync(2);
        }

        private async Task AppPinAsync(Model.AppModel app)
        {
            if (app.IsPinned)
            {
                await AppCaller.RemoveAppPinAsync(app.Id);
            }
            else
            {
                await AppCaller.AddAppPinAsync(app.Id);
            }
            _apps = await GetAppByProjectIdAsync(new List<int>() { app.ProjectId });
        }
    }
}
