namespace Masa.Dcc.Service.Admin.Application.App.Commands
{
    public record UpdateConfigObjectContentCommand(UpdateConfigObjectContentDto ConfigObjectContent) : Command
    {
        public ConfigObjectDto Result { get; set; } = null!;
    }
}
