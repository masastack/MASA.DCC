// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Dcc.Contracts.Admin.App.Enums;

namespace Masa.Dcc.ApiGateways.Caller
{
    public class ConfigObjectCaller : HttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1";

        public ConfigObjectCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(ConfigObjectCaller);
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

        public async Task<List<ConfigObjectDto>> GetConfigObjectsAsync(int envClusterId, int objectId, ConfigObjectType type, string configObjectName = "")
        {
            var result = await CallerProvider.GetAsync<List<ConfigObjectDto>>($"{_prefix}/configObjects?envClusterId={envClusterId}&objectId={objectId}&type={type}&configObjectName={configObjectName}");

            return result ?? new();
        }

        public async Task AddConfigObjectAsync(List<AddConfigObjectDto> dtos)
        {
            await CallerProvider.PostAsync($"{_prefix}/configObject", dtos);
        }

        public async Task UpdateConfigObjectContentAsync(UpdateConfigObjectContentDto dto)
        {
            await CallerProvider.PutAsync($"{_prefix}/configObject", dto);
        }

        public async Task<List<PublicConfigDto>> GetPublicConfigAsync()
        {
            var result = await CallerProvider.GetAsync<List<PublicConfigDto>>($"{_prefix}/publicConfig");

            return result ?? new();
        }

        public async Task<PublicConfigDto> AddPublicConfigAsync(AddObjectConfigDto dto)
        {
            var result = await CallerProvider.PostAsync<AddObjectConfigDto, PublicConfigDto>($"{_prefix}/publicConfig", dto);

            return result ?? new();
        }

        public async Task RemoveAsync(int configObjectId)
        {
            await CallerProvider.DeleteAsync($"{_prefix}/configObject/{configObjectId}", null);
        }

        public async Task ReleaseAsync(AddConfigObjectReleaseDto dto)
        {
            await CallerProvider.PostAsync($"{_prefix}/configObject/release", dto);
        }

        public async Task RevokeAsync(int configObjectId)
        {
            await CallerProvider.PutAsync($"{_prefix}/configObject/revoke/{configObjectId}", null);
        }

        public async Task<ConfigObjectWithReleaseHistoryDto> GetReleaseHistoryAsync(int configObjectId)
        {
            var result = await CallerProvider.GetAsync<ConfigObjectWithReleaseHistoryDto>($"{_prefix}/configObject/release/history/{configObjectId}");

            return result ?? new();
        }

        public async Task RollbackAsync(RollbackConfigObjectReleaseDto dto)
        {
            await CallerProvider.PutAsync($"{_prefix}/configObject/rollback", dto);
        }
    }
}
