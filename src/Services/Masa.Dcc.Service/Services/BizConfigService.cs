// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services
{
    public class BizConfigService : ServiceBase
    {

        public BizConfigService()
        {
            App.MapPost("api/v1/bizConfig", AddAsync);
            App.MapPut("api/v1/bizConfig", UpdateAsync);
            App.MapGet("api/v1/bizConfig/{identity}", GetAsync);
            App.MapPost("api/v1/bizConfig/latestReleaseConfig", GetLatestReleaseConfigByProjectAsync);
        }

        public async Task<List<LatestReleaseConfigModel>> GetLatestReleaseConfigByProjectAsync(IEventBus eventBus,
            IAuthClient authClient,
            LatestReleaseConfigRequestDto<ProjectModel> request)
        {
            var query = new ProjectLatestReleaseQuery(request.Items, request.EnvClusterId);
            await eventBus.PublishAsync(query);
            return await authClient.FillUserNameAsync(query.Result);
        }

        public async Task<BizConfigDto> AddAsync(IEventBus eventBus, AddObjectConfigDto dto)
        {
            var command = new AddBizConfigCommand(dto);
            await eventBus.PublishAsync(command);

            return command.BizConfigDto;
        }

        public async Task<BizConfigDto> UpdateAsync(IEventBus eventBus, UpdateObjectConfigDto dto)
        {
            var command = new UpdateBizConfigCommand(dto);
            await eventBus.PublishAsync(command);

            return command.BizConfigDto;
        }

        public async Task<BizConfigDto> GetAsync(IEventBus eventBus, string identity)
        {
            var query = new BizConfigsQuery(identity);
            await eventBus.PublishAsync(query);

            return query.Result;
        }
    }
}
