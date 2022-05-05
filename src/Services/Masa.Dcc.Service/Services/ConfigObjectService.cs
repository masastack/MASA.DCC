// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace Masa.Dcc.Service.Services;

public class ConfigObjectService : ServiceBase
{
    public ConfigObjectService(IServiceCollection services) : base(services)
    {
        App.MapPost("api/v1/configObject", AddAsync);
        App.MapDelete("api/v1/configObject/{Id}", RemoveAsync);
        App.MapGet("api/v1/configObject/{envClusterId}", GetListByEnvClusterIdAsync);
        App.MapPut("api/v1/configObject", UpdateConfigObjectContentAsync);
        App.MapPost("api/v1/configObject/release", AddConfigObjectReleaseAsync);
        App.MapPut("api/v1/configObject/rollback", RollbackAsync);
        App.MapGet("api/v1/configObject/release/history", GetConfigObjectReleaseHistoryAsync);
    }

    public async Task AddAsync(IEventBus eventBus, AddConfigObjectDto dto)
    {
        await eventBus.PublishAsync(new AddConfigObjectCommand(dto));
    }

    public async Task RemoveAsync(IEventBus eventBus, [FromQuery] int Id)
    {
        await eventBus.PublishAsync(new RemoveConfigObjectCommand(Id));
    }

    public async Task<List<ConfigObjectDto>> GetListByEnvClusterIdAsync(IEventBus eventBus, int envClusterId, string configObjectName = "")
    {
        var query = new ConfigObjectsQuery(envClusterId, configObjectName);
        await eventBus.PublishAsync(query);

        return query.Result;
    }

    public async Task<ConfigObjectDto> UpdateConfigObjectContentAsync(IEventBus eventBus, UpdateConfigObjectContentDto dto)
    {
        var command = new UpdateConfigObjectContentCommand(dto);
        await eventBus.PublishAsync(command);

        return command.Result;
    }

    #region ConfigObjectRelease

    public async Task AddConfigObjectReleaseAsync(IEventBus eventBus, AddConfigObjectReleaseDto dto)
    {
        await eventBus.PublishAsync(new AddConfigObjectReleaseCommand(dto));
    }

    public async Task RollbackAsync(IEventBus eventBus, RollbackConfigObjectReleaseDto dto)
    {
        await eventBus.PublishAsync(new RollbackConfigObjectReleaseCommand(dto));
    }

    public async Task<ConfigObjectWithReleaseHistoryDto> GetConfigObjectReleaseHistoryAsync(IEventBus eventBus, int configObejctId)
    {
        var query = new ConfigObjectReleaseQuery(configObejctId);
        await eventBus.PublishAsync(query);

        return query.Result;
    }

    #endregion
}
