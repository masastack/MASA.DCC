// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories
{
    public class PublicConfigObjectRepository : Repository<DccDbContext, PublicConfigObject>, IPublicConfigObjectRepository
    {
        public PublicConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }

        public async Task<List<PublicConfigObject>> GetListByEnvClusterIdAsync(int envClusterId, int publicConfigId)
        {
            var configData = await Context.Set<PublicConfigObject>()
                .Where(publicConfigObject => publicConfigObject.EnvironmentClusterId == envClusterId && publicConfigObject.PublicConfigId == publicConfigId)
                .Include(publicConfigObject => publicConfigObject.ConfigObject)
                .ToListAsync();

            return configData;
        }
    }
}
