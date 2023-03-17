// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Caller
{
    public class AppCaller : DccHttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/app";

        public AppCaller(
            IServiceProvider serviceProvider,
            DccApiGatewayOptions options) : base(serviceProvider, options)
        {
        }

        public async Task<AppDetailModel> GetWithEnvironmentClusterAsync(int Id)
        {
            var result = await Caller.GetAsync<AppDetailModel>($"api/v1/appWithEnvCluster/{Id}");

            return result ?? new();
        }

        public async Task<List<AppDetailModel>> GetListAsync()
        {
            var result = await Caller.GetAsync<List<AppDetailModel>>(_prefix);

            return result ?? new();
        }

        public async Task<List<AppDetailModel>> GetListByProjectIdsAsync(List<int> projectIds)
        {
            var result = await Caller.PostAsync<List<int>, List<AppDetailModel>>("/api/v1/projects/app", projectIds);

            return result ?? new();
        }

        public async Task<List<AppPinDto>> GetAppPinListAsync(List<int> appIds)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_prefix}/pin");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(appIds), Encoding.UTF8, "application/json");
            var result = await Caller.SendAsync<List<AppPinDto>>(httpRequest);

            return result ?? new();
        }

        public async Task AddAppPinAsync(int appId)
        {
            await Caller.PostAsync($"{_prefix}/pin/{appId}", null);
        }

        public async Task RemoveAppPinAsync(int appId)
        {
            await Caller.DeleteAsync($"{_prefix}/pin/{appId}", null);
        }
    }
}
