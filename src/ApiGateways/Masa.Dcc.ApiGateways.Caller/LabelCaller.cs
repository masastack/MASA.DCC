// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.ApiGateways.Caller
{
    public class LabelCaller : HttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1/label";

        public LabelCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(LabelCaller);
        }

        protected override string BaseAddress { get; set; } = AppSettings.Get("ServiceBaseUrl");

        public async Task<List<LabelDto>> GetLabelsByTypeCodeAsync(string typeCode)
        {
            var result = await CallerProvider.GetAsync<List<LabelDto>>($"/api/v1/{typeCode}/labels");

            return result ?? new();
        }
    }
}
