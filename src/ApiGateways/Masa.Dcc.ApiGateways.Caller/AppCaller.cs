// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Caller
{
    public class AppCaller : DccHttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/app";

        public AppCaller(DccApiGatewayOptions options) : base(options)
        {
        }

        public async Task<AppDetailModel> GetWithEnvironmentClusterAsync(int id)
        {
            var result = await Caller.GetAsync<AppDetailModel>($"api/v1/appWithEnvCluster/{id}");

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


        public async Task<List<LatestReleaseConfigModel>> GetLatestReleaseConfigAsync(LatestReleaseConfigRequestDto<int> request)
        {
            var result = await Caller.PostAsync<List<LatestReleaseConfigModel>>($"{_prefix}/latestReleaseConfig", request);
            return result ?? new();
        }

        public async Task<List<AppPinDto>> GetAppPinListAsync(List<int> appIds)
        {
            var param = new StringBuilder();
            foreach (var appId in appIds)
            {
                param.Append($"&appIds={appId}");
            }
            if (param.Length > 0)
                param.Remove(0, 1);

            var result = await Caller.GetAsync<List<AppPinDto>>($"{_prefix}/pin?{param.ToString()}");
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
