// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.ApiGateways.Caller
{
    public class ConfigObjectCaller : DccHttpClientCallerBase
    {
        private readonly string _prefix = "/api/v1";

        public ConfigObjectCaller(DccApiGatewayOptions options) : base(options)
        {
        }

        public async Task<List<LatestReleaseConfigModel>> GetLatestReleaseConfigAsync(
            LatestReleaseConfigRequestDto<ProjectModel> request)
        {
            var result = await Caller.PostAsync<List<LatestReleaseConfigModel>>(
                $"{_prefix}/bizConfig/latestReleaseConfig",
                request);
            return result ?? new();
        }

        public async Task<BizConfigDto> GetBizConfigAsync(string identity)
        {
            var result = await Caller.GetAsync<BizConfigDto>($"{_prefix}/bizConfig/{identity}");

            return result ?? new();
        }

        public async Task<BizConfigDto> AddBizConfigAsync(AddObjectConfigDto dto)
        {
            var bizConfig = await Caller.PostAsync<AddObjectConfigDto, BizConfigDto>($"{_prefix}/bizConfig", dto);

            return bizConfig ?? new();
        }

        public async Task<BizConfigDto> UpdateBizConfigAsync(UpdateObjectConfigDto dto)
        {
            var bizConfig = await Caller.PutAsync<UpdateObjectConfigDto, BizConfigDto>($"{_prefix}/bizConfig", dto);

            return bizConfig ?? new();
        }

        public async Task<List<ConfigObjectDto>> GetConfigObjectsAsync(int envClusterId, int objectId, ConfigObjectType type, string configObjectName = "", bool getLatestRelease = false)
        {
            var url =
                $"{_prefix}/configObjects/{envClusterId}/{objectId}/{type}/{getLatestRelease}/{configObjectName}";
            var result = await Caller.GetAsync<List<ConfigObjectDto>>(url);

            return result ?? new();
        }

        public async Task<List<ConfigObjectDto>> GetConfigObjectsByIdsAsync(List<int> ids)
        {
            var result = await Caller.PostAsync<List<int>, List<ConfigObjectDto>>($"{_prefix}/configObjects/getListByIds", ids);

            return result ?? new();
        }

        public async Task AddConfigObjectAsync(List<AddConfigObjectDto> dtos)
        {
            await Caller.PostAsync($"{_prefix}/configObject", dtos);
        }

        public async Task UpdateConfigObjectContentAsync(UpdateConfigObjectContentDto dto)
        {
            await Caller.PutAsync($"{_prefix}/configObject", dto);
        }

        public async Task<List<PublicConfigDto>> GetPublicConfigAsync()
        {
            var result = await Caller.GetAsync<List<PublicConfigDto>>($"{_prefix}/publicConfig");

            return result ?? new();
        }

        public async Task<PublicConfigDto> AddPublicConfigAsync(AddObjectConfigDto dto)
        {
            var result = await Caller.PostAsync<AddObjectConfigDto, PublicConfigDto>($"{_prefix}/publicConfig", dto);

            return result ?? new();
        }

        public async Task RemoveAsync(RemoveConfigObjectDto dto)
        {
            await Caller.DeleteAsync($"{_prefix}/configObject", dto);
        }

        public async Task ReleaseAsync(AddConfigObjectReleaseDto dto)
        {
            await Caller.PostAsync($"{_prefix}/configObject/release", dto);
        }

        public async Task RevokeAsync(int configObjectId)
        {
            await Caller.PutAsync($"{_prefix}/configObject/revoke/{configObjectId}", null);
        }

        public async Task<ConfigObjectWithReleaseHistoryDto> GetReleaseHistoryAsync(int configObjectId)
        {
            var result = await Caller.GetAsync<ConfigObjectWithReleaseHistoryDto>($"{_prefix}/configObject/release/history/{configObjectId}");

            return result ?? new();
        }

        public async Task RollbackAsync(RollbackConfigObjectReleaseDto dto)
        {
            await Caller.PutAsync($"{_prefix}/configObject/rollback", dto);
        }

        public async Task CloneAsync(CloneConfigObjectDto dto)
        {
            await Caller.PostAsync($"{_prefix}/configObject/clone", dto);
        }

        public async Task<PublicConfigObjectDto> GetPublicConfigObjectAsync(int configObjectId)
        {
            var result = await Caller.GetAsync<PublicConfigObjectDto>($"{_prefix}/pubConfigObjects?configObjectId={configObjectId}");

            return result ?? new();
        }
    }
}
