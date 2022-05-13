// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Label.Dtos
{
    public class LabelDto : BaseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string TypeCode { get; set; } = null!;

        public string TypeName { get; set; } = null!;

        public string Description { get; set; } = null!;
    }
}
