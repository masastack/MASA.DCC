// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Overview : IDisposable
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
        private bool _configDisabled = true;
        private StringNumber _selectedEnvId = 0;
        private StringNumber _selectEnvClusterId = 0;
        private List<EnvironmentModel> _environments = new();
        private List<ClusterModel> _clusters = new();
        private ConfigComponentModel _configModel = new();
        private int _selectProjectId;
        private App? _app;
        private Config? _config;
        private ProjectList? _projectList;

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
        }

        private async Task TabValueChangedAsync(StringNumber value)
        {
            _curTab = value;

            StateHasChanged();

            if (_curTab == 0 && _projectList != null)
            {
                await _projectList.InitDataAsync();
            }
            if (_curTab == 1 && _app != null)
            {
                await _app.InitDataAsync();
            }
            else if (_curTab == 2 && _config != null)
            {
                await _config.InitDataAsync(_configModel);
            }
        }

        private async Task NavigateToAppAsync(int projectId)
        {
            if (projectId != _selectProjectId)
                _configDisabled = true;

            _teamDetailDisabled = false;
            _selectProjectId = projectId;

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

            _selectProjectId = model.ProjectId;
            _configModel = model;

            await TabValueChangedAsync(2);
        }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= HandleLocationChanged;
        }
    }
}
