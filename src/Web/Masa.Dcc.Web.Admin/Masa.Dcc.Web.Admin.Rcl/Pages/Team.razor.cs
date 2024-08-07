﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Team : IDisposable
    {
        [Inject]
        public AppCaller AppCaller { get; set; } = default!;

        [Inject]
        public GlobalConfig GlobalConfig { get; set; } = default!;


        private int _projectCount;
        private StringNumber _curTab = 0;
        private bool _teamDetailDisabled = true;
        private bool _configDisabled = true;
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
                var teamId = GlobalConfig.CurrentTeamId == Guid.Empty ? MasaUser.CurrentTeamId : GlobalConfig.CurrentTeamId;
                HandleCurrentTeamChanged(teamId);
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

        private async Task SearchProject()
        {
            if (_projectListComponent != null)
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

        public new void Dispose()
        {
            GlobalConfig.OnCurrentTeamChanged -= HandleCurrentTeamChanged;
            base.DisposeAsyncCore();
        }
    }
}
