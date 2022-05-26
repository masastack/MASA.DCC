// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Components
{
    public partial class RollbackModal
    {
        private bool _show;
        [Parameter]
        public bool Show
        {
            get => _show;
            set
            {
                _show = value;
                if (_show)
                {
                    _properties.Add(new ConfigObjectPropertyModel { Key = "1" });
                }
            }
        }

        [Parameter]
        public EventCallback OnClick { get; set; }

        [Parameter]
        [EditorRequired]
        public string FormatLabelCode { get; set; } = null!;

        [Parameter]
        [EditorRequired]
        public ConfigObjectReleaseDto CurrentConfigObjectRelease { get; set; } = null!;

        [Parameter]
        [EditorRequired]
        public ConfigObjectReleaseDto RollbackConfigObjectRelease { get; set; } = null!;

        private List<ConfigObjectPropertyModel> _properties = new();
        private readonly List<DataTableHeader<ConfigObjectPropertyModel>> _headers = new()
        {
            new (){ Text= "状态", Value= nameof(ConfigObjectPropertyModel.IsPublished)},
            new (){ Text= "Key", Value= nameof(ConfigObjectPropertyModel.Key)},
            new (){ Text= "发布的值", Value= nameof(ConfigObjectPropertyModel.TempValue)},
            new (){ Text= "未发布的值", Value= nameof(ConfigObjectPropertyModel.Value)},
            new (){ Text= "修改人", Value= nameof(ConfigObjectPropertyModel.Modifier) },
            new (){ Text= "修改时间", Value= nameof(ConfigObjectPropertyModel.ModificationTime) }
        };
    }
}
