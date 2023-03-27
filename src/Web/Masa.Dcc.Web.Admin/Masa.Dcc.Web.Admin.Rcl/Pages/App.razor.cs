// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class App
    {
        [Parameter]
        public int ProjectId { get; set; }

        [Parameter]
        public int AppCount { get; set; }

        [CascadingParameter]
        public Overview? Overview { get; set; }

        [CascadingParameter]
        public Team? Team { get; set; }

        [Inject]
        public ConfigObjectCaller ConfigObjectCaller { get; set; } = default!;

        [Inject]
        public IPopupService PopupService { get; set; } = default!;

        [Inject]
        public AppCaller AppCaller { get; set; } = default!;

        [Inject]
        public ProjectCaller ProjectCaller { get; set; } = default!;

        [Inject]
        public ClusterCaller ClusterCaller { get; set; } = default!;

        private ProjectDetailModel _projectDetail = new();
        private TeamDetailModel _projectTeamDetail = new();
        private List<EnvironmentClusterModel> _projectEnvClusters = new();
        private List<EnvironmentClusterModel> _allEnvClusters = new();
        private bool _isEditBiz;
        private BizModel _bizDetail = new();
        private string _bizConfigName = "";
        private string _appName = "";
        private List<Model.AppModel> _apps = new();
        private List<Model.AppModel> _backupApps = new();
        private readonly Func<string, StringBoolean> _requiredRule = value => !string.IsNullOrEmpty(value) ? true : "Required";
        private readonly Func<string, StringBoolean> _counterRule = value => (value.Length <= 25 && value.Length > 0) ? true : "Biz config name length range is [1-25]";
        private readonly Func<string, StringBoolean> _strRule = value =>
        {
            Regex regex = new Regex(@"^[\u4E00-\u9FA5A-Za-z0-9`~!@#%^&*()_\-+=<>?:""{}|,.\/;'\\[\]·~！￥%……&*（）——《》？：“”【】、；‘’，。]+$");
            if (!regex.IsMatch(value))
            {
                return "Special symbols are not allowed";
            }
            else
            {
                return true;
            }
        };
        private bool _showProcess = true;

        private IEnumerable<Func<string, StringBoolean>> BizNameRules => new List<Func<string, StringBoolean>>
        {
            _requiredRule,
            _counterRule,
            _strRule
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InitDataAsync();
            }
        }

        public async Task InitDataAsync()
        {
            _showProcess = true;
            _apps.Clear();

            try
            {
                _apps = await GetAppByProjectIdAsync(new List<int> { ProjectId });
            }
            finally
            {
                _showProcess = false;
            }

            _allEnvClusters = await ClusterCaller.GetEnvironmentClustersAsync();
            _projectDetail = await ProjectCaller.GetAsync(ProjectId);
            _projectTeamDetail = await AuthClient.TeamService.GetDetailAsync(_projectDetail.TeamId) ?? new();

            _backupApps = new List<Model.AppModel>(_apps.ToArray());
            _projectEnvClusters = _allEnvClusters.Where(envCluster => _projectDetail.EnvironmentClusterIds.Contains(envCluster.Id)).ToList();

            //init biz config
            var bizConfig = await ConfigObjectCaller.GetBizConfigAsync($"{_projectDetail.Identity}-$biz");
            if (bizConfig.Id == 0)
            {
                bizConfig = await ConfigObjectCaller.AddBizConfigAsync(new AddObjectConfigDto
                {
                    Name = "Biz",
                    Identity = $"{_projectDetail.Identity}-$biz"
                });
            }
            _bizDetail = bizConfig.Adapt<BizModel>();
            _bizDetail.EnvironmentClusters = _projectEnvClusters;
            _bizConfigName = bizConfig.Name;

            StateHasChanged();
        }

        private async Task UpdateBizAsync()
        {
            foreach (var ruleFunc in BizNameRules)
            {
                var value = ruleFunc.Invoke(_bizDetail.Name).Value;
                if (value is string)
                {
                    return;
                }
            }

            _isEditBiz = !_isEditBiz;

            if (!_isEditBiz && _bizConfigName != _bizDetail.Name)
            {
                var bizConfigDto = await ConfigObjectCaller.UpdateBizConfigAsync(new UpdateObjectConfigDto
                {
                    Id = _bizDetail.Id,
                    Name = _bizDetail.Name
                });
                _bizConfigName = _bizDetail.Name;
                _bizDetail = bizConfigDto.Adapt<BizModel>();
                await PopupService.EnqueueSnackbarAsync(T("Edit succeeded"), AlertTypes.Success);
            }
        }

        private void CancelEditBizName()
        {
            _bizDetail.Name = _bizConfigName;
            _isEditBiz = !_isEditBiz;
        }

        private async Task NavigateToConfigAsync(ConfigComponentModel model)
        {
            model.ProjectIdentity = _projectDetail.Identity;
            if (Overview != null)
            {
                await Overview.AppNavigateToConfigAsync(model);
            }
            else if (Team != null)
            {
                await Team.AppNavigateToConfigAsync(model);
            }
        }

        private void SearchApp(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                if (!string.IsNullOrWhiteSpace(_appName))
                {
                    _apps = _backupApps.Where(app => app.Name.ToLower().Contains(_appName.ToLower())).ToList();
                }
                else
                {
                    _apps = _backupApps;
                }
            }
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

            return result.OrderByDescending(app => app.IsPinned)
                .ThenByDescending(app => app.PinTime)
                .ThenByDescending(app => app.ModificationTime)
                .ToList();
        }

        private async Task AppDetailPinAsync(Model.AppModel app)
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
            _backupApps = new List<Model.AppModel>(_apps.ToArray());
        }
    }
}
