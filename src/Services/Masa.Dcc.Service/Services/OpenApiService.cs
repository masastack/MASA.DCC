// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

[Authorize]
public class OpenApiService : ServiceBase
{
    public OpenApiService()
    {
        RouteOptions.DisableAutoMapRoute = true;
        App.MapPut("open-api/releasing/{environment}/{cluster}/{appId}/{configObject}", UpdateConfigObjectAsync).RequireAuthorization();
        App.MapPost("open-api/releasing/{environment}/{cluster}/{appId}/{isEncryption}", AddConfigObjectAsync).RequireAuthorization();
        App.MapPost("open-api/releasing/get/{environment}/{cluster}/{appId}", GetConfigObjectsAsync).RequireAuthorization();
        App.MapGet("open-api/releasing/{environment}/{cluster}/stack-config", GetStackConfigAsync).RequireAuthorization();
        App.MapGet("open-api/releasing/{environment}/{cluster}/i18n/{culture}", GetI18NConfigAsync).RequireAuthorization();
    }

    [AllowAnonymous]
    public async Task UpdateConfigObjectAsync(IEventBus eventBus, string environment, string cluster, string appId, string configObject,
        [FromBody] string value)
    {
        await eventBus.PublishAsync(
            new UpdateConfigAndPublishCommand(environment, cluster, appId, configObject, value));
    }

    [AllowAnonymous]
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

    public async Task<Dictionary<string, string>> GetStackConfigAsync(IEventBus eventBus, string environment, string cluster)
    {
        var query = new StackConfigQuery(environment, cluster);
        await eventBus.PublishAsync(query);
        return query.Result;
    }

    public async Task<Dictionary<string, string>> GetI18NConfigAsync(IEventBus eventBus, string environment, string cluster, string culture)
    {
        var query = new I18NConfigQuery(environment, cluster, culture);
        await eventBus.PublishAsync(query);
        return query.Result;
    }
}
