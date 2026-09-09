// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Contracts.Admin;

public static class DccDaprConfigKeys
{
    /// <summary>
    /// Same prefix as Masa DaprConfigurationApiClient.FomartKey (Constants.DEFAULT_PREFIX).
    /// </summary>
    public const string ConfigurationKeyPrefix = "dcc.dapr.config.";

    [Obsolete("Use ConfigurationKeyPrefix / BuildConfigurationKey")]
    public const string StateKeyPrefix = ConfigurationKeyPrefix;

    /// <summary>
    /// Logical key for Dapr Configuration Redis (no State appId|| prefix).
    /// </summary>
    public static string BuildConfigurationKey(string environment, string cluster, string appId, string configObject)
        => $"{ConfigurationKeyPrefix}{environment}-{cluster}-{appId}-{configObject}".ToLowerInvariant();

    [Obsolete("Use BuildConfigurationKey")]
    public static string BuildStateKey(string environment, string cluster, string appId, string configObject)
        => BuildConfigurationKey(environment, cluster, appId, configObject);

    public static string BuildCacheKey(string environment, string cluster, string appId, string configObject)
        => $"{environment}-{cluster}-{appId}-{configObject}".ToLowerInvariant();
}
