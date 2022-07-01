// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Model
{
    public class AppModel : AppDetailModel
    {
        public bool IsPinned { get; set; }

        public DateTime PinTime { get; set; }
    }
}
