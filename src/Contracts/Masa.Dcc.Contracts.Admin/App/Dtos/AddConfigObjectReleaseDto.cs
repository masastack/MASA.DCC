// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class AddConfigObjectReleaseDto
    {
        [Required]
        public ReleaseType Type { get; set; }

        [Required(ErrorMessage = "Config object Id is required")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "Config object Id is required")]
        public int ConfigObjectId { get; set; }

        [Required(ErrorMessage = "Name is required", AllowEmptyStrings = true)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name length range is [2-100]")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Comment is required", AllowEmptyStrings = true)]
        [StringLength(500, MinimumLength = 0, ErrorMessage = "Comment length range is [0-500]")]
        public string Comment { get; set; } = "";

        [Required(ErrorMessage = "Content is required", AllowEmptyStrings = true)]
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Content length range is [1-2147483647]")]
        public string Content { get; set; } = "";

        public string Environment { get; set; } = "";

        public string Cluster { get; set; } = "";

        public string AppIdentity { get; set; } = "";
    }
}
