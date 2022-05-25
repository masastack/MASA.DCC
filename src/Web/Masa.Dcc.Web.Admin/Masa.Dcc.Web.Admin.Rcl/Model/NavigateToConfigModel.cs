// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Model
{
    public class NavigateToConfigModel
    {
        public int AppId { get; set; }

        public int ProjectId { get; set; }

        public int EnvClusterId { get; set; }

        public ConfigObjectType ConfigObjectType { get; set; }

        public NavigateToConfigModel(int appId, int projectId, int envClusterId, ConfigObjectType configObjectType)
        {
            AppId = appId;
            ProjectId = projectId;
            EnvClusterId = envClusterId;
            ConfigObjectType = configObjectType;
        }

        public NavigateToConfigModel()
        {
        }
    }
}
