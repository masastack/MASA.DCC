// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class AddConfigObjectDto
    {
        private string _name = "";

        [Required]
        [RegularExpression(@"^[\u4E00-\u9FA5A-Za-z0-9`~!$%^&*()_\-+=<>?:{}|,.\/;""·~！￥%……&*（）——\-+={}|《》？：“”【】、；]+$", ErrorMessage = "Special symbols are not allowed")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get => _name; set => _name = value.Trim(); }

        [Required]
        public string FormatLabelCode { get; set; } = "";

        [Required]
        [Range(1, int.MaxValue)]
        public ConfigObjectType Type { get; set; }

        /// <summary>
        /// appid or publicid or bizid
        /// </summary>
        [Required]
        public int ObjectId { get; set; }

        public int EnvironmentClusterId { get; set; }

        public string Content { get; set; } = default!;

        public string TempContent { get; set; } = default!;
    }
}
