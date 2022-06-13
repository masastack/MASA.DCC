// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Config
    {
        [Parameter]
        public int AppId { get; set; }

        [Parameter]
        public int EnvironmentClusterId { get; set; }

        [Parameter]
        public ConfigObjectType ConfigObjectType { get; set; } = default!;

        [Parameter]
        public string ProjectIdentity { get; set; } = default!;

        [Parameter]
        public int ProjectId { get; set; }

        [Parameter]
        public string Style { get; set; } = "height: calc(100vh - 180px);";

        [Parameter]
        public string ConfigPanelStyle { get; set; } = "height: calc(100vh - 292px);";

        [Inject]
        public ConfigObjectCaller ConfigObjectCaller { get; set; } = default!;

        [Inject]
        public IPopupService PopupService { get; set; } = default!;

        [Inject]
        public LabelCaller LabelCaller { get; set; } = default!;

        [Inject]
        public AppCaller AppCaller { get; set; } = default!;

        [Inject]
        public ProjectCaller ProjectCaller { get; set; } = default!;

        [Inject]
        public ClusterCaller ClusterCaller { get; set; } = default!;

        private AppDetailModel _appDetail = new();
        private List<EnvironmentClusterModel> _appEnvs = new();
        private List<EnvironmentClusterModel> _appClusters = new();
        private string _selectEnvName = "";
        private EnvironmentClusterModel _selectCluster = new();
        private List<ConfigObjectModel> _configObjects = new();
        private string _configObjectName = "";
        private readonly EditorContentModel _selectEditorContent = new();
        private List<ConfigObjectPropertyModel> _selectConfigObjectAllProperties = new();
        private StringNumber _tabIndex = 0;
        private readonly DataModal<ConfigObjectPropertyContentDto, int> _propertyConfigModal = new();
        private readonly List<DataTableHeader<ConfigObjectPropertyModel>> _headers = new()
        {
            new() { Text = "状态", Value = nameof(ConfigObjectPropertyModel.IsPublished) },
            new() { Text = "Key", Value = nameof(ConfigObjectPropertyModel.Key) },
            new() { Text = "Value", Value = nameof(ConfigObjectPropertyModel.Value) },
            new() { Text = "描述", Value = nameof(ConfigObjectPropertyModel.Description) },
            new() { Text = "修改人", Value = nameof(ConfigObjectPropertyModel.Modifier) },
            new() { Text = "修改时间", Value = nameof(ConfigObjectPropertyModel.ModificationTime) },
            new() { Text = "操作", Sortable = false, }
        };
        private readonly DataModal<AddConfigObjectDto> _addConfigObjectModal = new();
        private List<LabelDto> _configObjectFormats = new();
        private List<StringNumber> _selectEnvClusterIds = new();
        private ProjectDetailModel _projectDetail = new();
        private List<EnvironmentClusterModel> _allEnvClusters = new();
        private readonly DataModal<AddConfigObjectReleaseDto> _configObjectReleaseModal = new();
        private ConfigObjectModel _selectConfigObject = new();
        private readonly List<DataTableHeader<ConfigObjectPropertyModel>> _releaseHeaders = new()
        {
            new() { Text = "状态", Value = nameof(ConfigObjectPropertyModel.IsPublished) },
            new() { Text = "Key", Value = nameof(ConfigObjectPropertyModel.Key) },
            new() { Text = "发布的值", Value = nameof(ConfigObjectPropertyModel.TempValue) },
            new() { Text = "未发布的值", Value = nameof(ConfigObjectPropertyModel.Value) },
            new() { Text = "修改人", Value = nameof(ConfigObjectPropertyModel.Modifier) },
            new() { Text = "修改时间", Value = nameof(ConfigObjectPropertyModel.ModificationTime) }
        };
        private ConfigObjectWithReleaseHistoryDto _releaseHistory = new();
        private List<ConfigObjectReleaseModel> _configObjectReleases = new();
        private bool _showRollbackModal;
        private bool _showReleaseHistory;
        private ConfigObjectReleaseModel _selectReleaseHistory = new();
        private ConfigObjectReleaseModel _prevReleaseHistory = new();
        private List<ConfigObjectPropertyModel> _changedProperties = new();
        private int _releaseTabIndex;
        private readonly List<DataTableHeader<ConfigObjectPropertyModel>> _allConfigheaders = new()
        {
            new() { Text = "Key", Value = nameof(ConfigObjectPropertyModel.Key) },
            new() { Text = "Value", Value = nameof(ConfigObjectPropertyModel.Value) }
        };
        private readonly List<DataTableHeader<ConfigObjectPropertyModel>> _changedConfigheaders = new()
        {
            new() { Text = "状态", Value = nameof(ConfigObjectPropertyModel.IsPublished) },
            new() { Text = "Key", Value = nameof(ConfigObjectPropertyModel.Key) },
            new() { Text = "新的值", Value = nameof(ConfigObjectPropertyModel.Value) },
            new() { Text = "变更的值", Value = nameof(ConfigObjectPropertyModel.TempValue) }
        };
        private Action? _handleRollbackOnClickAfter = null;

        #region clone
        private bool _showCloneModal;
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

        private bool _cloneConfigObjectAllChecked;
        public bool CloneConfigObjectAllChecked
        {
            get
            {
                var checkedConfigObejctCount = _configObjects.Where(c => c.IsChecked).Count();
                return checkedConfigObejctCount == _configObjects.Count;
            }
            set
            {
                _cloneConfigObjectAllChecked = value;
            }
        }

        public bool IsNeedRebase
        {
            get => _configObjects.Where(c => c.IsNeedRebase).Any();
        }
        #endregion

        private Relation? _relation;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InitDataAsync();
            }
        }

        public async Task InitDataAsync()
        {
            if (ProjectId != 0)
                _projectDetail = await ProjectCaller.GetAsync(ProjectId);

            switch (ConfigObjectType)
            {
                case ConfigObjectType.Public:
                    _allEnvClusters = await ClusterCaller.GetEnvironmentClustersAsync();
                    //初始化公共配置
                    var publicConfigs = await ConfigObjectCaller.GetPublicConfigAsync();
                    var publicConfig = publicConfigs.FirstOrDefault();
                    if (publicConfig == null)
                    {
                        publicConfig = await ConfigObjectCaller.AddPublicConfigAsync(new AddObjectConfigDto
                        {
                            Name = "Public",
                            Identity = $"public-$Config",
                            Description = "Public config"
                        });
                    }
                    _appDetail = publicConfig.Adapt<AppDetailModel>();
                    _appDetail.EnvironmentClusters = _allEnvClusters;
                    EnvironmentClusterId = _allEnvClusters.First().Id;
                    break;
                case ConfigObjectType.Biz:
                    _appDetail = (await ConfigObjectCaller.GetBizConfigAsync($"{ProjectIdentity}-$biz")).Adapt<AppDetailModel>();
                    _allEnvClusters = await ClusterCaller.GetEnvironmentClustersAsync();
                    var projectEnvClusters = _allEnvClusters.Where(envCluster => _projectDetail.EnvironmentClusterIds.Contains(envCluster.Id)).ToList();
                    _appDetail.EnvironmentClusters = projectEnvClusters;
                    break;
                case ConfigObjectType.App:
                    _appDetail = await AppCaller.GetWithEnvironmentClusterAsync(AppId);
                    break;
                default:
                    throw new NotImplementedException();
            }

            _appEnvs = _appDetail.EnvironmentClusters.DistinctBy(env => env.EnvironmentName).ToList();
            var selectEnvCluster = _appDetail.EnvironmentClusters.First(envCluster => envCluster.Id == EnvironmentClusterId);
            _selectEnvName = selectEnvCluster.EnvironmentName;
            _selectCluster = selectEnvCluster;
            _appClusters = _appDetail.EnvironmentClusters.Where(envCluster => envCluster.EnvironmentName == _selectEnvName).ToList();
            await OnClusterChipClick(selectEnvCluster);

            StateHasChanged();
        }

        private async void OnEnvChipClick(string envName)
        {
            _selectEnvName = envName;
            _appClusters = _appDetail.EnvironmentClusters.Where(envCluster => envCluster.EnvironmentName == envName).ToList();

            await OnClusterChipClick(_appClusters.First());
        }

        private async Task OnClusterChipClick(EnvironmentClusterModel model)
        {
            _selectCluster = model;

            await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType);
        }

        private async Task GetConfigObjectsAsync(int envClusterId, ConfigObjectType configObjectType, string configObjectName = "")
        {
            var configObjects = await ConfigObjectCaller.GetConfigObjectsAsync(envClusterId, _appDetail.Id, configObjectType, configObjectName);
            _configObjects = configObjects.Adapt<List<ConfigObjectModel>>();
            var configObjectIds = _configObjects.Where(c => c.RelationConfigObjectId != 0).Select(c => c.RelationConfigObjectId).ToList();
            var publicConfigObjects = await ConfigObjectCaller.GetConfigObjectsByIdsAsync(configObjectIds);

            _configObjects.ForEach(config =>
            {
                if (config.RelationConfigObjectId != 0)
                {
                    publicConfigObjects.ForEach(publicConfig =>
                    {
                        if (config.RelationConfigObjectId == publicConfig.Id)
                        {
                            if (config.FormatLabelCode.Trim().ToLower() == "properties")
                            {
                                var publicContents = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(publicConfig.Content) ?? new();
                                var publicTempContents = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(publicConfig.TempContent) ?? new();
                                var appContents = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(config.Content) ?? new();
                                var appTempContents = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(config.TempContent) ?? new();

                                var appPublished = appContents
                                    .IntersectBy(appTempContents.Select(content => new { content.Key, content.Value }),
                                        content => new { content.Key, content.Value })
                                    .Select(publishedContent => new ConfigObjectPropertyModel
                                    {
                                        Key = publishedContent.Key,
                                        Value = publishedContent.Value,
                                        Description = publishedContent.Description,
                                        Modifier = publishedContent.Modifier,
                                        ModificationTime = publishedContent.ModificationTime,
                                        IsRelationed = false,
                                        IsPublished = true
                                    }).ToList();

                                appContents.RemoveAll(c => appPublished.Select(p => p.Key).Contains(c.Key));
                                var appNotPublished = appContents.Select(content => new ConfigObjectPropertyModel
                                {
                                    Key = content.Key,
                                    Value = content.Value,
                                    Description = content.Description,
                                    Modifier = content.Modifier,
                                    ModificationTime = content.ModificationTime,
                                    IsRelationed = false,
                                    IsPublished = false
                                }).ToList();

                                var relationConfigObjects = publicContents.ExceptBy(appContents.Select(c => c.Key), content => content.Key)
                                    .Select(content => new ConfigObjectPropertyModel
                                    {
                                        Key = content.Key,
                                        Value = content.Value,
                                        Description = content.Description,
                                        Modifier = content.Modifier,
                                        ModificationTime = content.ModificationTime,
                                        IsRelationed = true
                                    }).ToList();
                                var relationConfigObjectsPublished = relationConfigObjects
                                    .IntersectBy(publicTempContents.Select(c => new { c.Key, c.Value }), content => new { content.Key, content.Value })
                                    .Select(content => new ConfigObjectPropertyModel
                                    {
                                        Key = content.Key,
                                        Value = content.Value,
                                        Description = content.Description,
                                        Modifier = content.Modifier,
                                        ModificationTime = content.ModificationTime,
                                        IsRelationed = true,
                                        IsPublished = true
                                    }).ToList();
                                relationConfigObjects.RemoveAll(c => relationConfigObjectsPublished.Select(c => c.Key).Contains(c.Key));
                                var relationConfigObjectsNotPublished = relationConfigObjects.Select(content => new ConfigObjectPropertyModel
                                {
                                    Key = content.Key,
                                    Value = content.Value,
                                    Description = content.Description,
                                    Modifier = content.Modifier,
                                    ModificationTime = content.ModificationTime,
                                    IsRelationed = true,
                                    IsPublished = false
                                });

                                config.ConfigObjectPropertyContents = relationConfigObjectsPublished.Union(relationConfigObjectsNotPublished).Union(appPublished).Union(appNotPublished).ToList();
                            }
                            else
                            {
                                config.Content = publicConfig.Content;
                                config.TempContent = publicConfig.TempContent;
                            }
                        }
                    });
                }
                else
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
                            IsEdited = true,
                            TempValue = tempContents.First(content => content.Key == editedContent.Key).Value
                        }).ToList();

                        config.ConfigObjectPropertyContents = published.Union(deleted).Union(added).Union(edited).ToList();
                    }
                }
            });

            StateHasChanged();
        }

        private async Task SearchConfigObjectAsync(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType, _configObjectName);
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

                    await ConfigObjectCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                    {
                        ConfigObjectId = configObjectId,
                        FormatLabelCode = "Properties",
                        DeleteConfigObjectPropertyContent = deleted,
                        AddConfigObjectPropertyContent = added,
                        EditConfigObjectPropertyContent = edit
                    });

                    await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType);
                    await PopupService.ToastSuccessAsync("修改成功，若要生效请发布！");
                    _tabIndex = 0;
                }
            }
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

                await ConfigObjectCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                {
                    ConfigObjectId = configObject.Id,
                    Content = configObject.Content,
                    FormatLabelCode = "Json"
                });

                await PopupService.ToastSuccessAsync("修改成功，若要生效请发布！");
            }
        }

        private async Task UpdateOtherConfigObjectContentAsync(ConfigObjectModel configObject)
        {
            configObject.IsEditing = !configObject.IsEditing;
            if (!configObject.IsEditing)
            {
                if (string.IsNullOrWhiteSpace(configObject.Content))
                {
                    await PopupService.ToastErrorAsync("内容不能为空");
                    return;
                }
                else
                {
                    await ConfigObjectCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                    {
                        ConfigObjectId = configObject.Id,
                        Content = configObject.Content
                    });

                    await PopupService.ToastSuccessAsync("修改成功，若要生效请发布！");
                }
            }
        }

        private async Task DeleteConfigObjectPropertyContentAsync(ConfigObjectPropertyModel model, int configObjectId)
        {
            await PopupService.ConfirmAsync("删除配置项", $"您确定要删除Key：{model.Key}，Value：{model.Value}的配置项吗？", async args =>
            {
                var content = JsonSerializer.Serialize(model);
                await ConfigObjectCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                {
                    ConfigObjectId = configObjectId,
                    Content = content,
                    FormatLabelCode = "Properties",
                    DeleteConfigObjectPropertyContent = new List<ConfigObjectPropertyContentDto>() { model }
                });

                await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType);
                await PopupService.ToastSuccessAsync("修改成功，若要生效请发布！");
            });
        }

        private void ShowPropertyModal(ConfigObjectModel configObject, List<ConfigObjectPropertyModel>? models = null, ConfigObjectPropertyModel? model = null)
        {
            _selectConfigObject = configObject;
            if (models != null)
            {
                //add
                _selectConfigObjectAllProperties = models;
                _propertyConfigModal.Show(configObject.Id);
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
                _propertyConfigModal.Show(dto, configObject.Id);
            }
        }

        private void PropertyConfigModalValueChanged(bool value)
        {
            if (value == false)
            {
                _propertyConfigModal.Hide();
            }
        }

        private async Task ShowAddConfigObjectModal()
        {
            _addConfigObjectModal.Show();
            _configObjectFormats = await LabelCaller.GetLabelsByTypeCodeAsync("ConfigObjectFormat");
        }

        private async Task AddConfigObject(EditContext context)
        {
            _addConfigObjectModal.Data.Type = ConfigObjectType;
            _addConfigObjectModal.Data.ObjectId = _appDetail.Id;
            if (context.Validate())
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
                        Type = _addConfigObjectModal.Data.Type,
                        ObjectId = _addConfigObjectModal.Data.ObjectId,
                        EnvironmentClusterId = _selectEnvClusterIds[i].AsT1,
                        Content = initialContent,
                        TempContent = initialContent
                    });
                }

                await ConfigObjectCaller.AddConfigObjectAsync(configObjectDtos);
                var configObjects = await ConfigObjectCaller.GetConfigObjectsAsync(_selectCluster.Id, _appDetail.Id, _addConfigObjectModal.Data.Type, ""); ;
                _configObjects = configObjects.Adapt<List<ConfigObjectModel>>();

                _addConfigObjectModal.Hide();
                _selectEnvClusterIds.Clear();
            }
        }

        private async Task SubmitPropertyConfigAsync(EditContext context)
        {
            if (context.Validate())
            {
                if (_propertyConfigModal.HasValue)
                {
                    await ConfigObjectCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                    {
                        ConfigObjectId = _propertyConfigModal.Depend,
                        FormatLabelCode = "Properties",
                        EditConfigObjectPropertyContent = new List<ConfigObjectPropertyContentDto> { _propertyConfigModal.Data }
                    });
                }
                else
                {
                    if (_selectConfigObjectAllProperties.Any(prop => prop.Key.ToLower() == _propertyConfigModal.Data.Key.ToLower()))
                    {
                        await PopupService.ToastErrorAsync($"key：{_propertyConfigModal.Data.Key} 已存在");
                    }
                    else
                    {
                        await ConfigObjectCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                        {
                            ConfigObjectId = _propertyConfigModal.Depend,
                            FormatLabelCode = "Properties",
                            AddConfigObjectPropertyContent = new List<ConfigObjectPropertyContentDto> { _propertyConfigModal.Data }
                        });
                    }
                }

                await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType);
                _propertyConfigModal.Hide();
                await PopupService.ToastSuccessAsync("操作成功");
            }
        }

        private async Task RemoveAsync(ConfigObjectModel configObject)
        {
            await PopupService.ConfirmAsync("删除配置对象", $"删除配置对象“{configObject.Name}”将导致实例获取不到此配置对象的配置，确定要删除吗？", async args =>
            {
                await ConfigObjectCaller.RemoveAsync(configObject.Id);
                _configObjects.Remove(configObject);
            });
        }

        private void ShowReleaseModalAsync(ConfigObjectModel model)
        {
            _selectConfigObject = model;
            if (model.ConfigObjectPropertyContents.Any())
            {
                _selectConfigObjectAllProperties = model.ConfigObjectPropertyContents.Where(prop => prop.IsPublished == false).ToList();
            }
            _configObjectReleaseModal.Show();
        }

        private async Task ReleaseAsync(EditContext context)
        {
            _configObjectReleaseModal.Data.ConfigObjectId = _selectConfigObject.Id;
            _configObjectReleaseModal.Data.Type = ReleaseType.MainRelease;
            _configObjectReleaseModal.Data.EnvironmentName = _selectCluster.EnvironmentName;
            _configObjectReleaseModal.Data.ClusterName = _selectCluster.ClusterName;
            _configObjectReleaseModal.Data.Identity = _appDetail.Identity;

            if (context.Validate())
            {
                await ConfigObjectCaller.ReleaseAsync(_configObjectReleaseModal.Data);

                await InitDataAsync();
                await PopupService.ToastSuccessAsync("发布成功");
                _configObjectReleaseModal.Hide();
            }
        }

        private async Task RevokeAsync(ConfigObjectModel configObject)
        {
            await PopupService.ConfirmAsync("撤销配置", $"配置对象“{configObject.Name}”下已修改但尚未发布的配置将呗撤销，您确定要撤销吗？", async args =>
            {
                await ConfigObjectCaller.RevokeAsync(configObject.Id);

                await InitDataAsync();
                await PopupService.ToastSuccessAsync("撤销成功");
            });
        }

        private async Task ShowRollbackModalAsync(ConfigObjectModel configObject)
        {
            _releaseHistory = await ConfigObjectCaller.GetReleaseHistoryAsync(configObject.Id);
            if (configObject.FormatLabelCode.ToLower() == "properties")
            {
                var latestConfigObjectRelease = _releaseHistory.ConfigObjectReleases.OrderByDescending(release => release.Id).First();
                if (latestConfigObjectRelease.FromReleaseId == 0)
                {
                    _releaseHistory.ConfigObjectReleases = _releaseHistory.ConfigObjectReleases
                       .OrderByDescending(release => release.Id)
                       .Take(2)
                       .ToList();
                }
                else
                {
                    _releaseHistory.ConfigObjectReleases = _releaseHistory.ConfigObjectReleases
                        .OrderByDescending(release => release.Id)
                        .Where(release => release.Id <= latestConfigObjectRelease.ToReleaseId)
                        .Take(2)
                        .ToList();
                }
            }
            if (_releaseHistory.ConfigObjectReleases.Count <= 1)
            {
                await PopupService.ToastErrorAsync("没有可以回滚的发布历史");
                return;
            }

            _showRollbackModal = true;
        }

        private async Task RollbackAsync()
        {
            await ConfigObjectCaller.RollbackAsync(new RollbackConfigObjectReleaseDto
            {
                ConfigObjectId = _releaseHistory.Id,
                RollbackToReleaseId = _releaseHistory.ConfigObjectReleases.Last().Id
            });

            await InitDataAsync();
            _showRollbackModal = false;
            await PopupService.ToastSuccessAsync("回滚成功");
        }

        private async Task ShowReleaseHistoryAsync(ConfigObjectModel configObject)
        {
            _releaseHistory = await ConfigObjectCaller.GetReleaseHistoryAsync(configObject.Id);
            _configObjectReleases = _releaseHistory.ConfigObjectReleases
                .OrderByDescending(release => release.Id)
                .Adapt<List<ConfigObjectReleaseModel>>();

            if (configObject.FormatLabelCode.Trim().ToLower() == "properties")
            {
                _configObjectReleases.ForEach(release =>
                {
                    release.ConfigObjectProperties = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(release.Content) ?? new();
                });
            }

            if (_configObjectReleases.Count < 1)
            {
                await PopupService.ToastErrorAsync("暂无发布历史");
            }
            else
            {
                OnTimelineItemClick(_configObjectReleases.First(), false);
                _showReleaseHistory = true;
            }
        }

        private void OnTimelineItemClick(ConfigObjectReleaseModel configObjectRelease, bool enableTabIndexChanged = true)
        {
            _selectReleaseHistory = configObjectRelease;
            int index = _configObjectReleases.IndexOf(configObjectRelease);
            _prevReleaseHistory = _configObjectReleases.Skip(index + 1).FirstOrDefault() ?? new();
            configObjectRelease.IsActive = true;
            _configObjectReleases.ForEach(release =>
            {
                if (release.Id != configObjectRelease.Id)
                {
                    release.IsActive = false;
                }
            });

            if (enableTabIndexChanged)
            {
                ReleaseHistoryTabIndexChanged(_releaseTabIndex);
            }
        }

        private void ReleaseHistoryTabIndexChanged(StringNumber index)
        {
            _releaseTabIndex = index.AsT1;
            if (_releaseTabIndex == 1)
            {
                if (_prevReleaseHistory.ConfigObjectProperties.Count == 0)
                {
                    _changedProperties = _selectReleaseHistory.ConfigObjectProperties.Select(release => new ConfigObjectPropertyModel
                    {
                        Key = release.Key,
                        Value = "",
                        TempValue = release.Value,
                        IsAdded = true
                    }).ToList();
                }
                else
                {
                    var current = _selectReleaseHistory.ConfigObjectProperties;
                    var prev = _prevReleaseHistory.ConfigObjectProperties;

                    var added = current.ExceptBy(prev.Select(content => content.Key), content => content.Key)
                        .Select(content => new ConfigObjectPropertyModel
                        { IsAdded = true, Key = content.Key, TempValue = content.Value })
                        .ToList();
                    var deleted = prev.ExceptBy(current.Select(content => content.Key), content => content.Key)
                        .Select(content => new ConfigObjectPropertyModel
                        { IsDeleted = true, Key = content.Key, Value = content.Value })
                        .ToList();
                    var intersectAndEdited = current.IntersectBy(prev.Select(content => content.Key), content => content.Key)
                        .ToList();
                    var intersect = current.IntersectBy(
                        prev.Select(content => new { content.Key, content.Value }), content => new { content.Key, content.Value });
                    intersectAndEdited.RemoveAll(content => intersect.Select(content => content.Key).Contains(content.Key));
                    var edited = intersectAndEdited
                        .Select(content => new ConfigObjectPropertyModel
                        {
                            IsEdited = true,
                            Key = content.Key,
                            Value = current.FirstOrDefault(current => current.Key == content.Key)?.Value ?? "",
                            TempValue = prev.FirstOrDefault(rollback => rollback.Key == content.Key)?.Value ?? ""
                        }).ToList();

                    _changedProperties = added
                        .UnionBy(deleted, prop => prop.Key)
                        .UnionBy(edited, prop => prop.Key).ToList();
                }
            }
        }

        private async Task RollbackToAsync()
        {
            var current = _configObjectReleases.First();
            if (_selectReleaseHistory.IsInvalid)
            {
                await PopupService.ToastErrorAsync("此版本已作废，无法回滚");
                return;
            }
            if (current.ToReleaseId == _selectReleaseHistory.Id || _selectReleaseHistory.Id == current.Id || current.Version == _selectReleaseHistory.Version)
            {
                await PopupService.ToastErrorAsync("该版本与当前版本配置相同，无法回滚");
                return;
            }

            _releaseHistory.ConfigObjectReleases.Clear();
            _releaseHistory.ConfigObjectReleases.Add(current);
            _releaseHistory.ConfigObjectReleases.Add(_selectReleaseHistory);
            _showRollbackModal = true;
            _handleRollbackOnClickAfter = async () =>
            {
                _releaseHistory = await ConfigObjectCaller.GetReleaseHistoryAsync(current.ConfigObjectId);
                _configObjectReleases = _releaseHistory.ConfigObjectReleases.OrderByDescending(release => release.Id).Adapt<List<ConfigObjectReleaseModel>>();
                if (_releaseHistory.FormatLabelCode.Trim().ToLower() == "properties")
                {
                    _configObjectReleases.ForEach(release =>
                    {
                        release.ConfigObjectProperties = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(release.Content) ?? new();
                    });
                }
                StateHasChanged();
            };
        }

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
            _cloneApps = await AppCaller.GetListByProjectIdsAsync(new List<int> { projectId });
        }

        #region clone
        private async Task ShowCloneModalAsync(ConfigObjectModel? configObject = null)
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

            _allProjects = await GetProjectList();
            _showCloneModal = true;
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
                _configObjects.Clear();
                _configObjects.Add(_selectConfigObject);
            }

            _step = 2;

            if (_cloneAppSelect)
            {
                _cloneSelectApp = await AppCaller.GetWithEnvironmentClusterAsync(_cloneSelectAppId);
            }
            else if (_cloneEnvSelect)
            {
                _cloneSelectApp = _appDetail.Adapt<AppDetailModel>();
                _cloneSelectApp.EnvironmentClusters.RemoveAll(e => e.Id == _selectCluster.Id);
            }

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

            //this will be chang
            _afterSelectCloneConfigObjects.Clear();
            _selectEnvClusterIds.ForEach(envClusterId =>
            {
                var configObjects = _afterAllCloneConfigObjects.Where(c => c.EnvironmentClusterId == envClusterId);
                _afterSelectCloneConfigObjects.AddRange(configObjects);
            });

            if (!_afterSelectCloneConfigObjects.Any())
            {
                _configObjects.ForEach(configObject =>
                {
                    configObject.IsNeedRebase = false;
                });
            }
            else
            {
                var checkedConfigObject = _configObjects.Where(c => c.IsChecked);
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

        private void CloneConfigObjectAllCheckedChanged(bool value)
        {
            _cloneConfigObjectAllChecked = value;

            if (value)
            {
                _configObjects.ForEach(configObject =>
                {
                    configObject.IsChecked = value;
                });

                IntersectByConfigObjectName(_configObjects, _afterSelectCloneConfigObjects);
            }
            else
            {
                _configObjects.ForEach(configObject =>
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
                _configObjects.ForEach(configObject =>
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

        private void RemoveProperty(string key, ConfigObjectModel model)
        {
            model.ConfigObjectPropertyContents.RemoveAll(prop => prop.Key == key);
        }

        private async Task CloneAsync()
        {
            if (!_selectEnvClusterIds.Any())
            {
                await PopupService.ToastErrorAsync("请选择环境/集群");
                return;
            }

            var configObjects = _configObjects.Where(c => c.IsChecked);
            if (!configObjects.Any())
            {
                await PopupService.ToastErrorAsync("请选要克隆的配置");
                return;
            }

            var dto = new CloneConfigObjectDto
            {
                ToAppId = _cloneSelectApp.Id
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

                    dto.ConfigObjects.Add(new AddConfigObjectDto
                    {
                        Name = configObject.Name,
                        FormatLabelCode = configObject.FormatLabelCode,
                        Type = configObject.Type,
                        ObjectId = configObject.Id,
                        EnvironmentClusterId = envClusterId.AsT1,
                        Content = content,
                        TempContent = initialContent,
                    });
                }
            }
            await ConfigObjectCaller.CloneAsync(dto);

            await CloneDialogValueChangedAsync(false);
        }

        private async Task CloneDialogValueChangedAsync(bool value)
        {
            _showCloneModal = value;
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
                _configObjects.Clear();
                _selectConfigObject = new();
                await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType);
                _step = 1;
            }
        }
        #endregion

        private async Task ShowRelationModal()
        {
            if (_relation != null)
            {
                _relation.SheetDialogValueChanged(true);
                await _relation.InitDataAsync();
            }
        }
    }
}
