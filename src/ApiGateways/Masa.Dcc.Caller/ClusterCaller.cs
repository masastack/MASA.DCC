using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Dcc.Caller
{
    public class ClusterCaller : HttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/cluster";

        public ClusterCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(ClusterCaller);
        }

        protected override string BaseAddress { get; set; } = AppSettings.Get("ServiceBaseUrl");

        public async Task<List<ClusterModel>> GetListByEnvIdAsync(int envId)
        {
            var result = await CallerProvider.GetAsync<List<ClusterModel>>($"/api/v1/{envId}/cluster");

            return result ?? new();
        }

        public async Task<List<ClusterModel>> GetListAsync()
        {
            var result = await CallerProvider.GetAsync<List<ClusterModel>>($"/api/v1/cluster");

            return result ?? new();
        }

        public async Task<ClusterDetailModel> GetAsync(int Id)
        {
            var result = await CallerProvider.GetAsync<ClusterDetailModel>($"{_prefix}/{Id}");

            return result ?? new();
        }

        public async Task<List<EnvironmentClusterModel>> GetEnvironmentClusters()
        {
            var result = await CallerProvider.GetAsync<List<EnvironmentClusterModel>>($"/api/v1/envClusters");

            return result ?? new();
        }        
    }
}
