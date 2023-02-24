// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories.App
{
    public class BizConfigObjectRepository : Repository<DccDbContext, BizConfigObject>, IBizConfigObjectRepository
    {
        public BizConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }

        public async Task<List<BizConfigObject>> GetListByEnvClusterIdAsync(int envClusterId, int bizConfigId)
        {
            var configData = await Context.Set<BizConfigObject>()
                .Where(bizConfigObject => bizConfigObject.EnvironmentClusterId == envClusterId && bizConfigObject.BizConfigId == bizConfigId)
                .Include(bizConfigObject => bizConfigObject.ConfigObject)
                .ToListAsync();

            return configData;
        }

        public async Task<List<BizConfigObject>> GetListByBizConfigIdAsync(int bizConfigId)
        {
            var bizConfigObjects = await Context.Set<BizConfigObject>()
                .Include(bizConfigObject => bizConfigObject.ConfigObject)
                .Where(bizConfigObject => bizConfigObject.BizConfigId == bizConfigId)
                .ToListAsync();

            return bizConfigObjects;
        }
    }
}
