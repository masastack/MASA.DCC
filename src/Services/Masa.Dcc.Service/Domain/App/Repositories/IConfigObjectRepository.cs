namespace Masa.Dcc.Service.Admin.Domain.App.Repositories
{
    public interface IConfigObjectRepository : IRepository<ConfigObject>
    {
        Task<ConfigObject> GetConfigObjectWhitReleaseHistoriesAsync(int Id);
    }
}
