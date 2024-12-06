// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Shared;

[Table("BizConfigs")]
public class BizConfig : FullAggregateRoot<int, Guid>
{
    [Required]
    public string Name { get; private set; }

    [Required]
    public string Identity { get; private set; }

    private readonly List<BizConfigObject> _bizConfigObjects = new();
    public IReadOnlyCollection<BizConfigObject> BizConfigObjects => _bizConfigObjects;

    public BizConfig(string name, string identity)
    {
        Name = name;
        Identity = identity;
    }

    public void Update(string name)
    {
        Name = name;
    }
}
