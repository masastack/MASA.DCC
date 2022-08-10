// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Dcc.Caller;

namespace Masa.Dcc.ApiGateways.Caller
{
    public class LabelCaller : DccHttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/labels";

        public LabelCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(LabelCaller);
        }

        protected override string BaseAddress { get; set; } = AppSettings.Get("ServiceBaseUrl");

        public async Task<List<LabelDto>> GetListAsync()
        {
            var result = await CallerProvider.GetAsync<List<LabelDto>>(_prefix);

            return result ?? new();
        }

        public async Task<List<LabelDto>> GetLabelsByTypeCodeAsync(string typeCode)
        {
            var result = await CallerProvider.GetAsync<List<LabelDto>>($"/api/v1/{typeCode}/labels");

            return result ?? new();
        }

        public async Task AddAsync(UpdateLabelDto dto)
        {
            await CallerProvider.PostAsync(_prefix, dto);
        }

        public async Task UpdateAsync(UpdateLabelDto dto)
        {
            await CallerProvider.PutAsync(_prefix, dto);
        }

        public async Task RemoveAsync(string typeCode)
        {
            await CallerProvider.DeleteAsync($"{_prefix}/{typeCode}", null);
        }
    }
}
