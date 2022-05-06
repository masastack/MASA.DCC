// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("AppConfigObjects")]
    public class AppConfigObject : BaseEntity<int, Guid>
    {
        [Comment("EnvironmentClusterId")]
        [Required]
        [Range(1, int.MaxValue)]
        public int EnvironmentClusterId { get; private set; }

        [Comment("AppId")]
        [Required]
        [Range(1, int.MaxValue)]
        public int AppId { get; private set; }

        [Comment("ConfigObjectId")]
        [Required]
        [Range(1, int.MaxValue)]
        public int ConfigObjectId { get; private set; }

        public ConfigObject? ConfigObject { get; set; }

        public AppConfigObject(int configObjectId, int appId, int environmentClusterId)
        {
            EnvironmentClusterId = environmentClusterId;
            AppId = appId;
            ConfigObjectId = configObjectId;
        }
    }
}
