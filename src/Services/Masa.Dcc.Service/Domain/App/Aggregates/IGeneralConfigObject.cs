// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates;

public abstract class GeneralConfigObject : FullAggregateRoot<int, Guid>
{
    [Comment("ConfigObjectId")]
    [Required]
    [Range(1, int.MaxValue)]
    public int ConfigObjectId { get; protected set; }

    public ConfigObject ConfigObject { get; set; } = null!;

    [Comment("EnvironmentClusterId")]
    [Required]
    [Range(1, int.MaxValue)]
    public int EnvironmentClusterId { get; protected set; }

    [NotMapped]
    public EnvironmentClusterModel? EnvironmentClusterModel { get; protected set; }

}
