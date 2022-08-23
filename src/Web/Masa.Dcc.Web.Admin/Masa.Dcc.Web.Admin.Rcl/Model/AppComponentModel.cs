// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Model
{
    public class AppComponentModel
    {
        public int ProjectId { get; set; }

        public AppComponentModel()
        {
        }

        public AppComponentModel(int projectId)
        {
            ProjectId = projectId;
        }
    }
}
