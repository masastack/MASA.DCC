// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Domain.Services;

public interface IConfigPublishStore
{
    Task SetAsync(
        string environment,
        string cluster,
        string appId,
        string configObjectName,
        PublishReleaseModel model,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string environment,
        string cluster,
        string appId,
        string configObjectName,
        CancellationToken cancellationToken = default);

    Task<PublishReleaseModel?> GetAsync(
        string environment,
        string cluster,
        string appId,
        string configObjectName,
        CancellationToken cancellationToken = default);
}
