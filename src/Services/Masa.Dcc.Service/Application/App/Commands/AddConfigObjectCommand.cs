namespace Masa.Dcc.Service.Admin.Application.App.Commands
{
    public record AddConfigObjectCommand(AddConfigObjectDto ConfigObject) : Command
    {
        public int ConfigObjectId { get; set; }
    }
}
