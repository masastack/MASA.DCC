﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class UpdateConfigObjectContentDto
    {
        public int ConfigObjectId { get; set; }

        public string Content { get; set; } = "";

        public string FormatLabelCode { get; set; } = "";

        public List<ConfigObjectPropertyContentDto> AddConfigObjectPropertyContent { get; set; } = new();

        public List<ConfigObjectPropertyContentDto> EditConfigObjectPropertyContent { get; set; } = new();

        public List<ConfigObjectPropertyContentDto> DeleteConfigObjectPropertyContent { get; set; } = new();
    }
}
