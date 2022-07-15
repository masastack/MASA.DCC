// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("PublicConfigObjects")]
    public class PublicConfigObject : FullAggregateRoot<int, Guid>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int PublicConfigId { get; private set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ConfigObjectId { get; private set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int EnvironmentClusterId { get; private set; }

        public ConfigObject ConfigObject { get; private set; } = null!;

        public PublicConfig PublicConfig { get; private set; } = null!;

        [NotMapped]
        public EnvironmentClusterModel? EnvironmentClusterModel { get; private set; }

        public PublicConfigObject(int publicConfigId, int environmentClusterId)
        {
            PublicConfigId = publicConfigId;
            EnvironmentClusterId = environmentClusterId;
        }
    }
}
