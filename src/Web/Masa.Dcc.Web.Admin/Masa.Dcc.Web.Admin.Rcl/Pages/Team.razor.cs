// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.StackSdks.Auth.Contracts;
using Masa.Stack.Components.Configs;
using Microsoft.AspNetCore.Http;

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Team : IDisposable
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

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public GlobalConfig GlobalConfig { get; set; } = default!;

        [Inject]
        public MasaUser MasaUser { get; set; } = default!;

        [Inject]
        public HttpContextAccessor HttpContextAccessor { get; set; }

        private int _projectCount;
        private StringNumber _curTab = 0;
        private bool _teamDetailDisabled = true;
        private bool _configDisabled = true;
        private List<Model.AppModel> _apps = new();
        private string _projectName = "";
        private ConfigComponentModel _configModel = new();
        private AppComponentModel _appModel = new();
        private App? _app;
        private Config? _config;
        private ProjectList? _projectListComponent;
        private TeamDetailModel _userTeam = new();
        private Guid _teamId;

        protected override Task OnInitializedAsync()
        {
            GlobalConfig.OnCurrentTeamChanged += HandleCurrentTeamChanged;
            return base.OnInitializedAsync();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                HandleCurrentTeamChanged(MasaUser.CurrentTeamId);
            }
        }

        private async void HandleCurrentTeamChanged(Guid teamId)
        {
            if (teamId != _userTeam.Id)
            {
                _teamId = teamId;
                await TabValueChangedAsync(0);
                _userTeam = await AuthClient.TeamService.GetDetailAsync(teamId) ?? new();
            }

            StateHasChanged();
        }

        private void ProjectCountHandler(int projectCount)
        {
            _projectCount = projectCount;
        }

        private async Task SearchProject(KeyboardEventArgs args)
        {
            if (args.Key == "Enter" && _projectListComponent != null)
            {
                await _projectListComponent.SearchProjectsByNameAsync(_projectName);
            }
        }

        private async Task TabValueChangedAsync(StringNumber value)
        {
            _curTab = value;
            StateHasChanged();

            if (_curTab == 0 && _projectListComponent != null)
            {
                await _projectListComponent.InitDataAsync();
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
            _teamDetailDisabled = false;
            _configDisabled = true;

            var apps = _apps.Where(app => app.ProjectId == projectId);
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

            _configModel = model;
            _appModel = new(model.ProjectId);

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
        }

        public void Dispose()
        {
            GlobalConfig.OnCurrentTeamChanged -= HandleCurrentTeamChanged;
        }
    }
}
