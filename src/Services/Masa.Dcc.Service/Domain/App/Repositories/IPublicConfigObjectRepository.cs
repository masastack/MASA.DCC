// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Domain.App.Repositories
{
    public interface IPublicConfigObjectRepository : IRepository<PublicConfigObject>
    {
        Task<List<PublicConfigObject>> GetListByEnvClusterIdAsync(int? envClusterId, int publicConfigId);

        Task<PublicConfigObject> GetByConfigObjectIdAsync(int configObjectId);
    }
}
