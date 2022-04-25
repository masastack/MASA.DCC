namespace Masa.Dcc.Service.Admin.Domain.App.Aggregates
{
    [Table("PublicConfigs")]
    public class PublicConfig : BaseEntity<int, Guid>
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

        public void AddConfigObject(int configObjectId, int envClusterId)
        {
            _publicConfigObjects.Add(new PublicConfigObject(configObjectId, envClusterId));
        }
    }
}
