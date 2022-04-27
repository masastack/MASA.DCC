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
}
