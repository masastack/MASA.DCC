// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Label.Dtos;

public class UpdateLabelDto
{
    public string TypeCode { get; set; } = "";

    public string TypeName { get; set; } = "";

    public string Description { get; set; } = "";

    public List<LabelValueDto> LabelValues { get; set; } = new();
}

public class LabelValueDto
{
    public string Name { get; set; } = "";

    public string Code { get; set; } = "";
}
