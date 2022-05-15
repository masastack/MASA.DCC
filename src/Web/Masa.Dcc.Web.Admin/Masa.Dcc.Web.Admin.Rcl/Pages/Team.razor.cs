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
        public ConfigObjecCaller ConfigObjecCaller { get; set; } = default!;

        private StringNumber _curTab = 0;
        private bool _teamDetailDisabled = true;
        private bool _configDisabled = true;
        private List<ProjectModel> _projects = new();
        private List<AppDto> _apps = new();
        private string _projectName = "";
        private ProjectDetailModel _projectDetail = new();
        private AppDto _appDetail = new();
        private List<EnvironmentClusterModel> _allAppEnvClusters = new();
        private List<EnvironmentClusterModel> _projectEnvClusters = new();
        private List<EnvironmentClusterModel> _allEnvClusters = new();
        private int _selectProjectId;
        private int _appCount;
        private List<AppDto> _projectApps = new();
        private string _appName = "";
        private bool _isEditBiz;
        private BizDto _bizDetail = new();
        private string _bizConfigName = "";
        private List<EnvironmentClusterModel> _appEnvs = new();
        private List<EnvironmentClusterModel> _appEnvClusters = new();
        private string _selectEnvName = "";
        private EnvironmentClusterModel _selectCluster = new();
        private int _selectEnvClusterId;
        private List<ConfigObjectDto> _configObjects = new();
        private ConfigObjectType _configObjectType;


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

        private async Task<List<AppDto>> GetAppByProjectIdAsync(IEnumerable<int> projectIds)
        {
            var apps = await AppCaller.GetListByProjectIdAsync(projectIds.ToList());
            var appPins = await AppCaller.GetAppPinListAsync();

            var result = from app in apps
                         join appPin in appPins on app.Id equals appPin.AppId into appGroup
                         from newApp in appGroup.DefaultIfEmpty()
                         select new AppDto
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

        private async Task<ProjectDetailModel> GetProjectAsync(int projectId)
        {
            _projectDetail = await ProjectCaller.GetAsync(projectId);

            return _projectDetail;
        }

        private async Task GetProjectDetailAsync(int projectId, int appCount)
        {
            _teamDetailDisabled = false;

            _selectProjectId = projectId;
            _appCount = appCount;

            await TabValueChangedAsync(1);
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

        private async Task TabValueChangedAsync(StringNumber value)
        {
            _curTab = value;
            int currentValue = value.ToInt32();

            if (currentValue == 0)
            {
                await InitDataAsync();
            }

            if (currentValue == 1)
            {
                _projectApps = _apps.Where(app => app.ProjectId == _selectProjectId).ToList();
                _allEnvClusters = await ClusterCaller.GetEnvironmentClustersAsync();
                _projectDetail = await GetProjectAsync(_selectProjectId);
                _projectEnvClusters = _allEnvClusters.Where(envCluster => _projectDetail.EnvironmentClusterIds.Contains(envCluster.Id)).ToList();

                //初始化业务配置
                var bizConfig = await ConfigObjecCaller.GetBizConfigAsync($"{_projectDetail.Identity}-$biz");
                if (bizConfig.Id == 0)
                {
                    bizConfig = await ConfigObjecCaller.AddBizConfigAsync(new AddObjectConfigDto
                    {
                        Name = "Biz",
                        Identity = $"{_projectDetail.Identity}-$biz"
                    });
                }

                _bizDetail = bizConfig.Adapt<BizDto>();
                _bizDetail.EnvironmentClusters = _projectEnvClusters;
                _bizConfigName = bizConfig.Name;
            }

            if (currentValue == 2)
            {
                _allEnvClusters = await ClusterCaller.GetEnvironmentClustersAsync();
                _allAppEnvClusters = _allEnvClusters.Where(
                        envCluster => _appDetail.EnvironmentClusters.Select(ec => ec.Id).Contains(envCluster.Id)
                    ).ToList();
                _appEnvs = _allAppEnvClusters.DistinctBy(env => env.EnvironmentName).ToList();

                var selectEnvCluster = _allAppEnvClusters.First(envCluster => envCluster.Id == _selectEnvClusterId);
                _selectEnvName = selectEnvCluster.EnvironmentName;
                _selectCluster = selectEnvCluster;
                await OnClusterChipClick(selectEnvCluster);

                _appEnvClusters = _allAppEnvClusters.Where(envCluster => envCluster.EnvironmentName == _selectEnvName)
                    .ToList();
            }
        }

        private async Task AppPinAsync(AppDto app)
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

        private async Task AppDetailPinAsync(AppDto app)
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
            _projectApps = _apps.Where(app => app.ProjectId == app.ProjectId).ToList();
        }

        private async Task NavigateToConfigAsync(int envClusterId, AppDto? appDto, ConfigObjectType configObjectType)
        {
            _configObjectType = configObjectType;
            _appDetail = configObjectType switch
            {
                ConfigObjectType.Biz => _bizDetail.Adapt<AppDto>(),
                ConfigObjectType.App => appDto!,
                ConfigObjectType.Public => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };

            _selectEnvClusterId = envClusterId;
            _curTab = 2;
            _teamDetailDisabled = false;
            _configDisabled = false;
            //_selectProjectId = appDto!.ProjectId;

            await TabValueChangedAsync(2);
        }

        private async void UpdateBizAsync()
        {
            _isEditBiz = !_isEditBiz;

            if (!_isEditBiz && _bizConfigName != _bizDetail.Name)
            {
                var bizConfigDto = await ConfigObjecCaller.UpdateBizConfigAsync(new UpdateObjectConfigDto
                {
                    Id = _bizDetail.Id,
                    Name = _bizDetail.Name
                });
                _bizDetail = bizConfigDto.Adapt<BizDto>();
                await PopupService.AlertAsync("修改成功", AlertTypes.Success);
            }
        }

        private async void OnEnvChipClick(string envName)
        {
            _selectEnvName = envName;
            _appEnvClusters = _allAppEnvClusters.Where(envCluster => envCluster.EnvironmentName == envName)
                    .ToList();

            await OnClusterChipClick(_appEnvClusters.First());
        }

        private async Task OnClusterChipClick(EnvironmentClusterModel model)
        {
            _selectCluster = model;

            await GetConfigObjectsAsync(_selectCluster.Id, _configObjectType);
        }

        private async Task GetConfigObjectsAsync(int envClusterId, ConfigObjectType configObjectType, string configObjectName = "")
        {
            _configObjects = await ConfigObjecCaller.GetConfigObjectsAsync(envClusterId, configObjectType, configObjectName);
            StateHasChanged();
        }
    }
}
