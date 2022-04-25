namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class PublicConfigDto : BaseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Identity { get; set; } = "";

        public string Description { get; set; } = "";
    }
}
