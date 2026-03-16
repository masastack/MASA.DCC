// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain;

internal static class CacheUtils
{
    public const string CacheKeyPrefix = Masa.Contrib.Configuration.ConfigurationApi.Dcc.Internal.Constants.DEFAULT_PREFIX;

    static JsonSerializerOptions? _jsonSerializerOptions = null;

    public static async Task RemoveAsync(string key, StackExchange.Redis.IConnectionMultiplexer? connection, IMultilevelCacheClient? multilevelCacheClient)
    {
        if (connection != null)
        {
            await connection.GetDatabase().KeyDeleteAsync(key);
        }
        else
        {
            await multilevelCacheClient!.RemoveAsync<PublishReleaseModel>(key);
        }
    }

    public static async Task SetAsync(string key, StackExchange.Redis.IConnectionMultiplexer? connection, IMultilevelCacheClient? multilevelCacheClient, PublishReleaseModel publishRelease)
    {
        if (connection != null)
        {
            _jsonSerializerOptions ??= GetSerializerOptions();
            await connection.GetDatabase().StringSetAsync(key, JsonSerializer.Serialize(publishRelease, _jsonSerializerOptions));
        }
        else
        {
            await multilevelCacheClient!.SetAsync(key, publishRelease);
        }
    }

    public static async Task<bool> ExistsAsync(string key, StackExchange.Redis.IConnectionMultiplexer? connection, IMultilevelCacheClient? multilevelCacheClient)
    {
        if (connection != null)
        {
            return await connection.GetDatabase().KeyExistsAsync(key);
        }
        else
        {
            return await multilevelCacheClient!.GetAsync<PublishReleaseModel>(key) != null;
        }
    }

    private static JsonSerializerOptions GetSerializerOptions()
    {
        var globalJsonSerializerOptions = MasaApp.GetJsonSerializerOptions();
        var jsonSerializerOption = globalJsonSerializerOptions != null ?
            new JsonSerializerOptions(globalJsonSerializerOptions)
            {
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            } :
            new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
        return jsonSerializerOption;
    }
}
