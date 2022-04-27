namespace Masa.Dcc.Service.Admin.Application.App.Commands
{
    public record AddConfigObjectCommand(AddConfigObjectDto ConfigObjectDto) : Command
    {
        public ConfigObject ConfigObject { get; set; } = null!;
    }
}
