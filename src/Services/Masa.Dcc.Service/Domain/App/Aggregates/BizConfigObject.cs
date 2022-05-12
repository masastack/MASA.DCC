// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("BizConfigObjects")]
    public class BizConfigObject : BaseEntity<int, Guid>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int BizConfigId { get; private set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ConfigObjectId { get; private set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EnvironmentClusterId { get; private set; }

        public ConfigObject ConfigObject { get; private set; } = null!;

        public BizConfig BizConfig { get; private set; } = null!;

        [NotMapped]
        public EnvironmentClusterModel? EnvironmentClusterModel { get; private set; }

        public BizConfigObject(int bizConfigId, int environmentClusterId)
        {
            BizConfigId = bizConfigId;
            EnvironmentClusterId = environmentClusterId;
        }
    }
}
