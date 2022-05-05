// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class UpdatePublicConfigDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^[\u4E00-\u9FA5A-Za-z0-9_-.]+$", ErrorMessage = "Please enter [Chinese, English、and - _ . symbols] ")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = "";

        [StringLength(255)]
        public string Description { get; set; } = "";

        public UpdatePublicConfigDto()
        {
        }

        public UpdatePublicConfigDto(int Id, string name, string description = "")
        {
            this.Id = Id;
            Name = name;
            Description = description;
        }
    }
}
