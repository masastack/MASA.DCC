// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("PublicConfigs")]
    public class PublicConfig : FullAggregateRoot<int, Guid>
    {
        [Required]
        public string Name { get; private set; }

        [Required]
        public string Identity { get; private set; }

        [Required]
        public string Description { get; private set; }

        private readonly List<PublicConfigObject> _publicConfigObjects = new();
        public IReadOnlyCollection<PublicConfigObject> PublicConfigObjects => _publicConfigObjects;

        public PublicConfig(string name, string identity, string description = "")
        {
            Name = name;
            Identity = identity;
            Description = description;
        }

        public void Update(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
