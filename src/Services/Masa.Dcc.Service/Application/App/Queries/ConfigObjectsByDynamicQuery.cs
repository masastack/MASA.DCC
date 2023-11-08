// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.App.Queries
{
    public record class ConfigObjectsByDynamicQuery(string environment, string cluster, string appId,
            List<string>? configObjects) : Query<Dictionary<string, PublishReleaseModel>>
    {
        public override Dictionary<string, PublishReleaseModel> Result { get; set; }
    }
}
