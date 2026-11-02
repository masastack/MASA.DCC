// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.App;

/// <summary>
/// Search condition of config objects. A null member means "no limit" on that dimension.
/// </summary>
public class ConfigObjectSearchCondition
{
    /// <summary>Allowed environment cluster ids.</summary>
    public List<int>? EnvironmentClusterIds { get; set; }

    /// <summary>Allowed app ids (matches app config objects).</summary>
    public List<int>? AppIds { get; set; }

    /// <summary>Lower-cased biz config identity (matches project level config objects).</summary>
    public string? BizIdentity { get; set; }

    public string? Name { get; set; }

    public string? Content { get; set; }

    /// <summary>Decrypts encrypted content so that it can be matched by <see cref="Content"/>.</summary>
    public Func<string, string>? ContentDecryptor { get; set; }
}
