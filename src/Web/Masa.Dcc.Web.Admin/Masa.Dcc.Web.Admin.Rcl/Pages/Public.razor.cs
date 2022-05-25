// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Public
    {
        [Inject]
        public ConfigObjecCaller ConfigObjecCaller { get; set; } = default!;

        private Config _config = null!;
        private ConfigComponentModel _configModel = new() { ConfigObjectType = ConfigObjectType.Public };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await _config.InitDataAsync();
            }
        }
    }
}
