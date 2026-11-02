// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    /// <summary>
    /// Reuses <see cref="ConfigObjectDto"/> and adds the owner info (environment / cluster / project / app).
    /// For public config objects the project and app fields are empty.
    /// </summary>
    public class ConfigObjectSearchItemDto : ConfigObjectDto
    {
        public string EnvironmentName { get; set; } = "";

        public string ClusterName { get; set; } = "";

        public int? ProjectId { get; set; }

        public string ProjectName { get; set; } = "";

        public string ProjectIdentity { get; set; } = "";

        public int? AppId { get; set; }

        public string AppName { get; set; } = "";

        public string AppIdentity { get; set; } = "";
    }
}
