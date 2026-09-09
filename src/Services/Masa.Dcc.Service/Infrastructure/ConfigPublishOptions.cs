// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure;

public class ConfigPublishOptions
{
    public const string SectionName = "ConfigPublish";

    public ConfigPublishStorageType StorageType { get; set; } = ConfigPublishStorageType.Redis;

    /// <summary>
    /// Dapr Configuration store component name.
    /// Must stay in sync with <c>DCC_STORE_NAME</c> (SDK bootstrap) and the
    /// configuration.redis component metadata.name (default: masa-stack-dcc).
    /// </summary>
    public string ConfigurationStoreName { get; set; } = "masa-stack-dcc";

    /// <summary>
    /// Redis backing the Configuration store. Must match the configuration.redis component
    /// (not the state.redis key encoding). Required when StorageType is Dapr.
    /// </summary>
    public ConfigPublishRedisOptions Redis { get; set; } = new();
}

public class ConfigPublishRedisOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 6379;

    public int Db { get; set; } = 1;

    public string Password { get; set; } = "";
}
