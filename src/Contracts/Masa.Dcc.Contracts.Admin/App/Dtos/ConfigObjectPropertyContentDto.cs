// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class ConfigObjectPropertyContentDto
    {
        [Required]
        public string Key { get; set; } = "";

        [Required]
        public string Value { get; set; } = "";

        public string Description { get; set; } = "";

        public string Modifier { get; set; } = "";

        public DateTime ModificationTime { get; set; }
    }
}
