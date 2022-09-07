﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class ConfigObjectDto : BaseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public int EnvironmentClusterId { get; set; }

        public string FormatLabelCode { get; set; } = "";

        public string FormatName { get; set; } = "";

        public ConfigObjectType Type { get; set; }

        public int RelationConfigObjectId { get; set; }

        public bool FromRelation { get; set; }

        public string Content { get; set; } = "";

        public string TempContent { get; set; } = "";

        public bool Encryption { get; set; }
    }
}
