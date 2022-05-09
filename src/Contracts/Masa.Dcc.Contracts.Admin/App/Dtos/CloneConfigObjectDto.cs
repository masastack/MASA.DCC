// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class CloneConfigObjectDto
    {
        public int ToAppId { get; set; }

        public List<AddConfigObjectDto> ConfigObjects { get; set; } = new();
    }
}
