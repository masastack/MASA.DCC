// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System.Text;

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

        [Inject]
        public LabelCaller LabelCaller { get; set; } = default!;

        private StringNumber _curTab = 0;
        private bool _teamDetailDisabled = true;
        private bool _configDisabled = true;
        private List<ProjectModel> _projects = new();
        private List<Model.AppModel> _apps = new();
        private string _projectName = "";
        private ProjectDetailModel _projectDetail = new();
        private Model.AppModel _appDetail = new();
        private List<EnvironmentClusterModel> _allAppEnvClusters = new();
        private List<EnvironmentClusterModel> _projectEnvClusters = new();
        private List<EnvironmentClusterModel> _allEnvClusters = new();
        private int _selectProjectId;
        private int _appCount;
        private List<Model.AppModel> _projectApps = new();
        private string _appName = "";
        private bool _isEditBiz;
        private BizModel _bizDetail = new();
        private string _bizConfigName = "";
        private List<EnvironmentClusterModel> _appEnvs = new();
        private List<EnvironmentClusterModel> _appEnvClusters = new();
        private string _selectEnvName = "";
        private EnvironmentClusterModel _selectCluster = new();
        private int _selectEnvClusterId;
        private List<ConfigObjectModel> _configObjects = new();
        private ConfigObjectType _configObjectType;
        private List<LabelDto> _configObjectFormats = new();
        private readonly DataModal<AddConfigObjectDto> _addConfigObjectModal = new();
        private List<StringNumber> _selectEnvClusterIds = new();
        private readonly List<DataTableHeader<ConfigObjectPropertyModel>> _headers = new()
        {
            new (){ Text= "状态", Value= nameof(ConfigObjectPropertyModel.IsPublished)},
            new (){ Text= "Key", Value= nameof(ConfigObjectPropertyModel.Key)},
            new (){ Text= "Value", Value= nameof(ConfigObjectPropertyModel.Value)},
            new (){ Text= "描述", Value= nameof(ConfigObjectPropertyModel.Description)},
            new (){ Text= "修改人", Value= nameof(ConfigObjectPropertyModel.Modifier) },
            new (){ Text= "修改时间", Value= nameof(ConfigObjectPropertyModel.ModificationTime) },
            new (){ Text= "操作"}
        };
        private readonly DataModal<ConfigObjectPropertyContentDto, int> _propertyConfigModal = new();
        private List<ConfigObjectPropertyModel> _selectConfigObjectAllProperties = new();
        private readonly EditorContentModel _selectEditorContent = new();
        private StringNumber _tabIndex = 0;

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
            var apps = await AppCaller.GetListByProjectIdAsync(projectIds.ToList());
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

                _bizDetail = bizConfig.Adapt<BizModel>();
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
            _projectApps = _apps.Where(app => app.ProjectId == app.ProjectId).ToList();
        }

        private async Task NavigateToConfigAsync(int envClusterId, ConfigObjectType configObjectType, int? projectId = null, Model.AppModel? appDto = null)
        {
            _configObjectType = configObjectType;
            _appDetail = configObjectType switch
            {
                ConfigObjectType.Biz => _bizDetail.Adapt<Model.AppModel>(),
                ConfigObjectType.App => appDto!,
                ConfigObjectType.Public => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };

            _selectEnvClusterId = envClusterId;
            _curTab = 2;
            _teamDetailDisabled = false;
            _configDisabled = false;

            if (projectId != null)
            {
                _selectProjectId = projectId.Value;
            }

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
                _bizDetail = bizConfigDto.Adapt<BizModel>();
                await PopupService.ToastSuccessAsync("修改成功");
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
            var configObjects = await ConfigObjecCaller.GetConfigObjectsAsync(envClusterId, configObjectType, configObjectName);
            _configObjects = configObjects.Adapt<List<ConfigObjectModel>>();

            _configObjects.ForEach(config =>
            {
                if (config.FormatLabelCode.Trim().ToLower() == "properties")
                {
                    var contents = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(config.Content) ?? new();
                    var tempContents = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(config.TempContent) ?? new();

                    var deleted = tempContents.ExceptBy(contents.Select(content => content.Key), content => content.Key)
                        .Select(deletedContent => new ConfigObjectPropertyModel
                        {
                            Key = deletedContent.Key,
                            Value = deletedContent.Value,
                            Description = deletedContent.Description,
                            Modifier = deletedContent.Modifier,
                            ModificationTime = deletedContent.ModificationTime,
                            IsDeleted = true
                        })
                        .ToList();

                    var added = contents.ExceptBy(tempContents.Select(content => content.Key), content => content.Key)
                        .Select(addedContent => new ConfigObjectPropertyModel
                        {
                            Key = addedContent.Key,
                            Value = addedContent.Value,
                            Description = addedContent.Description,
                            Modifier = addedContent.Description,
                            ModificationTime = addedContent.ModificationTime,
                            IsAdded = true
                        }).ToList();

                    var published = contents
                        .IntersectBy(tempContents.Select(content => new { content.Key, content.Value }),
                            content => new { content.Key, content.Value })
                        .Select(publishedContent => new ConfigObjectPropertyModel
                        {
                            Key = publishedContent.Key,
                            Value = publishedContent.Value,
                            Description = publishedContent.Description,
                            Modifier = publishedContent.Modifier,
                            ModificationTime = publishedContent.ModificationTime,
                            IsPublished = true
                        }).ToList();

                    var publishedWithEdited = contents
                        .IntersectBy(tempContents.Select(content => content.Key), content => content.Key).ToList();
                    publishedWithEdited.RemoveAll(c => published.Select(d => d.Key).Contains(c.Key));
                    var edited = publishedWithEdited.Select(editedContent => new ConfigObjectPropertyModel
                    {
                        Key = editedContent.Key,
                        Value = editedContent.Value,
                        Description = editedContent.Description,
                        ModificationTime = editedContent.ModificationTime,
                        Modifier = editedContent.Modifier,
                        IsEdited = true
                    }).ToList();

                    config.ConfigObjectPropertyContents = published.Union(deleted).Union(added).Union(edited).ToList();
                }
            });

            StateHasChanged();
        }

        private async Task ShowAddConfigObjectModal()
        {
            _addConfigObjectModal.Show();
            _configObjectFormats = await LabelCaller.GetLabelsByTypeCodeAsync("ConfigObjectFormat");
        }

        private async Task AddConfigObject()
        {
            string initialContent = (_addConfigObjectModal.Data.FormatLabelCode.ToLower()) switch
            {
                "json" => "{}",
                "properties" => "[]",
                _ => "",
            };

            List<AddConfigObjectDto> configObjectDtos = new();
            for (int i = 0; i < _selectEnvClusterIds.Count; i++)
            {
                configObjectDtos.Add(new AddConfigObjectDto
                {
                    Name = _addConfigObjectModal.Data.Name,
                    FormatLabelCode = _addConfigObjectModal.Data.FormatLabelCode,
                    Type = _configObjectType,
                    ObjectId = _appDetail.Id,
                    EnvironmentClusterId = _selectEnvClusterIds[i].AsT1,
                    Content = initialContent,
                    TempContent = initialContent
                });
            }

            await ConfigObjecCaller.AddConfigObjectAsync(configObjectDtos);
            var configObjects = await ConfigObjecCaller.GetConfigObjectsAsync(_selectEnvClusterId, _configObjectType, "");
            _configObjects = configObjects.Adapt<List<ConfigObjectModel>>();

            _addConfigObjectModal.Hide();
            _selectEnvClusterIds.Clear();
        }

        private async Task UpdateJsonConfigAsync(ConfigObjectModel configObject)
        {
            configObject.IsEditing = !configObject.IsEditing;
            if (!configObject.IsEditing)
            {
                if (string.IsNullOrWhiteSpace(configObject.Content))
                {
                    await PopupService.ToastErrorAsync("json内容不能为空");
                    return;
                }

                string indexStr = configObject.Content[..1];
                switch (indexStr)
                {
                    case "[":
                        try
                        {
                            configObject.Content = JArray.Parse(configObject.Content).ToString();
                        }
                        catch
                        {
                            await PopupService.ToastErrorAsync("json格式有误!");
                            return;
                        }

                        break;
                    case "{":
                        try
                        {
                            configObject.Content = JObject.Parse(configObject.Content).ToString();
                        }
                        catch
                        {
                            await PopupService.ToastErrorAsync("json格式有误!");
                            return;
                        }

                        break;
                    default:
                        await PopupService.ToastErrorAsync("json格式有误!");
                        return;
                }

                await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                {
                    ConfigObjectId = configObject.Id,
                    Content = configObject.Content,
                    FormatLabelCode = "Json"
                });

                await PopupService.ToastSuccessAsync("修改成功");
            }
        }

        private void ShowPropertyModal(int configObjectId, List<ConfigObjectPropertyModel>? models = null, ConfigObjectPropertyModel? model = null)
        {
            if (models != null)
            {
                //add
                _selectConfigObjectAllProperties = models;
                _propertyConfigModal.Show(configObjectId);
            }
            else if (model != null)
            {
                //edit
                var dto = new ConfigObjectPropertyContentDto
                {
                    Key = model.Key,
                    Value = model.Value,
                    Description = model.Description,
                    Modifier = model.Modifier,
                    ModificationTime = model.ModificationTime
                };
                _propertyConfigModal.Show(dto, configObjectId);
            }
        }

        private async Task UpdateConfigObjectPropertyContentAsync(ConfigObjectPropertyContentDto dto)
        {
            var content = JsonSerializer.Serialize(dto);
            await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
            {
                ConfigObjectId = _propertyConfigModal.Depend,
                Content = content,
                FormatLabelCode = "Properties"
            });
        }

        private async Task SubmitPropertyConfigAsync()
        {
            if (_propertyConfigModal.HasValue)
            {
                //edit
                //await UpdateConfigObjectPropertyContentAsync(_propertyConfigModal.Data);

                await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                {
                    ConfigObjectId = _propertyConfigModal.Depend,
                    FormatLabelCode = "Properties",
                    EditConfigObjectPropertyContent = new List<ConfigObjectPropertyContentDto> { _propertyConfigModal.Data }
                });
            }
            else
            {
                //add
                if (_selectConfigObjectAllProperties.Any(prop => prop.Key.ToLower() == _propertyConfigModal.Data.Key.ToLower()))
                {
                    await PopupService.ToastErrorAsync($"key：{_propertyConfigModal.Data.Key} 已存在");
                }
                else
                {
                    //await UpdateConfigObjectPropertyContentAsync(_propertyConfigModal.Data);

                    await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                    {
                        ConfigObjectId = _propertyConfigModal.Depend,
                        FormatLabelCode = "Properties",
                        AddConfigObjectPropertyContent = new List<ConfigObjectPropertyContentDto> { _propertyConfigModal.Data }
                    });
                }
            }

            await GetConfigObjectsAsync(_selectEnvClusterId, _configObjectType);
            _propertyConfigModal.Hide();
            await PopupService.ToastSuccessAsync("操作成功");
        }

        private async Task DeleteConfigObjectPropertyContentAsync(ConfigObjectPropertyModel model, int configObjectId)
        {
            await PopupService.ConfirmAsync("删除配置项", $"您确定要删除Key：{model.Key}，Value：{model.Value}的配置项吗？", async args =>
            {
                var content = JsonSerializer.Serialize(model);
                await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                {
                    ConfigObjectId = configObjectId,
                    Content = content,
                    FormatLabelCode = "Properties",
                    DeleteConfigObjectPropertyContent = new List<ConfigObjectPropertyContentDto>() { model }
                });

                await GetConfigObjectsAsync(_selectEnvClusterId, _configObjectType);
                await PopupService.ToastSuccessAsync("操作成功");
            });
        }

        private void HandleTabIndexChangedAsync(StringNumber index, List<ConfigObjectPropertyModel> configObjectProperties)
        {
            _tabIndex = index;
            if (index == 1)
            {
                _selectConfigObjectAllProperties = configObjectProperties;
                StringBuilder stringBuilder = new();
                foreach (var property in configObjectProperties)
                {
                    stringBuilder.AppendLine($"{property.Key} = {property.Value}");
                }
                _selectEditorContent.Content = stringBuilder.ToString();
                _selectEditorContent.FormatLabelCode = "Properties";
            }
        }

        private async Task TextConvertPropertyAsync(int configObjectId)
        {
            if (!string.IsNullOrWhiteSpace(_selectEditorContent.Content))
            {
                string[] lineDatas = _selectEditorContent.Content.Trim().Split("\n");
                var errorData = lineDatas.FirstOrDefault(l => !l.Contains('='));
                if (errorData != null)
                {
                    var errorLine = lineDatas.ToList().IndexOf(errorData) + 1;
                    await PopupService.ToastErrorAsync($"Line：{errorLine} key value must separate by '='");
                    return;
                }
                else
                {
                    Dictionary<string, ConfigObjectPropertyContentDto> propertyContents = new();
                    foreach (var lineData in lineDatas)
                    {
                        string[] keyValues = lineData.Trim().Split('=');
                        propertyContents.Add(keyValues[0], new ConfigObjectPropertyContentDto
                        {
                            Key = keyValues[0],
                            Value = keyValues[1]
                        });
                    }
                    List<ConfigObjectPropertyContentDto> editorPropertyContents = propertyContents.Values.ToList();


                    var added = editorPropertyContents.ExceptBy(
                            _selectConfigObjectAllProperties.Select(content => content.Key),
                            content => content.Key)
                        .ToList();

                    var deleted = _selectConfigObjectAllProperties.ExceptBy(
                            editorPropertyContents.Select(content => content.Key),
                            content => content.Key)
                        .ToList()
                        .Adapt<List<ConfigObjectPropertyContentDto>>();

                    var normalOrEdited = editorPropertyContents.ExceptBy(
                            added.Select(content => content.Key),
                            content => content.Key)
                        .ExceptBy(
                            deleted.Select(content => content.Key),
                            content => content.Key
                        ).ToList()
                        .Adapt<List<ConfigObjectPropertyContentDto>>();

                    var allPropertiesIntersect = _selectConfigObjectAllProperties.IntersectBy(
                        normalOrEdited.Select(content => content.Key), content => content.Key);

                    var edit = normalOrEdited.ExceptBy(
                            allPropertiesIntersect.Select(content => content.Value),
                            content => content.Value)
                        .ToList()
                        .Adapt<List<ConfigObjectPropertyContentDto>>();

                    await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                    {
                        ConfigObjectId = configObjectId,
                        FormatLabelCode = "Properties",
                        DeleteConfigObjectPropertyContent = deleted,
                        AddConfigObjectPropertyContent = added,
                        EditConfigObjectPropertyContent = edit
                    });

                    await GetConfigObjectsAsync(_selectEnvClusterId, _configObjectType);
                    await PopupService.ToastSuccessAsync("操作成功");
                    _tabIndex = 0;
                }
            }
        }
    }
}
