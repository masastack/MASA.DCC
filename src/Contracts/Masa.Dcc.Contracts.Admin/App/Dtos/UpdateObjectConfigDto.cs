// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class UpdateObjectConfigDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public UpdateObjectConfigDto()
        {
        }

        public UpdateObjectConfigDto(int id, string name, string description = "")
        {
            this.Id = id;
            Name = name;
            Description = description;
        }
    }
}
