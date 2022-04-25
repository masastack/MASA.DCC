namespace Masa.Dcc.Service.Admin.Application.App.Queries
{
    public record ConfigObjectsQuery(int EnvClusterId) : Query<List<ConfigObjectDto>>
    {
        public override List<ConfigObjectDto> Result { get; set; } = new();
    }
}
