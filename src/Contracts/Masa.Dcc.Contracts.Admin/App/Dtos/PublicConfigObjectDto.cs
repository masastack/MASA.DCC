namespace Masa.Dcc.Contracts.Admin.App.Dtos
{
    public class PublicConfigObjectDto
    {
        public int Id { get; set; }

        public int LabelId { get; set; }

        public PublicConfigDto PublicConfigObject { get; set; } = new();

        public ConfigObjectDto ConfigObject { get; set; } = new();

        public ConfigObjectMainDto ConfigObjectMain { get; set; } = new();
    }
}
