
namespace Masa.Dcc.Service.Admin.Application.App.Queries
{
    public record PublicConfigsQuery : Query<List<PublicConfigDto>>
    {
        public override List<PublicConfigDto> Result { get; set; } = new();
    }
}
