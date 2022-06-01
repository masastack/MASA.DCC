// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Model
{
    public class ConfigObjectReleaseModel : ConfigObjectReleaseDto
    {
        public bool IsActive { get; set; }

        public List<ConfigObjectPropertyModel> ConfigObjectProperties { get; set; } = new();
    }
}
