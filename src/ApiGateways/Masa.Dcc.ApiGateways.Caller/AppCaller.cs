// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Caller
{
    public class AppCaller : DccHttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/app";

        public AppCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(AppCaller);
        }

        public async Task<AppDetailModel> GetWithEnvironmentClusterAsync(int Id)
        {
            var result = await CallerProvider.GetAsync<AppDetailModel>($"api/v1/appWithEnvCluster/{Id}");

            return result ?? new();
        }

        public async Task<List<AppDetailModel>> GetListAsync()
        {
            var result = await CallerProvider.GetAsync<List<AppDetailModel>>(_prefix);

            return result ?? new();
        }

        public async Task<List<AppDetailModel>> GetListByProjectIdsAsync(List<int> projectIds)
        {
            var result = await CallerProvider.PostAsync<List<int>, List<AppDetailModel>>("/api/v1/projects/app", projectIds);

            return result ?? new();
        }

        public async Task<List<AppPinDto>> GetAppPinListAsync(List<int> appIds)
        {
            var result = await CallerProvider.PostAsync<List<AppPinDto>>($"{_prefix}/pin", appIds);

            return result ?? new();
        }

        public async Task AddAppPinAsync(int appId)
        {
            await CallerProvider.PostAsync($"{_prefix}/pin/{appId}", null);
        }

        public async Task RemoveAppPinAsync(int appId)
        {
            await CallerProvider.DeleteAsync($"{_prefix}/pin/{appId}", null);
        }
    }
}
