// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Model
{
    public class PublishReleaseModel
    {
        public string Content { get; set; } = "";

        public string FormatLabelCode { get; set; } = "";

        public ConfigObjectType ConfigObjectType { get; set; }

        public bool Encryption { get; set; }
    }
}
