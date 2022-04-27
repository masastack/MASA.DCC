namespace Masa.Dcc.Service.Admin.Application.App.Commands
{
    public record AddConfigObjectCommand(AddConfigObjectDto ConfigObjectDto) : Command
    {
        public ConfigObjectDto Result { get; set; } = null!;
    }
}
