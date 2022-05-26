// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Model
{
    public class RollbackComponentModel
    {
        public bool Show { get; set; }

        public string FormatLabelCode { get; set; } = null!;

        public ConfigObjectReleaseDto CurrentConfigObjectRelease { get; set; } = null!;

        public ConfigObjectReleaseDto RollbackConfigObjectRelease { get; set; } = null!;
    }
}
