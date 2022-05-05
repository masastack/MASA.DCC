// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class ConfigObjectReleaseDto : BaseDto
    {
        public int Id { get; set; }

        public ConfigObjectType Type { get; set; }

        public string Version { get; set; } = "";

        public int ConfigObjectId { get; set; }

        public string Name { get; set; } = "";

        public string Comment { get; set; } = "";

        public string Content { get; set; } = "";

        public int FromReleaseId { get; set; }

        public int ToReleaseId { get; set; }

        public bool IsInvalid { get; set; }
    }
}
