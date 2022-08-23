// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Model
{
    public class ConfigComponentModel
    {
        public int AppId { get; set; }

        public int EnvironmentClusterId { get; set; }

        public ConfigObjectType ConfigObjectType { get; set; }

        public string ProjectIdentity { get; set; } = "";

        public int ProjectId { get; set; }

        public ConfigComponentModel()
        {
        }

        public ConfigComponentModel(int appId, int environmentClusterId, ConfigObjectType configObjectType, int projectId)
        {
            AppId = appId;
            EnvironmentClusterId = environmentClusterId;
            ConfigObjectType = configObjectType;
            ProjectId = projectId;
        }

        public ConfigComponentModel(int appId, int environmentClusterId, ConfigObjectType configObjectType, string projectIdentity, int projectId)
        {
            AppId = appId;
            EnvironmentClusterId = environmentClusterId;
            ConfigObjectType = configObjectType;
            ProjectIdentity = projectIdentity;
            ProjectId = projectId;
        }
    }
}
