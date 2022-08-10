// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Caller
{
    public class ClusterCaller : DccHttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/cluster";

        public ClusterCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(ClusterCaller);
        }

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

        public async Task<List<EnvironmentClusterModel>> GetEnvironmentClustersAsync()
        {
            var result = await CallerProvider.GetAsync<List<EnvironmentClusterModel>>("/api/v1/envClusters");

            return result ?? new();
        }
    }
}
