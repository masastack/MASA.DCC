// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos;

public class AddObjectConfigDto
{
    public string Name { get; set; } = "";

    public string Identity { get; set; } = "";

    public string Description { get; set; } = "";

    public AddObjectConfigDto()
    {
    }

    public AddObjectConfigDto(string name, string identity, string description = "")
    {
        Name = name;
        Identity = identity;
        Description = description;
    }
}
