// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Caller
{
    public class EnvironmentCaller : DccHttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/env";

        public EnvironmentCaller(
            IServiceProvider serviceProvider,
            DccApiGatewayOptions options) : base(serviceProvider, options)
        {
        }

        public async Task<EnvironmentDetailModel> GetAsync(int Id)
        {
            var result = await Caller.GetAsync<EnvironmentDetailModel>($"{_prefix}/{Id}");

            return result ?? new();
        }

        public async Task<List<EnvironmentModel>> GetListAsync()
        {
            var result = await Caller.GetAsync<List<EnvironmentModel>>($"{_prefix}");

            return result ?? new();
        }
    }
}
