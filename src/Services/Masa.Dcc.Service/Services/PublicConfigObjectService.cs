using Microsoft.AspNetCore.Mvc;

namespace Masa.Dcc.Service.Services;

public class PublicConfigObjectService : ServiceBase
{
    public PublicConfigObjectService(IServiceCollection services) : base(services)
    {
        App.MapPost("api/v1/publicConfigObject", AddAsync);
        App.MapDelete("api/v1/publicConfigObject/{Id}", RemoveAsync);
        App.MapGet("api/v1/publicConfigObject", GetListAsync);
    }

    public async Task AddAsync(IEventBus eventBus, AddPublicConfigObjectDto dto)
    {
        await eventBus.PublishAsync(new AddPublicConfigObjectCommand(dto));
    }

    public async Task RemoveAsync(IEventBus eventBus, [FromQuery] int Id)
    {
        await eventBus.PublishAsync(new RemovePublicConfigObjectCommand(Id));
    }

    public async Task GetListAsync(IEventBus eventBus)
    {
        await eventBus.PublishAsync(new PublicConfigObjectsQuery());
    }
}
