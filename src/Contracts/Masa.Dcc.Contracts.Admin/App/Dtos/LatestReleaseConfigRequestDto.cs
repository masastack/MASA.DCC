// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos;

public class LatestReleaseConfigRequestDto<T>
{
    public List<T> Items { get; set; } = new();
    public int? EnvClusterId { get; set; } = null;
}
