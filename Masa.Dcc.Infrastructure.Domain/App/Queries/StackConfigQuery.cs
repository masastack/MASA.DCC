// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.App.Queries;

public record class StackConfigQuery(string Environment, string Cluster) : Query<Dictionary<string, string>>
{
    public override Dictionary<string, string> Result { get; set; }
}
