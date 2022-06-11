// Copyright (c) MASA Stack All rights reserved.
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
        public EventCallback<bool> ValueChanged { get; set; }

        [Inject]
        public ConfigObjectCaller ConfigObjectCaller { get; set; } = null!;

        [Inject]
        public IPopupService PopupService { get; set; } = null!;

        private List<StringNumber> _selectEnvClusterIds = new();
        private List<ConfigObjectDto> _publicConfigObjects = new();
        private int _selectPublicConfigObjectId;
        private ConfigObjectModel _selectConfigObject = new();
        private bool _isRelation = true;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Value)
            {
                await InitDataAsync();
            }
            else
            {
                _selectPublicConfigObjectId = 0;
                _selectConfigObject = new();
            }
        }

        private async Task InitDataAsync()
        {
            var publicConfig = await ConfigObjectCaller.GetPublicConfigAsync();
            _publicConfigObjects = await ConfigObjectCaller.GetConfigObjectsAsync(0, publicConfig.First().Id, ConfigObjectType.Public);
            _selectPublicConfigObjectId = _publicConfigObjects.FirstOrDefault()?.Id ?? 0;
            _selectConfigObject = _publicConfigObjects.First(p => p.Id == _selectPublicConfigObjectId).Adapt<ConfigObjectModel>();

            //handle property
        }

        private void SelectConfigObjectValueChanged(int configObjectId)
        {
            _selectPublicConfigObjectId = configObjectId;
            _selectConfigObject = _publicConfigObjects.First(p => p.Id == _selectPublicConfigObjectId).Adapt<ConfigObjectModel>();
        }

        private void PropertyValueChanged(string value, ConfigObjectPropertyModel model)
        {
            model.IsRelationed = Value.Equals(model.Value);
            model.Value = value;
        }

        private void ContentValueChanged(string content, ConfigObjectModel model)
        {
            var newContent = content.Replace("\n", "").Replace("\r", "").Replace("\t", "");
            var oldContent = model.Content.Replace("\n", "").Replace("\r", "").Replace("\t", "");

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
                string initialContent = (_selectConfigObject.FormatLabelCode.ToLower()) switch
                {
                    "json" => "{}",
                    "properties" => "[]",
                    _ => "",
                };

                List<AddConfigObjectDto> configObjectDtos = new();
                foreach (var envClusterId in _selectEnvClusterIds)
                {
                    configObjectDtos.Add(new AddConfigObjectDto
                    {
                        Name = _selectConfigObject.Name,
                        FormatLabelCode = _selectConfigObject.FormatLabelCode,
                        Content = initialContent,
                        TempContent = initialContent,
                        RelationConfigObjectId = _selectConfigObject.Id,
                        FromRelation = true,
                        EnvironmentClusterId = envClusterId.AsT1,
                        Type = _selectConfigObject.Type,
                        ObjectId = AppDetail.Id
                    });
                }
                await ConfigObjectCaller.AddConfigObjectAsync(configObjectDtos);

                await PopupService.ToastSuccessAsync("操作成功");
            }
        }
    }
}
