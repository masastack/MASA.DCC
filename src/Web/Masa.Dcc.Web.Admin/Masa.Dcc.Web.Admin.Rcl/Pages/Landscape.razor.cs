// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Landscape : IDisposable
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
        public NavigationManager NavigationManager { get; set; } = default!;

        private StringNumber _curTab = 0;
        private bool _teamDetailDisabled = true;
        private bool _configDisabled = true;
        private StringNumber _selectedEnvId = 0;
        private StringNumber _selectEnvClusterId = 0;
        private List<EnvironmentModel> _environments = new();
        private List<ClusterModel> _clusters = new();
        private List<ProjectModel> _projects = new();
        private List<Model.AppModel> _apps = new();
        private ConfigComponentModel _configModel = new();
        private AppComponentModel _appModel = new();
        private App? _app;
        private Config? _config;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                NavigationManager.LocationChanged += HandleLocationChanged;

                _environments = await EnvironmentCaller.GetListAsync();
                if (_environments.Any())
                {
                    _selectedEnvId = _environments[0].Id;
                    _clusters = await GetClustersByEnvIdAsync(_environments[0].Id);

                    StateHasChanged();
                }
            }
        }

        private async void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
        {
            await TabValueChangedAsync(0);

            await InvokeAsync(StateHasChanged);
        }

        private async Task<List<ClusterModel>> GetClustersByEnvIdAsync(int envId, bool isFetchProjects = true)
        {
            _selectedEnvId = envId;
            _clusters = await ClusterCaller.GetListByEnvIdAsync(envId);
            if (_clusters.Any() && isFetchProjects)
            {
                _selectEnvClusterId = _clusters[0].EnvironmentClusterId;
            }
            else
            {
                _selectEnvClusterId = 0;
            }

            return _clusters;
        }

        private async Task<List<ProjectModel>> GetProjectByEnvClusterIdAsync(int envClusterId)
        {
            _selectEnvClusterId = envClusterId;
            _projects = await ProjectCaller.GetListByEnvIdAsync(envClusterId);
            if (_projects.Any())
            {
                var projectIds = _projects.Select(project => project.Id);
                _apps = await GetAppByProjectIdAsync(projectIds);
            }

            return _projects;
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

        private async Task AppPin(Model.AppModel app)
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
            if (projectId != _appModel.ProjectId)
                _configDisabled = true;

            _teamDetailDisabled = false;
            _appModel = new(projectId);

            await TabValueChangedAsync(1);
        }

        public async Task AppNavigateToConfigAsync(ConfigComponentModel model)
        {
            _teamDetailDisabled = false;
            _configDisabled = false;
            _configModel = model;

            await TabValueChangedAsync(2);
        }

        public async Task NavigateToConfigAsync(ConfigComponentModel model)
        {
            _teamDetailDisabled = false;
            _configDisabled = false;

            _appModel = new(model.ProjectId);
            _configModel = model;

            await TabValueChangedAsync(2);
        }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= HandleLocationChanged;
        }
    }
}
