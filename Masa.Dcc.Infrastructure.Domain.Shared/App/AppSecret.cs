// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Shared;

[Table("AppSecrets")]
public class AppSecret : FullAggregateRoot<int, Guid>
{
    [Required]
    [Range(1, int.MaxValue)]
    public int AppId { get; private set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int EnvironmentId { get; private set; }

    [Required]
    [Range(1, int.MaxValue)]
    public SecretType Type { get; private set; }

    [Required]
    public Guid EncryptionSecret { get; private set; }

    [Required]
    public Guid Secret { get; private set; }
}
