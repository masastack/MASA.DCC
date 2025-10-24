// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

[Authorize]
public class LabelService : ServiceBase
{
    public LabelService()
    {
        RouteOptions.DisableAutoMapRoute = true;
        App.MapGet("api/v1/labels", GetListAsync);
        App.MapGet("api/v1/{typeCode}/labels", GetLabelsByTypeCodeAsync);
        App.MapPost("api/v1/labels", AddAsync).AddFluentValidationAutoValidation();
        App.MapPut("api/v1/labels", UpdateAsync).AddFluentValidationAutoValidation();
        App.MapDelete("api/v1/labels/{typeCode}", RemoveAsync);
    }

    public async Task<List<LabelDto>> GetLabelsByTypeCodeAsync(IEventBus eventBus, string typeCode)
    {
        var query = new LabelsQuery(typeCode);
        await eventBus.PublishAsync(query);

        return query.Result;
    }

    public async Task<List<LabelDto>> GetListAsync(IEventBus eventBus)
    {
        var query = new LabelsQuery();
        await eventBus.PublishAsync(query);

        return query.Result;
    }

    public async Task AddAsync(IEventBus eventBus, UpdateLabelDto dto)
    {
        await eventBus.PublishAsync(new AddLabelCommand(dto));
    }

    public async Task UpdateAsync(IEventBus eventBus, UpdateLabelDto dto)
    {
        await eventBus.PublishAsync(new UpdateLabelCommand(dto));
    }

    public async Task RemoveAsync(IEventBus eventBus, string typeCode)
    {
        await eventBus.PublishAsync(new RemoveLabelCommand(typeCode));
    }
}
