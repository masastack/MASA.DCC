// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Model
{
    public class ConfigObjectModel : ConfigObjectDto
    {
        public bool IsEditing { get; set; }

        public bool IsPublished => Content.Equals(TempContent);
    }
}
