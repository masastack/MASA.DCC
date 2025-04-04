﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

public class PublicConfigService : ServiceBase
{
    public PublicConfigService()
    {
        RouteOptions.DisableAutoMapRoute = true;
        App.MapPost("api/v1/publicConfig", AddAsync).RequireAuthorization().RequireAuthorization();
        App.MapPut("api/v1/publicConfig", UpdateAsync).RequireAuthorization().RequireAuthorization();
        App.MapDelete("api/v1/publicConfig/{Id}", RemoveAsync).RequireAuthorization().RequireAuthorization();
        App.MapGet("api/v1/publicConfig", GetListAsync).RequireAuthorization().RequireAuthorization();
        App.MapGet("api/v1/pubConfigObjects", GetByConfigObjectIdAsync).RequireAuthorization();
    }

    public async Task<PublicConfigDto> AddAsync(IEventBus eventBus, AddObjectConfigDto dto)
    {
        var command = new AddPublicConfigCommand(dto);
        await eventBus.PublishAsync(command);

        return command.PublicConfigDto;
    }

    public async Task UpdateAsync(IEventBus eventBus, UpdateObjectConfigDto dto)
    {
        await eventBus.PublishAsync(new UpdatePublicConfigCommand(dto));
    }

    public async Task RemoveAsync(IEventBus eventBus, [FromQuery] int Id)
    {
        await eventBus.PublishAsync(new RemovePublicConfigCommand(Id));
    }

    public async Task<List<PublicConfigDto>> GetListAsync(IEventBus eventBus)
    {
        var query = new PublicConfigsQuery();
        await eventBus.PublishAsync(query);

        return query.Result;
    }

    public async Task<PublicConfigObjectDto> GetByConfigObjectIdAsync(IEventBus eventBus, int configObjectId)
    {
        var query = new PublicConfigObjectQuery(configObjectId);
        await eventBus.PublishAsync(query);

        return query.Result;
    }
}
