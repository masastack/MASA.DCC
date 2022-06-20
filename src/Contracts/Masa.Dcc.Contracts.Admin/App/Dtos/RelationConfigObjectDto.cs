// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class RelationConfigObjectDto : AddConfigObjectDto
    {
        public string RelationEnvironmentName { get; set; } = "";

        public string RelationClusterName { get; set; } = "";

        public string RelationIdentity { get; set; } = "";

        public string RelationConfigObjectName { get; set; } = "";

        public string EnvironmentName { get; set; } = "";

        public string ClusterName { get; set; } = "";

        public string Identity { get; set; } = "";
    }
}
