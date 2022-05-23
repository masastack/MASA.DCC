﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Model
{
    public class ConfigObjectModel : ConfigObjectDto
    {
        public bool IsEditing { get; set; }

        public bool IsPublished => Content.Equals(TempContent);

        /// <summary>
        /// handle this Content by yourself
        /// </summary>
        public List<ConfigObjectPropertyModel> ConfigObjectPropertyContents { get; set; } = new();
    }

    public class ConfigObjectPropertyModel : ConfigObjectPropertyContentDto
    {
        public bool IsDeleted { get; set; }

        public bool IsPublished { get; set; }

        public bool IsEdited { get; set; }

        public bool IsAdded { get; set; }

        public bool IsRelationed { get; set; }
    }
}