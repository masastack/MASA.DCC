namespace Masa.Dcc.Service.Admin.Domain.Aggregates
{
    [Table("PublicConfigs")]
    public class PublicConfig : BaseEntity<int, Guid>
    {
        [Required]
        public string Name { get; }

        [Required]
        public string Identity { get; }
        
        [Required]
        public string Description { get; }

        [Required]
        public int EnvironmentClusterId { get; }

        private readonly List<PublicConfigObject> publicConfigObjects = new();
        public IReadOnlyCollection<PublicConfigObject> PublicConfigObjects => publicConfigObjects;

        public PublicConfig(string name, string identity, string description, int environmentClusterId)
        {
            Name = name;
            Identity = identity;
            Description = description;
            EnvironmentClusterId = environmentClusterId;
        }
    }
}
