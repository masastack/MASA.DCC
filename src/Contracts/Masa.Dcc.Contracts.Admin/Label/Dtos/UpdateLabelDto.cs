// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.Label.Dtos
{
    public class UpdateLabelDto
    {
        [Required(ErrorMessage = "TypeCode is required")]
        [RegularExpression(@"^[\u4E00-\u9FA5A-Za-z0-9_.-]+$", ErrorMessage = "Please enter [Chinese、Number、 English、and - _ . symbols] ")]
        [StringLength(50, MinimumLength = 2)]
        public string TypeCode { get; set; } = "";

        [Required(ErrorMessage = "TypeName is required")]
        [RegularExpression(@"^[\u4E00-\u9FA5A-Za-z0-9_.-]+$", ErrorMessage = "Please enter [Chinese、Number、 English、and - _ . symbols] ")]
        [StringLength(50, MinimumLength = 2)]
        public string TypeName { get; set; } = "";

        [StringLength(255, MinimumLength = 0, ErrorMessage = "Description length range is [0-255]")]
        public string Description { get; set; } = "";

        public List<LabelValueDto> LabelValues { get; set; } = new();
    }

    public class LabelValueDto
    {
        [Required]
        [RegularExpression(@"^[\u4E00-\u9FA5A-Za-z0-9_.-]+$", ErrorMessage = "Special symbols are not allowed")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name length range is [2-50]")]
        public string Name { get; set; } = "";

        [Required]
        [RegularExpression(@"^[\u4E00-\u9FA5A-Za-z0-9_.-]+$", ErrorMessage = "Special symbols are not allowed")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Code length range is [2-50]")]
        public string Code { get; set; } = "";
    }
}
