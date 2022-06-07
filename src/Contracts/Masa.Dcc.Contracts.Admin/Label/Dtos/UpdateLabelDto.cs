// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Label.Dtos
{
    public class UpdateLabelDto
    {
        [Required(ErrorMessage = "TypeCode is required")]
        public string TypeCode { get; set; } = "";

        [Required(ErrorMessage = "TypeName is required")]
        public string TypeName { get; set; } = "";

        [StringLength(255, MinimumLength = 0, ErrorMessage = "Description length range is [0-255]")]
        public string Description { get; set; } = "";

        [MinCount(1)]
        public List<LabelValueDto> LabelValues { get; set; } = new();
    }

    public class LabelValueDto
    {
        public string Name { get; set; } = "";

        public string Code { get; set; } = "";
    }
}
