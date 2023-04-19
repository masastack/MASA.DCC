// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.Label.Aggregates
{
    [Table("Labels")]
    [Index(nameof(TypeCode), nameof(IsDeleted), Name = "IX_TypeCode")]
    public class Label : FullAggregateRoot<int, Guid>
    {
        [Comment("Code")]
        [Required(ErrorMessage = "Label code is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Label code length range is [2-100]")]
        public string Code { get; set; }

        [Comment("Name")]
        [Required(ErrorMessage = "Label name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Label name length range is [2-100]")]
        public string Name { get; private set; }

        [Comment("TypeCode")]
        [Required(ErrorMessage = "TypeCode is required")]
        [StringLength(255, MinimumLength = 0, ErrorMessage = "TypeCode length range is [0-255]")]
        public string TypeCode { get; private set; }

        [Comment("TypeName")]
        [Required(ErrorMessage = "TypeName is required")]
        [StringLength(255, MinimumLength = 0, ErrorMessage = "TypeName length range is [0-255]")]
        public string TypeName { get; private set; }

        [Comment("Description")]
        [Required(ErrorMessage = "Description is required")]
        [StringLength(255, MinimumLength = 0, ErrorMessage = "Description length range is [0-255]")]
        public string Description { get; private set; }

        public Label(string code, string name, string typeCode, string typeName, string description = "")
        {
            Code = code;
            Name = name;
            TypeCode = typeCode;
            TypeName = typeName;
            Description = description;
        }

        public Label(string code, string name, string typeCode, string typeName, Guid creator, DateTime creationTime, string description = "")
        {
            Code = code;
            Name = name;
            TypeCode = typeCode;
            TypeName = typeName;
            Creator = creator;
            CreationTime = creationTime;
            Description = description;
        }

        public void SetUserId(Guid userId)
        {
            Creator = userId;
            Modifier = userId;
        }
    }
}
