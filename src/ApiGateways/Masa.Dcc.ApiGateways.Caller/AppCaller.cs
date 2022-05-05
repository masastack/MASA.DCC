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

        protected override string BaseAddress { get; set; } = "";

        public async Task<List<AppDetailModel>> GetListAsync()
        {
            var result = await CallerProvider.GetAsync<List<AppDetailModel>>(_prefix);

            return result ?? new();
        }

        public async Task<List<AppDetailModel>> GetListByProjectIdAsync(List<int> projectIds)
        {
            var result = await CallerProvider.PostAsync<List<int>, List<AppDetailModel>>($"/api/v1/projects/app", projectIds);

            return result ?? new();
        }
    }
}
