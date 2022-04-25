namespace Masa.Dcc.Service.Admin.Domain.App.Repositories
{
    public interface IPublicConfigObjectRepository : IRepository<PublicConfigObject>
    {
        Task<List<PublicConfigObject>> GetListByEnvClusterIdAsync(int envClusterId);
    }
}