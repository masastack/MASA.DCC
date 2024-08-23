// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Application.App.Queries;

public record class PublicConfigQuery(string Environment, string Cluster, string ConfigObject) : Query<Dictionary<string, string>>
{
    public override Dictionary<string, string> Result { get; set; }
}
