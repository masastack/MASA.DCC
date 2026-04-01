// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.App;

public interface IPublicConfigObjectRepository : IRepositoryExtensition<PublicConfigObject>
{
    Task<List<PublicConfigObject>> GetListByEnvClusterIdAsync(int? envClusterId, int publicConfigId,
        bool getLatestRelease = false);

    Task<PublicConfigObject> GetByConfigObjectIdAsync(int configObjectId);

    Task<List<PublicConfigObject>> GetListByPublicConfigIdAsync(int publicConfigId);
}
