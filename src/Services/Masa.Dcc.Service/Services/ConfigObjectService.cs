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
        App.MapGet("api/v1/configObjects", GetListAsync);
        App.MapPost("api/v1/configObjects/getListbyIds", GetListByIdsAsync);
        App.MapPut("api/v1/configObject", UpdateConfigObjectContentAsync);
        App.MapPost("api/v1/configObject/release", AddConfigObjectReleaseAsync);
        App.MapPut("api/v1/configObject/revoke/{Id}", RevokeConfigObjectAsync);
        App.MapPut("api/v1/configObject/rollback", RollbackAsync);
        App.MapPost("api/v1/configObject/clone", CloneConfigObjectAsync);
        App.MapPost("api/v1/configObject/relation", RelationConfigObjectAsync);
        App.MapGet("api/v1/configObject/release/history/{configObejctId}", GetConfigObjectReleaseHistoryAsync);
    }

    public async Task AddAsync(IEventBus eventBus, List<AddConfigObjectDto> dtos)
    {
        await eventBus.PublishAsync(new AddConfigObjectCommand(dtos));
    }

    public async Task RemoveAsync(IEventBus eventBus, [FromRoute] int Id)
    {
        await eventBus.PublishAsync(new RemoveConfigObjectCommand(Id));
    }

    public async Task<List<ConfigObjectDto>> GetListAsync(
        IEventBus eventBus, int envClusterId, int objectId, ConfigObjectType type, string configObjectName = "")
    {
        var query = new ConfigObjectsQuery(envClusterId, objectId, type, configObjectName);
        await eventBus.PublishAsync(query);

        return query.Result;
    }

    public async Task<List<ConfigObjectDto>> GetListByIdsAsync(IEventBus eventBus, List<int> Ids)
    {
        var query = new ConfigObjectListQuery(Ids);
        await eventBus.PublishAsync(query);

        return query.Result;
    }

    public async Task UpdateConfigObjectContentAsync(IEventBus eventBus, UpdateConfigObjectContentDto dto)
    {
        var command = new UpdateConfigObjectContentCommand(dto);
        await eventBus.PublishAsync(command);
    }

    public async Task RevokeConfigObjectAsync(IEventBus eventBus, [FromRoute] int Id)
    {
        await eventBus.PublishAsync(new RevokeConfigObjectCommand(Id));
    }

    public async Task CloneConfigObjectAsync(IEventBus eventBus, CloneConfigObjectDto dto)
    {
        await eventBus.PublishAsync(new CloneConfigObjectCommand(dto));
    }

    public async Task RelationConfigObjectAsync(IEventBus eventBus, List<RelationConfigObjectDto> dtos)
    {
        await eventBus.PublishAsync(new RelationConfigObjectCommand(dtos));
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
