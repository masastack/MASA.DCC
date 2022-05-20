// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Components
{
    public partial class Editor
    {
        private int _lineCount = 1;

        public ElementReference Ref { get; set; }

        [Inject]
        public IJSRuntime Js { get; set; } = default!;

        [Parameter]
        public string Value { get; set; } = default!;

        [Parameter]
        public EventCallback<string> ValueChanged { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public EventCallback<ChangeEventArgs> HandleOnInput { get; set; }

        protected override void OnParametersSet()
        {
            if (!string.IsNullOrEmpty(Value))
            {
                _lineCount = Regex.Matches(Value, "\n").Count + 1;
            }
        }

        public async void HandleOnChange(ChangeEventArgs args)
        {
            Value = args.Value?.ToString() ?? "";
            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync(Value);
            }
        }

        public async void OnInput(ChangeEventArgs args)
        {
            if (HandleOnInput.HasDelegate)
            {
                await HandleOnInput.InvokeAsync(args);
            }

            var value = args.Value?.ToString() ?? "";
            _lineCount = Regex.Matches(value, "\n").Count + 1;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Js.InvokeVoidAsync("PreventTab", Ref);
            }
        }
    }
}
