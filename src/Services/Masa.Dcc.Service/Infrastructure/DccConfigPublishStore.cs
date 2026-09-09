// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.StackSdks.Dcc.Contracts.Enum;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Masa.Dcc.Service.Admin.Infrastructure;

/// <summary>
/// Publishes config for Dapr Configuration API consumers.
/// Dapr has no Configuration Set API; State and Configuration Redis encodings differ,
/// so we write the Configuration Redis wire format directly:
/// key = dcc.dapr.config.{env}-{cluster}-{appId}-{configObject}
/// value = {PublishReleaseModel json}||{version}
/// </summary>
public class DccConfigPublishStore : IConfigPublishStore, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _connection;
    private readonly int _db;

    public DccConfigPublishStore(IOptions<ConfigPublishOptions> options)
    {
        var redis = options.Value.Redis ?? new ConfigPublishRedisOptions();
        var config = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            DefaultDatabase = redis.Db
        };
        config.EndPoints.Add(redis.Host, redis.Port);
        if (!string.IsNullOrEmpty(redis.Password))
            config.Password = redis.Password;

        _db = redis.Db;
        _connection = ConnectionMultiplexer.Connect(config);
    }

    public async Task SetAsync(
        string environment,
        string cluster,
        string appId,
        string configObjectName,
        PublishReleaseModel model,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigFormat(model);
        var key = DccDaprConfigKeys.BuildConfigurationKey(environment, cluster, appId, configObjectName);
        var db = _connection.GetDatabase(_db);
        var existing = await db.StringGetAsync(key).ConfigureAwait(false);
        var version = NextVersion(existing);
        var payload = $"{JsonSerializer.Serialize(model, JsonOptions)}||{version}";
        await db.StringSetAsync(key, payload).ConfigureAwait(false);
    }

    public async Task RemoveAsync(
        string environment,
        string cluster,
        string appId,
        string configObjectName,
        CancellationToken cancellationToken = default)
    {
        var key = DccDaprConfigKeys.BuildConfigurationKey(environment, cluster, appId, configObjectName);
        var db = _connection.GetDatabase(_db);
        await db.KeyDeleteAsync(key).ConfigureAwait(false);
    }

    public async Task<PublishReleaseModel?> GetAsync(
        string environment,
        string cluster,
        string appId,
        string configObjectName,
        CancellationToken cancellationToken = default)
    {
        var key = DccDaprConfigKeys.BuildConfigurationKey(environment, cluster, appId, configObjectName);
        var db = _connection.GetDatabase(_db);
        var raw = await db.StringGetAsync(key).ConfigureAwait(false);
        if (raw.IsNullOrEmpty)
            return null;

        var value = SplitValueAndVersion(raw!);
        return JsonSerializer.Deserialize<PublishReleaseModel>(value, JsonOptions);
    }

    public void Dispose() => _connection.Dispose();

    private static void EnsureConfigFormat(PublishReleaseModel model)
    {
        if (model.ConfigFormat != 0)
            return;

        model.ConfigFormat = model.FormatLabelCode?.Trim().ToLowerInvariant() switch
        {
            "properties" => ConfigFormats.Properties,
            "raw" => ConfigFormats.RAW,
            "json" => ConfigFormats.JSON,
            "yaml" => ConfigFormats.YAML,
            "xml" => ConfigFormats.XML,
            _ => ConfigFormats.JSON
        };
    }

    private static long NextVersion(RedisValue existing)
    {
        if (existing.IsNullOrEmpty)
            return 1;

        var text = (string)existing!;
        var idx = text.LastIndexOf("||", StringComparison.Ordinal);
        if (idx < 0 || idx + 2 >= text.Length)
            return 1;

        return long.TryParse(text[(idx + 2)..], out var version) ? version + 1 : 1;
    }

    private static string SplitValueAndVersion(string redisValue)
    {
        var idx = redisValue.LastIndexOf("||", StringComparison.Ordinal);
        return idx < 0 ? redisValue : redisValue[..idx];
    }
}
