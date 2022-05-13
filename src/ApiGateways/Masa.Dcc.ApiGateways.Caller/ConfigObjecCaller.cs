// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.ApiGateways.Caller
{
    public class ConfigObjecCaller : HttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1";

        public ConfigObjecCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(ConfigObjecCaller);
        }

        protected override string BaseAddress { get; set; } = AppSettings.Get("ServiceBaseUrl");

        public async Task<BizConfigDto> GetBizConfigAsync(string identity)
        {
            var result = await CallerProvider.GetAsync<BizConfigDto>($"{_prefix}/bizConfig/{identity}");

            return result ?? new();
        }

        public async Task<BizConfigDto> AddBizConfigAsync(AddObjectConfigDto dto)
        {
            var bizConfig = await CallerProvider.PostAsync<AddObjectConfigDto, BizConfigDto>($"{_prefix}/bizConfig", dto);

            return bizConfig ?? new();
        }

        public async Task<BizConfigDto> UpdateBizConfigAsync(UpdateObjectConfigDto dto)
        {
            var bizConfig = await CallerProvider.PutAsync<UpdateObjectConfigDto, BizConfigDto>($"{_prefix}/bizConfig", dto);

            return bizConfig ?? new();
        }
    }
}
