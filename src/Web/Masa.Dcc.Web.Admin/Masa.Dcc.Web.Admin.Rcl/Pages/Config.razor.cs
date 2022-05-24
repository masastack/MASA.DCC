﻿// Copyright (c) MASA Stack All rights reserved.
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

        [Inject]
        public ConfigObjecCaller ConfigObjecCaller { get; set; } = default!;

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
            new (){ Text= "状态", Value= nameof(ConfigObjectPropertyModel.IsPublished)},
            new (){ Text= "Key", Value= nameof(ConfigObjectPropertyModel.Key)},
            new (){ Text= "Value", Value= nameof(ConfigObjectPropertyModel.Value)},
            new (){ Text= "描述", Value= nameof(ConfigObjectPropertyModel.Description)},
            new (){ Text= "修改人", Value= nameof(ConfigObjectPropertyModel.Modifier) },
            new (){ Text= "修改时间", Value= nameof(ConfigObjectPropertyModel.ModificationTime) },
            new (){ Text= "操作"}
        };
        private readonly DataModal<AddConfigObjectDto> _addConfigObjectModal = new();
        private List<LabelDto> _configObjectFormats = new();
        private List<StringNumber> _selectEnvClusterIds = new();
        private ProjectDetailModel _projectDetail = new();
        private List<EnvironmentClusterModel> _allEnvClusters = new();

        public async Task InitDataAsync()
        {
            switch (ConfigObjectType)
            {
                case ConfigObjectType.Public:
                    _allEnvClusters = await ClusterCaller.GetEnvironmentClustersAsync();
                    //初始化公共配置
                    var publicConfigs = await ConfigObjecCaller.GetPublicConfigAsync();
                    var publicConfig = publicConfigs.FirstOrDefault();
                    if (publicConfig == null)
                    {
                        publicConfig = await ConfigObjecCaller.AddPublicConfigAsync(new AddObjectConfigDto
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
                    _appDetail = (await ConfigObjecCaller.GetBizConfigAsync($"{ProjectIdentity}-$biz")).Adapt<AppDetailModel>();
                    _projectDetail =await ProjectCaller.GetAsync(ProjectId); ;
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
            var configObjects = await ConfigObjecCaller.GetConfigObjectsAsync(envClusterId, _appDetail.Id, configObjectType, configObjectName);
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

                    await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
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

                await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
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
                    await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
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
                await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
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
                    Type = ConfigObjectType,
                    ObjectId = _appDetail.Id,
                    EnvironmentClusterId = _selectEnvClusterIds[i].AsT1,
                    Content = initialContent,
                    TempContent = initialContent
                });
            }

            await ConfigObjecCaller.AddConfigObjectAsync(configObjectDtos);
            var configObjects = await ConfigObjecCaller.GetConfigObjectsAsync(_selectCluster.Id, _appDetail.Id, ConfigObjectType, ""); ;
            _configObjects = configObjects.Adapt<List<ConfigObjectModel>>();

            _addConfigObjectModal.Hide();
            _selectEnvClusterIds.Clear();
        }

        private async Task SubmitPropertyConfigAsync()
        {
            if (_propertyConfigModal.HasValue)
            {
                await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
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
                    await ConfigObjecCaller.UpdateConfigObjectContentAsync(new UpdateConfigObjectContentDto
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
}
