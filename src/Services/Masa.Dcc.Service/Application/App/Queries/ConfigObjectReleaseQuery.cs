namespace Masa.Dcc.Service.Admin.Application.App.Queries
{
    public record ConfigObjectReleaseQuery(int ConfigObejctId) : Query<ConfigObjectWithReleaseHistoryDto>
    {
        public override ConfigObjectWithReleaseHistoryDto Result { get; set; } = new();
    }
}
