// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components.Forms;

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
        private bool _showRollbackModal;
        private string _propertyConfigObjectName = "";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InitDataAsync();
            }
        }

        public async Task InitDataAsync()
        {
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
                    _projectDetail = await ProjectCaller.GetAsync(ProjectId); ;
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
                        IsEdited = true,
                        TempValue = tempContents.First(content => content.Key == editedContent.Key).Value
                    }).ToList();

                    config.ConfigObjectPropertyContents = published.Union(deleted).Union(added).Union(edited).ToList();
                }
            });

            StateHasChanged();
        }

        private async Task SearchConfigObjectAsync()
        {
            await GetConfigObjectsAsync(_selectCluster.Id, ConfigObjectType, _configObjectName);
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
    }
}
