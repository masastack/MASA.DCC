// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class ProjectList
    {
        [Parameter]
        public EventCallback<int> OnNameClick { get; set; }

        [Parameter]
        public EventCallback<ConfigComponentModel> OnAppCardClick { get; set; }

        [Parameter]
        public EventCallback<Model.AppModel> OnAppPinClick { get; set; }

        [Parameter]
        public bool LandscapePage { get; set; }

        [Parameter]
        public int EnvironmentClusterId { get; set; }

        [Parameter]
        public Guid TeamId { get; set; }

        [Parameter]
        public EventCallback<int> FetchProjectCount { get; set; }

        [Parameter]
        public IMultiEnvironmentUserContext MultiEnvironmentUserContext { get; set; } = default!;

        [Inject]
        public ProjectCaller ProjectCaller { get; set; } = default!;

        [Inject]
        public AppCaller AppCaller { get; set; } = default!;

        [Inject]
        public ConfigObjectCaller ConfigObjectCaller { get; set; } = default!;

        private int _internalEnvironmentClusterId;
        private Guid _internalTeamId;
        private List<ProjectModel> _projects = new();
        private List<ProjectModel> _backupProjects = new();
        private List<TeamModel> _allTeams = new();
        private List<Model.AppModel> _apps = new();
        private bool _showProcess = false;
        private List<LatestReleaseConfigModel> _appLatestReleaseConfig = new();
        private List<LatestReleaseConfigModel> _bizLatestReleaseConfig = new();

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

                    if (FetchProjectCount.HasDelegate)
                    {
                        await FetchProjectCount.InvokeAsync(_projects.Count);
                    }
                }
                finally
                {
                    _showProcess = false;
                }
            }
        }

        public async Task InitDataAsync()
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

            _allTeams = await AuthClient.TeamService.GetAllAsync(MultiEnvironmentUserContext.Environment ?? "");
            _backupProjects = new List<ProjectModel>(_projects.ToArray());
            var projectIds = _projects.Select(p => p.Id).ToList();
            _apps = await GetAppByProjectIdAsync(projectIds);
            int? emvClusterId = !LandscapePage || EnvironmentClusterId <= 0 ? null : EnvironmentClusterId;
            _appLatestReleaseConfig = await AppCaller.GetLatestReleaseConfigAsync(new LatestReleaseConfigRequestDto<int>()
            {
                Items = _apps.Select(x => x.Id).ToList(),
                EnvClusterId = emvClusterId
            });
            _bizLatestReleaseConfig = await ConfigObjectCaller.GetLatestReleaseConfigAsync(
                new LatestReleaseConfigRequestDto<ProjectModel>()
                {
                    Items = _projects,
                    EnvClusterId = emvClusterId
                });
        }

        private async Task<List<Model.AppModel>> GetAppByProjectIdAsync(IEnumerable<int> projectIds)
        {
            var apps = await AppCaller.GetListByProjectIdsAsync(projectIds.ToList());

            var appPins = await AppCaller.GetAppPinListAsync(apps.Select(app => app.Id).ToList());

            var result = from app in apps
                         join appPin in appPins on app.Id equals appPin.AppId into appGroup
                         from newApp in appGroup.DefaultIfEmpty()
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
                             ModificationTime = app.ModificationTime,
                             IsPinned = newApp != null && !newApp.IsDeleted,
                             PinTime = newApp != null ? newApp.ModificationTime : DateTime.MinValue
                         };

            result = result.Where(app => EnvironmentClusterId == 0 || app.EnvironmentClusters.Any(ec => ec.Id == EnvironmentClusterId))
                .GroupBy(x => x.ProjectId).SelectMany(g => g.OrderByDescending(app => app.IsPinned)
                .ThenByDescending(app => app.PinTime)
                .ThenByDescending(app => app.ModificationTime));

            return result.ToList();
        }

        private async Task HandleProjectNameClick(int projectId)
        {
            if (OnNameClick.HasDelegate)
            {
                await OnNameClick.InvokeAsync(projectId);
            }
        }

        private async Task HandleAppCardClick(ConfigComponentModel model)
        {
            if (OnAppCardClick.HasDelegate)
            {
                model.ProjectIdentity = _projects.Find(project => project.Id == model.ProjectId)?.Identity ?? "";
                await OnAppCardClick.InvokeAsync(model);

                //init biz config
                var bizConfig = await ConfigObjectCaller.GetBizConfigAsync($"{model.ProjectIdentity}{DccConst.BizConfigSuffix}");
                if (bizConfig.Id == 0)
                {
                    bizConfig = await ConfigObjectCaller.AddBizConfigAsync(new AddObjectConfigDto
                    {
                        Name = "Biz",
                        Identity = $"{model.ProjectIdentity}{DccConst.BizConfigSuffix}"
                    });
                }
            }
        }

        private async Task HandleAppPinClick(Model.AppModel model)
        {
            if (OnAppPinClick.HasDelegate)
            {
                await OnAppPinClick.InvokeAsync(model);
                var apps = await GetAppByProjectIdAsync(new List<int>() { model.ProjectId });
                _apps.RemoveAll(app => apps.Select(appModel => appModel.Id).Contains(app.Id));
                _apps.AddRange(apps);
            }
        }

        public async Task SearchProjectsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await InitDataAsync();
            }
            else
            {
                _projects = _backupProjects.Where(project => project.Name.ToLower().Contains(name.ToLower())).ToList();
            }
        }
    }
}
