// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class PublishReleaseDto
    {
        public string Content { get; set; } = null!;

        public string FormatLabelName { get; set; } = null!;

        public ConfigObjectType ConfigObjectType { get; set; }
    }
}
