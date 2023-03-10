// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System.Xml.Linq;

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
        public string Style { get; set; } = "height: calc(100vh - 192px);";

        [Parameter]
        public string ConfigPanelStyle { get; set; } = "height: calc(100vh - 244px);";

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

        [Inject]
        public IJSRuntime Js { get; set; } = default!;

        private AppDetailModel _appDetail = new();
        private List<EnvironmentClusterModel> _appEnvs = new();
        private List<EnvironmentClusterModel> _appClusters = new();
        private string _selectEnvName = "";
        private EnvironmentClusterModel _selectCluster = new();
        private List<ConfigObjectModel> _configObjects = new();
        private List<ConfigObjectModel> _backupConfigObjects = new();
        private List<StringNumber> _selectPanels = new();
        private string _configObjectName = "";
        private List<ConfigObjectPropertyModel> _selectConfigObjectAllProperties = new();
        private readonly DataModal<ConfigObjectPropertyContentDto, int> _propertyConfigModal = new();
        private ProjectDetailModel _projectDetail = new();
        private List<EnvironmentClusterModel> _allEnvClusters = new();
        private ConfigObjectModel _selectConfigObject = new();
        private ConfigObjectWithReleaseHistoryDto _releaseHistory = new();
        private List<ConfigObjectReleaseModel> _configObjectReleases = new();
        private bool _showRollbackModal;
        private bool _showReleaseModal;
        private string _configObjectReleaseContent = "";
        private string _releaseTabText = "All configuration";
        private string? _cultureName;
        private CloneModal? _cloneModal;
        private RelationModal? _relation;
        private ReleaseHistoryModal? _releaseHistoryModal;
        private AddConfigObjectModal? _addConfigObjectModal;
        private UserModel _userInfo = new();
        private string _tempContent = "";

        private List<DataTableHeader<ConfigObjectPropertyModel>> Headers
        {
            get
            {
                var headers = new List<DataTableHeader<ConfigObjectPropertyModel>>()
                {
                    new() { Text = T("State"), Value = nameof(ConfigObjectPropertyModel.IsPublished), Width = 110 },
                    new() { Text = T("Key"), Value = nameof(ConfigObjectPropertyModel.Key), Width = 120 },
                    new() { Text = T("Value"), Value = nameof(ConfigObjectPropertyModel.Value), Width = 120  },
                    new() { Text = T("Description"), Value = nameof(ConfigObjectPropertyModel.Description) },
                    new() { Text = T("Modifier"), Value = nameof(ConfigObjectPropertyModel.Modifier), Width = 98  },
                    new() { Text = T("ModificationTime"), Value = nameof(ConfigObjectPropertyModel.ModificationTime), Width = 114 },
                    new() { Text = T("Operation"), Value="Operation", Sortable = false, Width = 88 }
                };

                return headers;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _userInfo = await AuthClient.UserService.GetCurrentUserAsync() ?? new();
                await InitDataAsync();
            }
        }

        public override Task SetParametersAsync(ParameterView parameters)
        {
            bool tryGetI18n = parameters.TryGetValue("I18n", out I18n? i18n);
            if (tryGetI18n)
            {
                if (i18n?.Culture.Name != _cultureName)
                {
                    _cultureName = i18n?.Culture.Name;
                    _configObjects.ForEach(config =>
                    {
                        if (config.FormatLabelCode.ToLower() == "properties")
                        {
                            _releaseTabText = T("All configuration");
                            config.ElevationTabPropertyContent.TabText = T("Table");
                        }
                    });
                }
            }
            return base.SetParametersAsync(parameters);
        }

        public async Task InitDataAsync(ConfigComponentModel? configModel = null)
        {
            if (configModel != null)
            {
                ProjectId = configModel.ProjectId;
                ProjectIdentity = configModel.ProjectIdentity;
                AppId = configModel.AppId;
                EnvironmentClusterId = configModel.EnvironmentClusterId;
                ConfigObjectType = configModel.ConfigObjectType;
            }

            if (ProjectId != 0)
            {
                _projectDetail = await ProjectCaller.GetAsync(ProjectId);
            }

            switch (ConfigObjectType)
            {
                case ConfigObjectType.Public:
                    _allEnvClusters = await ClusterCaller.GetEnvironmentClustersAsync();
                    //初始化公共配置
                    var publicConfigs = await ConfigObjectCaller.GetPublicConfigAsync();
                    var publicConfig = publicConfigs.FirstOrDefault();
                    publicConfig ??= await ConfigObjectCaller.AddPublicConfigAsync(new AddObjectConfigDto
                    {
                        Name = "Public",
                        Identity = $"public-$Config",
                        Description = "Public config"
                    });
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
            _selectPanels.Clear();

            await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType);
        }

        private async Task GetConfigObjectsAsync(int envClusterId, ConfigObjectType configObjectType, string configObjectName = "")
        {
            var configObjects = await ConfigObjectCaller.GetConfigObjectsAsync(envClusterId, _appDetail.Id, configObjectType, configObjectName);
            _configObjects = configObjects.OrderByDescending(config => config.CreationTime).Adapt<List<ConfigObjectModel>>();
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
                                var appOriginalContents = JsonSerializer.Deserialize<List<ConfigObjectPropertyModel>>(config.Content) ?? new();
                                var appContents = appOriginalContents.Adapt<List<ConfigObjectPropertyModel>>();
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

                                var relationConfigObjects = publicContents.ExceptBy(appOriginalContents.Select(c => c.Key), content => content.Key)
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
                                relationConfigObjects.RemoveAll(c => relationConfigObjectsPublished.Select(rc => rc.Key).Contains(c.Key));
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

                                config.ConfigObjectPropertyContents = relationConfigObjectsPublished
                                .Union(relationConfigObjectsNotPublished)
                                .Union(appPublished)
                                .Union(appNotPublished)
                                .ToList();

                                ConvertPropertyAsync(config.ConfigObjectPropertyContents, config);
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

                        config.ConfigObjectPropertyContents = ResolvePropertyConfigObjects(contents, tempContents);

                        ConvertPropertyAsync(config.ConfigObjectPropertyContents, config);
                    }
                }
            });

            _backupConfigObjects = new List<ConfigObjectModel>(_configObjects.ToArray());
            StateHasChanged();
        }

        private List<ConfigObjectPropertyModel> ResolvePropertyConfigObjects(
            List<ConfigObjectPropertyModel> contents,
            List<ConfigObjectPropertyModel> tempContents)
        {
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
                    Modifier = addedContent.Modifier,
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

            return published.Union(deleted).Union(added).Union(edited).ToList();
        }

        private void ConvertPropertyAsync(List<ConfigObjectPropertyModel> configObjectProperties, ConfigObjectModel config)
        {
            StringBuilder stringBuilder = new();
            foreach (var property in configObjectProperties)
            {
                stringBuilder.AppendLine($"{property.Key} = {property.Value}");
            }

            config.ElevationTabPropertyContent.TabText = T("Table");
            config.ElevationTabPropertyContent.Content = stringBuilder.ToString();
            config.ElevationTabPropertyContent.FormatLabelCode = "Properties";
        }

        private void SearchConfigObject(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                if (!string.IsNullOrWhiteSpace(_configObjectName))
                    _configObjects = _backupConfigObjects
                    .Where(config => config.Name.ToLower().Contains(_configObjectName.ToLower()))
                    .ToList();
                else
                    _configObjects = _backupConfigObjects;
            }
        }

        private async Task TextConvertPropertyAsync(int configObjectId)
        {
            var configObject = _configObjects.FirstOrDefault(c => c.Id == configObjectId) ?? new();
            var selectEditorContent = configObject.ElevationTabPropertyContent;

            if (!string.IsNullOrWhiteSpace(selectEditorContent.Content))
            {
                string[] lineDatas = selectEditorContent.Content.Trim().Split("\n");
                var errorData = lineDatas.FirstOrDefault(l => !l.Contains('='));
                if (errorData != null)
                {
                    var errorLine = lineDatas.ToList().IndexOf(errorData) + 1;
                    await PopupService.EnqueueSnackbarAsync(T("Line:{errorLine}, key value must separate by '='").Replace("{errorLine}", errorLine + ""), AlertTypes.Error);
                }
                else
                {
                    List<ConfigObjectPropertyModel> editorPropertyContents = new();
                    foreach (var lineData in lineDatas)
                    {
                        string[] keyValues = lineData.Trim().Split('=');
                        var key = keyValues[0].TrimEnd();
                        if (editorPropertyContents.Any(property => property.Key == key))
                        {
                            await PopupService.EnqueueSnackbarAsync(T("Key: {key} already exists").Replace("{key}", key), AlertTypes.Error);
                            return;
                        }
                        else
                        {
                            editorPropertyContents.Add(new ConfigObjectPropertyModel
                            {
                                Key = keyValues[0].TrimEnd(),
                                Value = keyValues[1].TrimStart(),
                                Modifier = _userInfo.StaffDislpayName ?? _userInfo.DisplayName
                            });
                        }
                    }
                    await HandleTextConvertPropertyAsync(editorPropertyContents, configObjectId);
                }
            }
            else
            {
                await HandleTextConvertPropertyAsync(new List<ConfigObjectPropertyModel>(), configObjectId);
            }

            selectEditorContent.Disabled = false;
        }

        private async Task HandleTextConvertPropertyAsync(List<ConfigObjectPropertyModel> editorPropertyContents, int configObjectId)
        {
            DateTime now = DateTime.UtcNow;
            var config = new TypeAdapterConfig();
            config.NewConfig<ConfigObjectPropertyModel, ConfigObjectPropertyContentDto>()
                .Map(dest => dest.ModificationTime, src => now);

            var added = editorPropertyContents.ExceptBy(
                    _selectConfigObjectAllProperties.Select(content => content.Key),
                    content => content.Key)
                .BuildAdapter(config)
                .AdaptToType<List<ConfigObjectPropertyContentDto>>();

            var deleted = _selectConfigObjectAllProperties.ExceptBy(
                    editorPropertyContents.Select(content => content.Key),
                    content => content.Key)
                .BuildAdapter(config)
                .AdaptToType<List<ConfigObjectPropertyContentDto>>();

            var normalOrEdited = editorPropertyContents.ExceptBy(
                    added.Select(content => content.Key),
                    content => content.Key)
                .ExceptBy(
                    deleted.Select(content => content.Key),
                    content => content.Key
                );

            var allPropertiesIntersect = _selectConfigObjectAllProperties.IntersectBy(
                normalOrEdited.Select(content => content.Key), content => content.Key);

            var edit = normalOrEdited.ExceptBy(
                    allPropertiesIntersect.Select(content => content.Value),
                    content => content.Value)
                .BuildAdapter(config)
                .AdaptToType<List<ConfigObjectPropertyContentDto>>();

            await ConfigObjectCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
            {
                ConfigObjectId = configObjectId,
                FormatLabelCode = "Properties",
                DeleteConfigObjectPropertyContent = deleted,
                AddConfigObjectPropertyContent = added,
                EditConfigObjectPropertyContent = edit
            });

            await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType);
            await PopupService.EnqueueSnackbarAsync(T("ModificationSucceededPublish"), AlertTypes.Success);
            HandleTabIndexChanged(T("Table"), _configObjects.First(c => c.Id == configObjectId));
        }

        private void HandleTabIndexChanged(string tabText, ConfigObjectModel configObject)
        {
            configObject.ElevationTabPropertyContent.TabText = tabText;
            if (tabText == T("Text"))
            {
                _selectConfigObjectAllProperties = configObject.ConfigObjectPropertyContents;
                StringBuilder stringBuilder = new();
                foreach (var property in _selectConfigObjectAllProperties)
                {
                    if (!property.IsDeleted)
                    {
                        stringBuilder.AppendLine($"{property.Key} = {property.Value}");
                    }
                }
                configObject.ElevationTabPropertyContent.Content = stringBuilder.ToString();
                configObject.ElevationTabPropertyContent.FormatLabelCode = "Properties";
            }
            else
            {
                configObject.ElevationTabPropertyContent.Disabled = false;
            }
        }

        private void CancelEditConfigContent(ConfigObjectModel configObject)
        {
            if (configObject.FormatLabelCode.ToLower() == "properties")
            {
                StringBuilder stringBuilder = new();
                foreach (var property in configObject.ConfigObjectPropertyContents)
                {
                    if (!property.IsDeleted)
                    {
                        stringBuilder.AppendLine($"{property.Key} = {property.Value}");
                    }
                }
                configObject.ElevationTabPropertyContent.Content = stringBuilder.ToString();
                configObject.ElevationTabPropertyContent.Disabled = !configObject.ElevationTabPropertyContent.Disabled;
            }
            else
            {
                configObject.Content = _tempContent;
                configObject.IsEditing = !configObject.IsEditing;
            }
            _selectPanels.Clear();
        }

        private async Task UpdateJsonConfigAsync(ConfigObjectModel configObject)
        {
            if (string.IsNullOrWhiteSpace(configObject.Content))
            {
                configObject.Content = "{}";
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
                        await PopupService.EnqueueSnackbarAsync(T("Wrong format"), AlertTypes.Error);
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
                        await PopupService.EnqueueSnackbarAsync(T("Wrong format"), AlertTypes.Error);
                        return;
                    }

                    break;
                default:
                    await PopupService.EnqueueSnackbarAsync(T("Wrong format"), AlertTypes.Error);
                    return;
            }

            configObject.IsEditing = !configObject.IsEditing;
            if (!configObject.IsEditing)
            {
                await ConfigObjectCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                {
                    ConfigObjectId = configObject.Id,
                    Content = configObject.Content,
                    FormatLabelCode = "Json"
                });

                configObject.RelationConfigObjectId = 0;

                await PopupService.EnqueueSnackbarAsync(T("ModificationSucceededPublish"), AlertTypes.Success);
            }
            else
            {
                _tempContent = configObject.Content;
                var configObjects = _configObjects.Except(new List<ConfigObjectModel> { configObject });
                configObjects.ForEach(config =>
                {
                    config.IsEditing = false;
                });

                if (_selectPanels.All(id => id != configObject.Id))
                {
                    _selectPanels.Add(configObject.Id);
                }
            }
        }

        private async Task UpdateOtherConfigObjectContentAsync(ConfigObjectModel configObject)
        {
            if (configObject.IsEditing)
            {
                if (configObject.FormatLabelCode.ToLower() == "xml")
                {
                    try
                    {
                        XElement contacts = XElement.Parse(configObject.Content);
                        configObject.Content = contacts.ToString();
                    }
                    catch (Exception)
                    {
                        await PopupService.EnqueueSnackbarAsync(T("Wrong format"), AlertTypes.Error);
                        return;
                    }
                }

                configObject.IsEditing = !configObject.IsEditing;
                await ConfigObjectCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
                {
                    ConfigObjectId = configObject.Id,
                    Content = configObject.Content
                });

                await PopupService.EnqueueSnackbarAsync(T("ModificationSucceededPublish"), AlertTypes.Success);
            }
            else
            {
                _tempContent = configObject.Content;
                configObject.IsEditing = !configObject.IsEditing;
                var configObjects = _configObjects.Except(new List<ConfigObjectModel> { configObject });
                configObjects.ForEach(config =>
                {
                    config.IsEditing = false;
                });

                if (_selectPanels.All(id => id != configObject.Id))
                {
                    _selectPanels.Add(configObject.Id);
                }
            }
        }

        private async Task DeleteConfigObjectPropertyContentAsync(ConfigObjectPropertyModel model, int configObjectId)
        {
            var result = await PopupService.ConfirmAsync(T("Delete config object item"),
                T("Are you sure delete config object item \"Key:{key},Value:{value}\"吗?")
                .Replace("{key}", model.Key).Replace("{value}", model.Value),
                AlertTypes.Error);

            if (result)
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
                await PopupService.EnqueueSnackbarAsync(T("DeletionSucceededPublish"), AlertTypes.Success);
            }
        }

        private void ShowPropertyModal(ConfigObjectModel configObject, List<ConfigObjectPropertyModel>? models = null, ConfigObjectPropertyModel? model = null)
        {
            _selectConfigObject = configObject;
            _propertyConfigModal.Data.Modifier = _userInfo.StaffDislpayName ?? _userInfo.DisplayName;
            _propertyConfigModal.Data.ModificationTime = DateTime.UtcNow;
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
                    Modifier = _propertyConfigModal.Data.Modifier,
                    ModificationTime = _propertyConfigModal.Data.ModificationTime,
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

        private async Task ShowAddConfigObjectModalAsync()
        {
            if (_addConfigObjectModal != null)
            {
                await _addConfigObjectModal.InitDataAsync();
            }
        }

        private async Task SubmitPropertyConfigAsync(FormContext context)
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
                        await PopupService.EnqueueSnackbarAsync(T("Key: {key} already exists").Replace("{key}", _propertyConfigModal.Data.Key), AlertTypes.Error);
                        return;
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
                await PopupService.EnqueueSnackbarAsync(T("Add succeeded"), AlertTypes.Success);
            }
        }

        private async Task RemoveAsync(ConfigObjectModel configObject)
        {
            var result = await PopupService.ConfirmAsync(T("Delete config object"),
                 T("AreYouSureToDeleteConfigObject")
                 .Replace("{name}", configObject.Name),
                 AlertTypes.Error);

            if (result)
            {
                await ConfigObjectCaller.RemoveAsync(new RemoveConfigObjectDto
                {
                    ConfigObjectId = configObject.Id,
                    EnvironmentName = _selectCluster.EnvironmentName,
                    ClusterName = _selectCluster.ClusterName,
                    AppId = _appDetail.Identity
                });
                _configObjects.Remove(configObject);

                await PopupService.EnqueueSnackbarAsync(T("Delete succeeded"), AlertTypes.Success);
            }
        }

        private async Task ShowReleaseModalAsync(ConfigObjectModel model)
        {
            if (model.Content == model.TempContent)
            {
                await PopupService.EnqueueSnackbarAsync(T("Config object content has not changed"), AlertTypes.Error);
                return;
            }

            _selectConfigObject = model;
            if (model.ConfigObjectPropertyContents.Any())
            {
                _selectConfigObjectAllProperties = model.ConfigObjectPropertyContents.Where(prop => prop.IsPublished == false).ToList();
            }
            _showReleaseModal = true;

            if (model.FormatLabelCode.ToLower() == "properties" && model.RelationConfigObjectId != 0)
                _configObjectReleaseContent = JsonSerializer.Serialize(model.ConfigObjectPropertyContents.Adapt<List<ConfigObjectPropertyContentDto>>());
            else
                _configObjectReleaseContent = model.Content;
        }

        private async Task RevokeAsync(ConfigObjectModel configObject)
        {
            var result = await PopupService.ConfirmAsync(T("Revoke config"),
                 T("AreYouSureToRevoke")
                 .Replace("{name}", configObject.Name),
                 AlertTypes.Error);

            if (result)
            {
                await ConfigObjectCaller.RevokeAsync(configObject.Id);
                await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType);
                await PopupService.EnqueueSnackbarAsync(T("Revoke succeeded"), AlertTypes.Success);
            }
        }

        private async Task ShowRollbackModalAsync(ConfigObjectModel configObject)
        {
            _selectConfigObject = configObject;
            _releaseHistory = await ConfigObjectCaller.GetReleaseHistoryAsync(configObject.Id);
            _releaseHistory.ConfigObjectReleases = _releaseHistory.ConfigObjectReleases.OrderByDescending(release => release.Id).ToList();
            if (_releaseHistory.ConfigObjectReleases.Count <= 1
                || _releaseHistory.ConfigObjectReleases.First().Version == _releaseHistory.ConfigObjectReleases.Last().Version)
            {
                await PopupService.EnqueueSnackbarAsync(T("No publishing history can be rolled back"), AlertTypes.Error);
                return;
            }
            var latestConfigObjectRelease = _releaseHistory.ConfigObjectReleases.First();
            if (latestConfigObjectRelease.FromReleaseId == 0)
            {
                _releaseHistory.ConfigObjectReleases = _releaseHistory.ConfigObjectReleases.ToList();
            }
            else
            {
                var fromRollback = _releaseHistory.ConfigObjectReleases
                    .First(c => c.Version == latestConfigObjectRelease.Version && c.Id != latestConfigObjectRelease.Id);
                _releaseHistory.ConfigObjectReleases = _releaseHistory.ConfigObjectReleases
                    .Where(release => release.Id <= fromRollback.Id)
                    .ToList();
            }

            _releaseHistory.ConfigObjectReleases = _releaseHistory.ConfigObjectReleases.Take(2).ToList();
            _showRollbackModal = true;
        }

        private async Task RollbackAsync()
        {
            await ConfigObjectCaller.RollbackAsync(new RollbackConfigObjectReleaseDto
            {
                ConfigObjectId = _releaseHistory.Id,
                RollbackToReleaseId = _releaseHistory.ConfigObjectReleases.Last().Id
            });

            await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType);
            _showRollbackModal = false;
            await PopupService.EnqueueSnackbarAsync(T("Rollback succeeded"), AlertTypes.Success);
        }

        private async Task ShowCloneModalAsync(ConfigObjectModel? configObject = null)
        {
            if (_cloneModal != null)
            {
                await _cloneModal.ShowCloneModalAsync(configObject);
            }
        }

        private async Task ShowRelationModal()
        {
            if (_relation != null)
                await _relation.InitDataAsync();
        }

        private async Task ShowReleaseHistoryAsync(ConfigObjectModel configObject)
        {
            if (_releaseHistoryModal != null)
                await _releaseHistoryModal.InitDataAsync(configObject);
        }

        private async Task ConfigValueChanged(bool configValue)
        {
            if (configValue)
            {
                await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType);
                StateHasChanged();
            }
        }
    }
}
