// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services
{
    public class OpenApiService : ServiceBase
    {
        public OpenApiService()
        {
            App.MapPut("open-api/releasing/{environment}/{cluster}/{appId}/{configObject}", UpdateConfigObjectAsync);
            App.MapPost("open-api/releasing/{environment}/{cluster}/{appId}/{isEncryption}", AddConfigObjectAsync);
            App.MapPost("open-api/releasing/get/{environment}/{cluster}/{appId}/{configObjects}", GetConfigObjectsAsync);
        }

        public async Task UpdateConfigObjectAsync(IEventBus eventBus, string environment, string cluster, string appId, string configObject,
            [FromBody] string value)
        {
            await eventBus.PublishAsync(
                new UpdateConfigAndPublishCommand(environment, cluster, appId, configObject, value));
        }

        public async Task AddConfigObjectAsync(IEventBus eventBus, string environment, string cluster, string appId,
            [FromBody] Dictionary<string, string> configObjects, ConfigObjectType configObjectType = ConfigObjectType.App,
            bool isEncryption = false)
        {
            await eventBus.PublishAsync(
                new InitConfigObjectCommand(environment, cluster, appId, configObjects, configObjectType, isEncryption));
        }

        public async Task<Dictionary<string, PublishReleaseModel>> GetConfigObjectsAsync(IEventBus eventBus, string environment, string cluster, string appId,
           [FromBody] List<string>? configObjects)
        {
            var query = new ConfigObjectsByDynamicQuery(environment, cluster, appId, configObjects);
            await eventBus.PublishAsync(query);
            return query.Result;
        }
    }
}
