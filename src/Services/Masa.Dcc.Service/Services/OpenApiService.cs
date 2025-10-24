// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Services;

public class OpenApiService : ServiceBase
{
    public OpenApiService()
    {
        RouteOptions.DisableAutoMapRoute = true;
        App.MapPut("open-api/releasing/{environment}/{cluster}/{appId}/{configObject}", UpdateConfigObjectAsync);
        App.MapPost("open-api/releasing/{environment}/{cluster}/{appId}/{isEncryption}", AddConfigObjectAsync);
        App.MapPost("open-api/releasing/get/{environment}/{cluster}/{appId}", GetConfigObjectsAsync);
        App.MapGet("open-api/releasing/{environment}/{cluster}/stack-config", GetStackConfigAsync);
        App.MapGet("open-api/releasing/{environment}/{cluster}/i18n/{culture}", GetI18NConfigAsync);
        App.MapGet("open-api/oss-token", GetOssSecurityTokenAsync);
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

    private async Task<OssSecurityTokenDto> GetOssSecurityTokenAsync([FromServices] IObjectStorageClient client, IMultiEnvironmentContext multiEnvironmentContext, IMasaStackConfig masaStackConfig, [FromServices] IConfigurationApiClient configurationApiClient)
    {
        var environment = multiEnvironmentContext.CurrentEnvironment;
        if (environment.IsNullOrEmpty())
        {
            environment = masaStackConfig.Environment;
        }
        var ossOptions = await configurationApiClient.GetAsync<OssOptions>(environment, "Default", "public-$Config", DccConstants.OssKey);
        var cdn = await configurationApiClient.GetAsync<CdnDto>(environment, "Default", "public-$Config", DccConstants.CdnKey);

        MasaArgumentException.ThrowIfNull(ossOptions);
        MasaArgumentException.ThrowIfNull(cdn);
        var response = client.GetSecurityToken();
        return new OssSecurityTokenDto(ossOptions.RegionId, response.AccessKeyId, response.AccessKeySecret, response.SessionToken, ossOptions.Bucket, cdn.CdnEndpoint);
    }
}
