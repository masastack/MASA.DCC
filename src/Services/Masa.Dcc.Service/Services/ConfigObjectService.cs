// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

public class ConfigObjectService : ServiceBase
{
    private readonly IAuthClient _authClient;
    public ConfigObjectService(IAuthClient authClient)
    {
        _authClient = authClient;
        App.MapPost("api/v1/configObject", AddAsync);
        App.MapDelete("api/v1/configObject", RemoveAsync);
        App.MapGet("api/v1/configObjects/{envClusterId}/{objectId}/{type}/{getLatestRelease}", GetListAsync);
        App.MapGet("api/v1/configObjects/{envClusterId}/{objectId}/{type}/{getLatestRelease}/{configObjectName}", GetListAsync);
        App.MapPost("api/v1/configObjects/getListByIds", GetListByIdsAsync);
        App.MapPut("api/v1/configObject", UpdateConfigObjectContentAsync);
        App.MapPost("api/v1/configObject/release", AddConfigObjectReleaseAsync);
        App.MapPut("api/v1/configObject/revoke/{id}", RevokeConfigObjectAsync);
        App.MapPut("api/v1/configObject/rollback", RollbackAsync);
        App.MapPost("api/v1/configObject/clone", CloneConfigObjectAsync);
        App.MapGet("api/v1/configObject/release/history/{configObjectId}", GetConfigObjectReleaseHistoryAsync);
        App.MapGet("api/v1/configObject/refresh", RefreshConfigObjectToRedisAsync);
    }

    public async Task AddAsync(IEventBus eventBus, List<AddConfigObjectDto> dtos)
    {
        await eventBus.PublishAsync(new AddConfigObjectCommand(dtos));
    }

    public async Task RemoveAsync(IEventBus eventBus, [FromBody] RemoveConfigObjectDto dto)
    {
        await eventBus.PublishAsync(new RemoveConfigObjectCommand(dto));
    }

    public async Task<List<ConfigObjectDto>> GetListAsync(
        IEventBus eventBus, int envClusterId, int objectId, ConfigObjectType type, string configObjectName = "", bool getLatestRelease = false)
    {
        var query = new ConfigObjectsQuery(envClusterId, objectId, type, configObjectName, getLatestRelease);
        await eventBus.PublishAsync(query);
        query.Result = await _authClient.FillUserNameAsync(query.Result);
        return query.Result;
    }

    public async Task<List<ConfigObjectDto>> GetListByIdsAsync(IEventBus eventBus, [FromBody] List<int> ids)
    {
        var query = new ConfigObjectListQuery(ids);
        await eventBus.PublishAsync(query);

        return query.Result;
    }

    public async Task UpdateConfigObjectContentAsync(IEventBus eventBus, UpdateConfigObjectContentDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.FormatLabelCode) && !string.IsNullOrWhiteSpace(dto.Content))
        {
            dto.FormatLabelCode = dto.FormatLabelCode.ToLower();

            switch (dto.FormatLabelCode)
            {
                case "json":
                    try
                    {
                        if (dto.Content.StartsWith("["))
                            JArray.Parse(dto.Content);
                        else
                            JObject.Parse(dto.Content);
                    }
                    catch
                    {
                        throw new Exception(I18n.T("Wrong format"));
                    }
                    break;
                case "xml":
                    try
                    {
                        XElement.Parse(dto.Content);
                    }
                    catch
                    {
                        throw new Exception(I18n.T("Wrong format"));
                    }
                    break;
                case "yaml":
                default:
                    break;
            }
        }

        var command = new UpdateConfigObjectContentCommand(dto);
        await eventBus.PublishAsync(command);
    }

    public async Task RevokeConfigObjectAsync(IEventBus eventBus, [FromRoute] int id)
    {
        await eventBus.PublishAsync(new RevokeConfigObjectCommand(id));
    }

    public async Task CloneConfigObjectAsync(IEventBus eventBus, CloneConfigObjectDto dto)
    {
        await eventBus.PublishAsync(new CloneConfigObjectCommand(dto));
    }

    public async Task<string> RefreshConfigObjectToRedisAsync(IEventBus eventBus)
    {
        var query = new RefreshConfigObjectToRedisQuery();
        await eventBus.PublishAsync(query);

        return query.Result;
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

    public async Task<ConfigObjectWithReleaseHistoryDto> GetConfigObjectReleaseHistoryAsync(IEventBus eventBus, int configObjectId)
    {
        var query = new ConfigObjectReleaseQuery(configObjectId);
        await eventBus.PublishAsync(query);

        return query.Result;
    }

    #endregion
}
