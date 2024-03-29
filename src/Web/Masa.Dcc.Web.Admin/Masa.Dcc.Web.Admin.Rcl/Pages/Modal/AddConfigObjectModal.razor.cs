﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages.Modal
{
    public partial class AddConfigObjectModal
    {
        [Parameter]
        public AppDetailModel AppDetail { get; set; } = new();

        [Parameter]
        public ConfigObjectType ConfigObjectType { get; set; }

        [Parameter]
        public EventCallback OnSubmitAfter { get; set; }

        [Parameter]
        public bool Value
        {
            get
            {
                return _addConfigObjectModal.Visible;
            }
            set
            {
                _addConfigObjectModal.Visible = value;
            }
        }

        [Parameter]
        public EventCallback<bool> ValueChanged { get; set; }

        [Inject]
        public ConfigObjectCaller ConfigObjectCaller { get; set; } = null!;

        [Inject]
        public LabelCaller LabelCaller { get; set; } = null!;

        private readonly DataModal<AddConfigObjectDto> _addConfigObjectModal = new();
        private List<LabelDto> _configObjectFormats { get; set; } = new();
        private List<StringNumber> _selectEnvClusterIds = new();
        private List<ConfigObjectDto> _configObjects = new();
        private string _namePrefix = "$public.";

        public async Task InitDataAsync()
        {
            Value = true;
            _configObjectFormats = await LabelCaller.GetLabelsByTypeCodeAsync("ConfigObjectFormat");
            _selectEnvClusterIds = AppDetail.EnvironmentClusters.Select(e => (StringNumber)e.Id).ToList();
            foreach (var item in AppDetail.EnvironmentClusters)
            {
                var configObjects = await ConfigObjectCaller.GetConfigObjectsAsync(item.Id, AppDetail.Id, ConfigObjectType);
                _configObjects.AddRange(configObjects);
            }
        }

        private async Task AddConfigObject(FormContext context)
        {
            _addConfigObjectModal.Data.Type = ConfigObjectType;
            _addConfigObjectModal.Data.ObjectId = AppDetail.Id;
            if (context.Validate())
            {
                if (!_selectEnvClusterIds.Any())
                {
                    await PopupService.EnqueueSnackbarAsync(T("Please select environment/cluster"), AlertTypes.Error);
                    return;
                }

                string configName = _addConfigObjectModal.Data.Name;
                if (ConfigObjectType == ConfigObjectType.Public)
                {
                    configName = _namePrefix + _addConfigObjectModal.Data.Name;
                }

                var selectConfigObject = _configObjects.Where(c => _selectEnvClusterIds.Contains(c.EnvironmentClusterId));
                if (selectConfigObject.Any(c => c.Name.ToLower().Equals(configName.ToLower())))
                {
                    await PopupService.EnqueueSnackbarAsync(T("Config object already exists"), AlertTypes.Error);
                    return;
                }

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
                        Name = configName,
                        FormatLabelCode = _addConfigObjectModal.Data.FormatLabelCode,
                        Type = _addConfigObjectModal.Data.Type,
                        Encryption = _addConfigObjectModal.Data.Encryption,
                        ObjectId = _addConfigObjectModal.Data.ObjectId,
                        EnvironmentClusterId = _selectEnvClusterIds[i].AsT1,
                        Content = initialContent,
                        TempContent = initialContent
                    });
                }

                await ConfigObjectCaller.AddConfigObjectAsync(configObjectDtos);

                await SheetDialogValueChangedAsync(false);

                if (OnSubmitAfter.HasDelegate)
                {
                    await OnSubmitAfter.InvokeAsync();
                }
            }
        }

        private void EnvClusterValueChanged(List<StringNumber> envClusterIds)
        {
            _selectEnvClusterIds = envClusterIds;
        }

        private async Task SheetDialogValueChangedAsync(bool value)
        {
            Value = value;
            if (!value)
            {
                _configObjects.Clear();
                _addConfigObjectModal.Hide();
                _selectEnvClusterIds.Clear();
            }

            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync();
            }
        }
    }
}
