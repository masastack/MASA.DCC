// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class PublishReleaseDto
    {
        public string Content { get; set; } = "";

        public string FormatLabelCode { get; set; } = "";

        public ConfigObjectType ConfigObjectType { get; set; }

        public string RelationKey { get; set; } = "";
    }
}
