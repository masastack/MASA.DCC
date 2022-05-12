// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class BizConfigDto : BaseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Identity { get; set; } = "";
    }
}
