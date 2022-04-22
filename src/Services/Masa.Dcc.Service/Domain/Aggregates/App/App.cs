using Masa.BuildingBlocks.BasicAbility.Pm.Enum;

namespace Masa.Dcc.Service.Admin.Domain.Aggregates.App
{
    [NotMapped]
    public class App
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Identity { get; set; } = "";

        public AppTypes Type { get; set; }

        public ServiceTypes ServiceType { get; set; }

        public string Url { get; set; } = "";

        public string SwaggerUrl { get; set; } = "";

        public string Description { get; set; } = "";

        public DateTime CreationTime { get; set; }

        public Guid Creator { get; set; }

        public DateTime ModificationTime { get; set; }

        public Guid Modifier { get; set; }

        private readonly List<AppConfigObject> appConfigObjects = new();
        public IReadOnlyCollection<AppConfigObject> AppConfigObjects => appConfigObjects;
    }
}
