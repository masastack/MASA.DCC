// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class RemoveConfigObjectDto
    {
        public int ConfigObjectId { get; set; }

        public string EnvironmentName { get; set; } = "";

        public string ClusterName { get; set; } = "";

        public string AppId { get; set; } = "";
    }
}
