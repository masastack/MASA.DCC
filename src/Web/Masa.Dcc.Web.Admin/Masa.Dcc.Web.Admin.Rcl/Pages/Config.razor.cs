// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Config
    {
        [Parameter]
        public Model.AppModel AppDetail { get; set; } = null!;

        [Parameter]
        public List<EnvironmentClusterModel> AppEnvClusters { get; set; } = null!;
    }
}
