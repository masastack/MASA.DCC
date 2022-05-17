// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services
{
    public class BizConfigService : ServiceBase
    {
        public BizConfigService(IServiceCollection services) : base(services)
        {
            App.MapPost("api/v1/bizConfig", AddAsync);
            App.MapPut("api/v1/bizConfig", UpdateAsync);
            App.MapGet("api/v1/bizConfig/{identity}", GetAsync);
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
