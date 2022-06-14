﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Relation
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

        private List<StringNumber> _selectEnvClusterIds = new();
        private List<ConfigObjectDto> _publicConfigObjects = new();
        private int _selectPublicConfigObjectId;
        private ConfigObjectModel _selectConfigObject = new();
        private bool _isRelation = true;
        private ConfigObjectModel _originalConfigObject = new();

        public void SheetDialogValueChanged(bool value)
        {
            Value = value;
            if (!value)
            {
                _selectEnvClusterIds = new();
                _selectPublicConfigObjectId = 0;
                _selectConfigObject = new();
            }
        }

        public async Task InitDataAsync()
        {
            var publicConfig = await ConfigObjectCaller.GetPublicConfigAsync();
            _publicConfigObjects = await ConfigObjectCaller.GetConfigObjectsAsync(0, publicConfig.First().Id, ConfigObjectType.Public);
            _selectPublicConfigObjectId = _publicConfigObjects.FirstOrDefault()?.Id ?? 0;
            SelectConfigObjectValueChanged(_selectPublicConfigObjectId);
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
            if (!_selectEnvClusterIds.Any())
            {
                await PopupService.ToastErrorAsync("请选择要关联的集群环境");
            }
            else if (_selectPublicConfigObjectId == 0)
            {
                await PopupService.ToastErrorAsync("请选择要关联的公共配置");
            }
            else
            {
                var formatLabelCode = _selectConfigObject.FormatLabelCode.ToLower();
                string initialContent = formatLabelCode switch
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

                    foreach (var envClusterId in _selectEnvClusterIds)
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
                    foreach (var envClusterId in _selectEnvClusterIds)
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
