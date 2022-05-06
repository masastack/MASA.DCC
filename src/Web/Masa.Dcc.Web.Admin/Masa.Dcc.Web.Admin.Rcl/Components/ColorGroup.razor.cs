// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Components
{
    public partial class ColorGroup
    {
        [EditorRequired]
        [Parameter]
        public List<string> Colors { get; set; } = new();

        [Parameter]
        public string Value { get; set; } = string.Empty;

        [Parameter]
        public EventCallback<string> ValueChanged { get; set; }

        [Parameter]
        public string? Class { get; set; }

        [Parameter]
        public string? Style { get; set; }

        [Parameter]
        public int Size { get; set; } = 24;

        [Parameter]
        public bool SpaceBetween { get; set; } = false;

        [Parameter]
        public RenderFragment<string>? ItemAppendContent { get; set; }
    }
}
