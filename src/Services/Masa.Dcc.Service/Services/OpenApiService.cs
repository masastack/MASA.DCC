// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services
{
    public class OpenApiService : ServiceBase
    {
        public OpenApiService(IServiceCollection services) : base(services)
        {
            App.MapPut("open-api/releasing/{environment}/{cluster}/{appId}/{configObject}", UpdateConfigObjectAsync);
            App.MapPost("open-api/releasing/init/{environment}/{cluster}/{appId}", InitConfigObjectAsync);
        }

        public async Task UpdateConfigObjectAsync(IEventBus eventBus, string environment, string cluster, string appId, string configObject,
            [FromBody] string value)
        {
            await eventBus.PublishAsync(
                new UpdateConfigAndPublishCommand(environment, cluster, appId, configObject, value));
        }

        public async Task InitConfigObjectAsync(IEventBus eventBus, string environment, string cluster, string appId, [FromBody] Dictionary<string, string> configObjects)
        {
            await eventBus.PublishAsync(
                new InitConfigObjectCommand(environment, cluster, appId, configObjects));
        }
    }
}
