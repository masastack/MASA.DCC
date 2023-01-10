// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages.Modal
{
    public partial class CloneModal
    {
        [Parameter]
        public List<ConfigObjectModel> ConfigObjects { get; set; } = new();

        [Parameter]
        public EnvironmentClusterModel SelectCluster { get; set; } = new();

        [Parameter]
        public ConfigObjectType ConfigObjectType { get; set; }

        [Parameter]
        public AppDetailModel AppDetail { get; set; } = new();

        [Parameter]
        public ProjectDetailModel ProjectDetail { get; set; } = new();

        [Parameter]
        public EventCallback OnSubmitAfter { get; set; }

        [Parameter]
        public bool Value { get; set; }

        [Inject]
        public ProjectCaller ProjectCaller { get; set; } = default!;

        [Inject]
        public AppCaller AppCaller { get; set; } = default!;

        [Inject]
        public ConfigObjectCaller ConfigObjectCaller { get; set; } = default!;

        [Inject]
        public ClusterCaller ClusterCaller { get; set; } = default!;

        [Inject]
        public IPopupService PopupService { get; set; } = default!;

        [Inject]
        public MasaUser MasaUser { get; set; } = default!;

        #region clone
        private ConfigObjectModel _selectConfigObject = new();
        private List<StringNumber> _selectEnvClusterIds = new();
        private int _step = 1;
        private bool _cloneAppSelect;
        private bool _cloneEnvSelect;
        private string _cloneAppCardStyle = "position: absolute; top:50%; margin-top:-65px; transition: 0.3s;";
        private string _cloneEnvCardStyle = "position: absolute; top:50%; margin-top:-65px; transition: 0.3s;";
        private List<ProjectModel> _allProjects = new();
        private int _cloneSelectProjectId = new();
        private List<AppDetailModel> _cloneApps = new();
        private int _cloneSelectAppId = new();
        private AppDetailModel _cloneSelectApp = new();
        private readonly List<ConfigObjectDto> _afterAllCloneConfigObjects = new();
        private readonly List<ConfigObjectDto> _afterSelectCloneConfigObjects = new();
        private bool _isCloneAll = true;//clone single config object or clone all config object

        public bool CloneConfigObjectAllChecked
        {
            get
            {
                var checkedConfigObejctCount = ConfigObjects.Where(c => c.IsChecked).Count();
                return checkedConfigObejctCount == ConfigObjects.Count;
            }
        }

        public bool IsNeedRebase
        {
            get => ConfigObjects.Where(c => c.IsNeedRebase).Any();
        }
        #endregion

        private async Task<List<ProjectModel>> GetProjectList()
        {
            return await ProjectCaller.GetProjectsAsync();
        }

        private async Task ProjectValueChangedAsync(int projectId)
        {
            if (projectId != _cloneSelectProjectId)
            {
                _cloneSelectAppId = 0;
            }
            _cloneSelectProjectId = projectId;

            if (ConfigObjectType == ConfigObjectType.Biz)
            {
                var project = _allProjects.Find(project => project.Id == projectId) ?? new();
                BizConfigDto bizConfig = await ConfigObjectCaller.GetBizConfigAsync($"{project.Identity}-$biz");
                if (bizConfig.Id == 0)
                {
                    bizConfig = await ConfigObjectCaller.AddBizConfigAsync(new AddObjectConfigDto
                    {
                        Name = "Biz",
                        Identity = $"{project.Identity}-$biz"
                    });
                }
                _cloneApps = new List<AppDetailModel>
                {
                    new AppDetailModel
                    {
                        Id = bizConfig.Id,
                        Name= bizConfig.Name,
                        Identity = bizConfig.Identity
                    }
                };
            }
            else if (ConfigObjectType == ConfigObjectType.App)
            {
                _cloneApps = await AppCaller.GetListByProjectIdsAsync(new List<int> { projectId });
                _cloneApps = _cloneApps.Where(app => app.Id != AppDetail.Id).ToList();
            }

            if (_cloneApps.Any())
            {
                _cloneSelectAppId = _cloneApps[0].Id;
            }
        }

        public async Task ShowCloneModalAsync(ConfigObjectModel? configObject = null)
        {
            if (configObject != null)
            {
                _isCloneAll = false;
                _selectConfigObject = configObject;
            }
            else
            {
                _isCloneAll = true;
            }

            if (!MasaUser.IsSuperAdmin)
            {
                ConfigObjects.RemoveAll(config => config.Encryption);
            }

            Value = true;
            _allProjects = await GetProjectList();
            if (ConfigObjectType == ConfigObjectType.Biz)
            {
                _allProjects.RemoveAll(project => project.Id == ProjectDetail.Id);
            }
        }

        private void SelectOtherApp()
        {
            _cloneAppSelect = !_cloneAppSelect;
            _cloneEnvSelect = false;
            _cloneAppCardStyle = _cloneAppSelect ? "position: absolute; top: 64px; transition: 0.3s;" : "position: absolute; top:50%; margin-top:-77px; transition: 0.3s;";
        }

        private void SelectOtherEnv()
        {
            _cloneEnvSelect = !_cloneEnvSelect;
            _cloneAppSelect = false;
            _cloneAppCardStyle = _cloneAppSelect ? "position: absolute; top: 64px; transition: 0.3s;" : "position: absolute; top:50%; margin-top:-77px; transition: 0.3s;";
        }

        private async Task CloneNextClick()
        {
            if (!_isCloneAll)
            {
                ConfigObjects.Clear();
                ConfigObjects.Add(_selectConfigObject);
            }

            _step = 2;

            if (_cloneAppSelect)
            {
                if (ConfigObjectType == ConfigObjectType.Biz)
                {
                    var allEnvClusters = await ClusterCaller.GetEnvironmentClustersAsync();
                    var peoject = await ProjectCaller.GetAsync(_cloneSelectProjectId);
                    var projectEnvClusters = allEnvClusters.Where(envCluster => peoject.EnvironmentClusterIds.Contains(envCluster.Id)).ToList();
                    _cloneSelectApp.EnvironmentClusters = projectEnvClusters;
                    _cloneSelectApp.Id = _cloneSelectAppId;
                }
                else if (ConfigObjectType == ConfigObjectType.App)
                {
                    _cloneSelectApp = await AppCaller.GetWithEnvironmentClusterAsync(_cloneSelectAppId);
                }
            }
            else if (_cloneEnvSelect)
            {
                _cloneSelectApp = AppDetail.Adapt<AppDetailModel>();
                _cloneSelectApp.EnvironmentClusters.RemoveAll(e => e.Id == SelectCluster.Id);
            }

            var envClusterIds = _cloneSelectApp.EnvironmentClusters.Select(ec => (StringNumber)ec.Id).ToList();
            if (!envClusterIds.Any())
            {
                await PopupService.AlertAsync(T("There is no environment cluster to clone under the current application"), AlertTypes.Error);
                return;
            }

            _selectEnvClusterIds = envClusterIds;
            CloneEnvClusterValueChanged(envClusterIds);

            _afterAllCloneConfigObjects.Clear();
            foreach (var item in _cloneSelectApp.EnvironmentClusters)
            {
                var configObjects = await ConfigObjectCaller.GetConfigObjectsAsync(item.Id, _cloneSelectApp.Id, ConfigObjectType);
                _afterAllCloneConfigObjects.AddRange(configObjects);
            }
        }

        private void ClonePrevClick()
        {
            _step = 1;
        }

        private void CloneEnvClusterValueChanged(List<StringNumber> envClusterIds)
        {
            _selectEnvClusterIds = envClusterIds;

            _afterSelectCloneConfigObjects.Clear();
            _selectEnvClusterIds.ForEach(envClusterId =>
            {
                var configObjects = _afterAllCloneConfigObjects.Where(c => c.EnvironmentClusterId == envClusterId);
                _afterSelectCloneConfigObjects.AddRange(configObjects);
            });

            if (!_afterSelectCloneConfigObjects.Any())
            {
                ConfigObjects.ForEach(configObject =>
                {
                    configObject.IsNeedRebase = false;
                });
            }
            else
            {
                var checkedConfigObject = ConfigObjects.Where(c => c.IsChecked);
                var needRebaseConfigObjects = checkedConfigObject
                    .IntersectBy(_afterSelectCloneConfigObjects.Select(c => c.Name), c => c.Name)
                    .ToList();

                IntersectByConfigObjectName(checkedConfigObject, _afterSelectCloneConfigObjects);
            }
        }

        private void CloneConfigObjectCheckValueChanged(ConfigObjectModel configObject, bool value)
        {
            configObject.IsChecked = value;
            if (configObject.IsChecked)
            {
                foreach (var afterCloneConfigObject in _afterSelectCloneConfigObjects)
                {
                    if (configObject.Name.Equals(afterCloneConfigObject.Name))
                    {
                        configObject.IsNeedRebase = true;
                        break;
                    }
                }
            }
            else
            {
                configObject.IsNeedRebase = false;
            }
        }

        private static void CloneConfigObjectCoverClick(ConfigObjectModel configObject)
        {
            configObject.IsNeedCover = true;
            configObject.IsNeedRebase = false;
        }

        private void CloneConfigObjectAllCheckedChanged(bool value)
        {
            if (value)
            {
                ConfigObjects.ForEach(configObject =>
                {
                    configObject.IsChecked = value;
                });

                IntersectByConfigObjectName(ConfigObjects, _afterSelectCloneConfigObjects);
            }
            else
            {
                ConfigObjects.ForEach(configObject =>
                {
                    configObject.IsChecked = value;
                    configObject.IsNeedRebase = value;
                });
            }
        }

        private void IntersectByConfigObjectName(IEnumerable<ConfigObjectModel> needCloneSelectConfigObjects, IEnumerable<ConfigObjectDto> afterSelectCloneConfigObjects)
        {
            var needRebaseConfigObjects = needCloneSelectConfigObjects
                    .IntersectBy(afterSelectCloneConfigObjects.Select(c => c.Name), c => c.Name).ToList();
            if (needRebaseConfigObjects.Any())
            {
                needRebaseConfigObjects.ForEach(configObject =>
                {
                    configObject.IsNeedRebase = true;
                });
            }
            else
            {
                ConfigObjects.ForEach(configObject =>
                {
                    configObject.IsNeedRebase = false;
                });
            }
        }

        private void CloneConfigObjectNameChanged(ConfigObjectModel configObject, string configObjectName)
        {
            configObject.Name = configObjectName;
            foreach (var afterConfigObject in _afterSelectCloneConfigObjects)
            {
                if (configObject.Name.Equals(afterConfigObject.Name))
                {
                    configObject.IsNeedRebase = true;
                    break;
                }
                else
                {
                    configObject.IsNeedRebase = false;
                }
            }
        }

        private static void RemoveProperty(string key, ConfigObjectModel model)
        {
            model.ConfigObjectPropertyContents.RemoveAll(prop => prop.Key == key);
        }

        private async Task CloneAsync()
        {
            if (!_selectEnvClusterIds.Any())
            {
                await PopupService.AlertAsync(T("Please select environment/cluster"), AlertTypes.Error);
                return;
            }

            var configObjects = ConfigObjects.Where(c => c.IsChecked);
            if (!configObjects.Any())
            {
                await PopupService.AlertAsync(T("Please select the configuration to clone"), AlertTypes.Error);
                return;
            }

            var dto = new CloneConfigObjectDto
            {
                ToObjectId = _cloneSelectApp.Id,
                ConfigObjectType = ConfigObjectType
            };
            foreach (var envClusterId in _selectEnvClusterIds)
            {
                foreach (var configObject in configObjects)
                {
                    var format = configObject.FormatLabelCode.ToLower();
                    string initialContent;
                    string content;
                    if (format == "properties")
                    {
                        initialContent = "[]";
                        content = JsonSerializer.Serialize(configObject.ConfigObjectPropertyContents);
                    }
                    else if (format == "json")
                    {
                        initialContent = "{}";
                        content = configObject.Content;
                    }
                    else
                    {
                        initialContent = "";
                        content = configObject.Content;
                    }

                    if (configObject.IsNeedCover)
                    {
                        dto.CoverConfigObjects.Add(new AddConfigObjectDto
                        {
                            Name = configObject.Name,
                            FormatLabelCode = configObject.FormatLabelCode,
                            Type = configObject.Type,
                            ObjectId = configObject.Id,
                            EnvironmentClusterId = envClusterId.AsT1,
                            Content = content,
                            TempContent = initialContent,
                            Encryption = configObject.Encryption
                        });
                    }
                    else
                    {
                        dto.ConfigObjects.Add(new AddConfigObjectDto
                        {
                            Name = configObject.Name,
                            FormatLabelCode = configObject.FormatLabelCode,
                            Type = configObject.Type,
                            ObjectId = configObject.Id,
                            EnvironmentClusterId = envClusterId.AsT1,
                            Content = content,
                            TempContent = initialContent,
                            Encryption = configObject.Encryption
                        });
                    }
                }
            }
            await ConfigObjectCaller.CloneAsync(dto);

            await CloneDialogValueChangedAsync(false);
        }

        private async Task CloneDialogValueChangedAsync(bool value)
        {
            Value = value;
            if (!value)
            {
                _cloneAppCardStyle = "position: absolute; top:50%; margin-top:-65px; transition: 0.3s;";
                _cloneEnvCardStyle = "position: absolute; top:50%; margin-top:-65px; transition: 0.3s;";
                _cloneAppSelect = false;
                _cloneEnvSelect = false;
                _cloneSelectProjectId = 0;
                _cloneSelectAppId = 0;
                _selectEnvClusterIds.Clear();
                _afterSelectCloneConfigObjects.Clear();
                _selectConfigObject = new();
                if (OnSubmitAfter.HasDelegate)
                {
                    await OnSubmitAfter.InvokeAsync();
                }
                _step = 1;
            }
        }
    }
}
