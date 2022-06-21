// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages.Modal
{
    public partial class RelationModal
    {
        [Parameter]
        public AppDetailModel AppDetail { get; set; } = new();

        [Parameter]
        public bool Value { get; set; }

        [Parameter]
        public EventCallback OnSubmitAfter { get; set; }

        [Inject]
        public ConfigObjectCaller ConfigObjectCaller { get; set; } = null!;

        [Inject]
        public IPopupService PopupService { get; set; } = null!;

        [Inject]
        public ClusterCaller ClusterCaller { get; set; } = null!;

        private List<StringNumber> _selectToEnvClusterIds = new();
        private List<ConfigObjectDto> _publicConfigObjects = new();
        private List<EnvironmentClusterModel> _allEnvClusters = new();
        private int _selectPublicConfigObjectId;
        private int _selectFromEnvClusterId;
        private ConfigObjectModel _selectConfigObject = new();
        private bool _isRelation = true;
        private ConfigObjectModel _originalConfigObject = new();
        private PublicConfigDto _publicConfig = new();

        public void SheetDialogValueChanged(bool value)
        {
            Value = value;
            if (!value)
            {
                _selectToEnvClusterIds = new();
                _selectPublicConfigObjectId = 0;
                _selectConfigObject = new();
            }
        }

        public async Task InitDataAsync()
        {
            _allEnvClusters = await ClusterCaller.GetEnvironmentClustersAsync();
            var publicConfig = await ConfigObjectCaller.GetPublicConfigAsync();
            if (!publicConfig.Any())
            {
                await PopupService.ToastErrorAsync("请先添加公共配置");
            }
            else
            {
                _publicConfig = publicConfig.First();
                Value = true;
            }
        }

        private async Task SelectEnvClusterValueChanged(int envClusterId)
        {
            _selectFromEnvClusterId = envClusterId;
            _selectPublicConfigObjectId = 0;
            _publicConfigObjects = await ConfigObjectCaller.GetConfigObjectsAsync(_selectFromEnvClusterId, _publicConfig.Id, ConfigObjectType.Public);
        }

        private void SelectConfigObjectValueChanged(int configObjectId)
        {
            _selectPublicConfigObjectId = configObjectId;
            _selectConfigObject = _publicConfigObjects.First(p => p.Id == _selectPublicConfigObjectId).Adapt<ConfigObjectModel>();

            if (_selectConfigObject.FormatLabelCode.ToLower() == "properties")
            {
                //handle property
                _selectConfigObject.ConfigObjectPropertyContents = JsonSerializer
                    .Deserialize<List<ConfigObjectPropertyModel>>(_selectConfigObject.Content) ?? new();
            }

            _originalConfigObject = _selectConfigObject.Adapt<ConfigObjectModel>();
        }

        private void PropertyValueChanged(string value, ConfigObjectPropertyModel model)
        {
            var originalValue = _originalConfigObject.ConfigObjectPropertyContents.First(p => p.Key == model.Key).Value;
            model.IsRelationed = value.Equals(originalValue);
            model.Value = value;
        }

        private void ContentValueChanged(string content, ConfigObjectModel model)
        {
            var newContent = content.Replace("\n", "").Replace("\r", "").Replace("\t", "");
            var oldContent = _originalConfigObject.Content.Replace("\n", "").Replace("\r", "").Replace("\t", "");

            _isRelation = newContent.Equals(oldContent);

            model.Content = content;
        }

        public async Task HandleOnSubmitAsync()
        {
            if (!_selectToEnvClusterIds.Any())
            {
                await PopupService.ToastErrorAsync("请选择要关联的集群环境");
            }
            else if (_selectPublicConfigObjectId == 0)
            {
                await PopupService.ToastErrorAsync("请选择要关联的公共配置");
            }
            else
            {
                foreach (var envClusterId in _selectToEnvClusterIds)
                {
                    var configObjects = await ConfigObjectCaller.GetConfigObjectsAsync(envClusterId.AsT1, AppDetail.Id, ConfigObjectType.App);
                    var relationed = configObjects.Select(c => c.Name).Contains(_selectConfigObject.Name);
                    if (relationed)
                    {
                        var envCluster = _allEnvClusters.First(c => c.Id == envClusterId.AsT1);
                        await PopupService.ToastErrorAsync($"该公共配置对象已在 {envCluster.EnvironmentClusterName} 中关联，请勿重复关联");
                        return;
                    }
                }

                var fromEnvCluster = _allEnvClusters.First(envCluster => envCluster.Id == _selectFromEnvClusterId);
                var formatLabelCode = _selectConfigObject.FormatLabelCode.ToLower();
                var initialContent = formatLabelCode switch
                {
                    "json" => "{}",
                    "properties" => "[]",
                    _ => "",
                };

                List<AddConfigObjectDto> configObjectDtos = new();

                if (formatLabelCode == "properties")
                {
                    var unRelationProperties = _selectConfigObject.ConfigObjectPropertyContents
                        .Where(p => p.IsRelationed == false)
                        .Adapt<List<ConfigObjectPropertyContentDto>>();
                    string unRelationContent;
                    if (unRelationProperties.Any())
                    {
                        unRelationContent = JsonSerializer.Serialize(unRelationProperties);
                    }
                    else
                    {
                        unRelationContent = initialContent;
                    }

                    foreach (var envClusterId in _selectToEnvClusterIds)
                    {
                        configObjectDtos.Add(new AddConfigObjectDto
                        {
                            Name = _selectConfigObject.Name,
                            FormatLabelCode = _selectConfigObject.FormatLabelCode,
                            Content = unRelationContent,
                            TempContent = initialContent,
                            RelationConfigObjectId = _selectConfigObject.Id,
                            FromRelation = true,
                            EnvironmentClusterId = envClusterId.AsT1,
                            Type = ConfigObjectType.App,
                            ObjectId = AppDetail.Id
                        });
                    }
                }
                else
                {
                    int relationConfigObjectId;
                    string content;
                    if (_isRelation)
                    {
                        relationConfigObjectId = _selectConfigObject.Id;
                        content = initialContent;
                    }
                    else
                    {
                        relationConfigObjectId = 0;
                        content = _selectConfigObject.Content;
                    }
                    foreach (var envClusterId in _selectToEnvClusterIds)
                    {
                        configObjectDtos.Add(new AddConfigObjectDto
                        {
                            Name = _selectConfigObject.Name,
                            FormatLabelCode = _selectConfigObject.FormatLabelCode,
                            Content = content,
                            TempContent = initialContent,
                            RelationConfigObjectId = relationConfigObjectId,
                            FromRelation = true,
                            EnvironmentClusterId = envClusterId.AsT1,
                            Type = ConfigObjectType.App,
                            ObjectId = AppDetail.Id
                        });
                    }
                }

                await ConfigObjectCaller.AddConfigObjectAsync(configObjectDtos);
                if (OnSubmitAfter.HasDelegate)
                {
                    await OnSubmitAfter.InvokeAsync();
                }
                await PopupService.ToastSuccessAsync("操作成功");
                SheetDialogValueChanged(false);
            }
        }
    }
}
