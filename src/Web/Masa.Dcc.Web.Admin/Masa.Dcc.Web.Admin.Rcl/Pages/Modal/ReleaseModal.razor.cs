// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages.Modal
{
    public partial class ReleaseModal
    {
        [Parameter]
        public EventCallback OnSubmitAfter { get; set; }

        [Parameter]
        public ConfigObjectModel SelectConfigObject { get; set; } = new();

        [Parameter]
        public EnvironmentClusterModel SelectCluster { get; set; } = new();

        [Parameter]
        public List<ConfigObjectPropertyModel> SelectConfigObjectAllProperties { get; set; } = new();

        [Parameter]
        public bool Value
        {
            get
            {
                return _configObjectReleaseModal.Visible;
            }
            set
            {
                _configObjectReleaseModal.Visible = value;
            }
        }

        [Parameter]
        public EventCallback<bool> ValueChanged { get; set; }

        [Parameter]
        public AppDetailModel AppDetail { get; set; } = new();

        [Parameter]
        public string Content { get; set; } = "";

        [Inject]
        public ConfigObjectCaller ConfigObjectCaller { get; set; } = default!;

        [Inject]
        public IPopupService PopupService { get; set; } = default!;

        private readonly DataModal<AddConfigObjectReleaseDto> _configObjectReleaseModal = new();

        private List<DataTableHeader<ConfigObjectPropertyModel>> ReleaseHeaders
        {
            get
            {
                return new List<DataTableHeader<ConfigObjectPropertyModel>>()
                {
                    new() { Text = T("State"), Value = nameof(ConfigObjectPropertyModel.IsPublished) },
                    new() { Text = T("Key"), Value = nameof(ConfigObjectPropertyModel.Key) },
                    new() { Text = T("Published values"), Value = nameof(ConfigObjectPropertyModel.TempValue) },
                    new() { Text = T("Unpublished value"), Value = nameof(ConfigObjectPropertyModel.Value) },
                    new() { Text = T("Modifier"), Value = nameof(ConfigObjectPropertyModel.Modifier) },
                    new() { Text = T("ModificationTime"), Value = nameof(ConfigObjectPropertyModel.ModificationTime) }
                };
            }
        }

        private async Task ReleaseAsync(FormContext context)
        {
            _configObjectReleaseModal.Data.ConfigObjectId = SelectConfigObject.Id;
            _configObjectReleaseModal.Data.Type = ReleaseType.MainRelease;
            _configObjectReleaseModal.Data.EnvironmentName = SelectCluster.EnvironmentName;
            _configObjectReleaseModal.Data.ClusterName = SelectCluster.ClusterName;
            _configObjectReleaseModal.Data.Identity = AppDetail.Identity;
            _configObjectReleaseModal.Data.Content = Content;

            if (context.Validate())
            {
                await ConfigObjectCaller.ReleaseAsync(_configObjectReleaseModal.Data);

                if (OnSubmitAfter.HasDelegate)
                {
                    await OnSubmitAfter.InvokeAsync();
                }
                await PopupService.EnqueueSnackbarAsync(T("Publish succeeded"), AlertTypes.Success);
                _configObjectReleaseModal.Hide();
                await ValueChanged.InvokeAsync(false);
            }
        }

        private async Task SheetDialogValueChangedAsync(bool value)
        {
            Value = value;
            if (!value)
            {
                _configObjectReleaseModal.Hide();
            }

            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync();
            }
        }
    }
}
