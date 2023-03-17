// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.ApiGateways.Caller
{
    public class LabelCaller : DccHttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/labels";

        public LabelCaller(DccApiGatewayOptions options) : base(options)
        {
        }

        public async Task<List<LabelDto>> GetListAsync()
        {
            var result = await Caller.GetAsync<List<LabelDto>>(_prefix);

            return result ?? new();
        }

        public async Task<List<LabelDto>> GetLabelsByTypeCodeAsync(string typeCode)
        {
            var result = await Caller.GetAsync<List<LabelDto>>($"/api/v1/{typeCode}/labels");

            return result ?? new();
        }

        public async Task AddAsync(UpdateLabelDto dto)
        {
            await Caller.PostAsync(_prefix, dto);
        }

        public async Task UpdateAsync(UpdateLabelDto dto)
        {
            await Caller.PutAsync(_prefix, dto);
        }

        public async Task RemoveAsync(string typeCode)
        {
            await Caller.DeleteAsync($"{_prefix}/{typeCode}", null);
        }
    }
}
