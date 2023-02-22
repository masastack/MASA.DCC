// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories.App
{
    public class AppConfigObjectRepository : Repository<DccDbContext, AppConfigObject>, IAppConfigObjectRepository
    {
        public AppConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }

        public async Task<List<AppConfigObject>> GetListByEnvClusterIdAsync(int envClusterId, int appId)
        {
            var configData = await Context.Set<AppConfigObject>()
                .Where(appConfigObject => appConfigObject.EnvironmentClusterId == envClusterId && appConfigObject.AppId == appId)
                .Include(appConfigObject => appConfigObject.ConfigObject)
                .ToListAsync();

            return configData;
        }
        public async Task<List<AppConfigObject>> GetListByAppIdAsync(int appId)
        {
            var configData = await Context.Set<AppConfigObject>()
                .Include(appConfigObject => appConfigObject.ConfigObject)
                .Where(appConfigObject => appConfigObject.AppId == appId)
                .ToListAsync();

            return configData;
        }
    }
}
