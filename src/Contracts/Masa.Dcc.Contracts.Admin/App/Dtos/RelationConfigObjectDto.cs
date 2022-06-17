// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class RelationConfigObjectDto : AddConfigObjectDto
    {
        [Required]
        public string EnvironmentName { get; set; } = "";

        [Required]
        public string ClusterName { get; set; } = "";

        [Required]
        public string Identity { get; set; } = "";
    }
}
