﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories
{
    public class PublicConfigObjectRepository : Repository<DccDbContext, PublicConfigObject>, IPublicConfigObjectRepository
    {
        public PublicConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }

        public async Task<List<PublicConfigObject>> GetListByEnvClusterIdAsync(int? envClusterId, int publicConfigId)
        {
            Expression<Func<PublicConfigObject, bool>> condition = p => p.PublicConfigId == publicConfigId;
            if (envClusterId.HasValue && envClusterId != 0)
                condition = condition.And(p => p.EnvironmentClusterId == envClusterId);

            var configData = await Context.Set<PublicConfigObject>()
                .Where(condition)
                .Include(publicConfigObject => publicConfigObject.ConfigObject)
                .ToListAsync();

            return configData;
        }

        public async Task<PublicConfigObject> GetByConfigObjectIdAsync(int configObjectId)
        {
            var result = await Context.Set<PublicConfigObject>()
                .FirstOrDefaultAsync(p => p.ConfigObjectId == configObjectId);

            return result ?? new(0, 0);
        }
    }
}
