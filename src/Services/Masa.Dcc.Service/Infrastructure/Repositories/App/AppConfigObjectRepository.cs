// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Formatters;

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

        public async Task<List<(int appId, ConfigObjectRelease release)>> GetAppLatestReleaseConfigAsync(IEnumerable<int> appIds, int? envClusterId)
        {
            List<(int appId, ConfigObjectRelease release)> result = new();
            if (appIds?.Any() != true)
            {
                return result;
            }
            var bizConfigs = Context.Set<AppConfigObject>()
                .Where(x => appIds.Contains(x.AppId))
                .Select(b => new { b.ConfigObjectId, b.AppId });
            if (envClusterId.HasValue)
            {
                bizConfigs = Context.Set<AppConfigObject>()
                    .Where(x => appIds.Contains(x.AppId) && x.EnvironmentClusterId == envClusterId.Value)
                    .Select(b => new { b.ConfigObjectId, b.AppId });
            }

            var qRelease = from biz in bizConfigs
                    join r in Context.Set<ConfigObjectRelease>() on biz.ConfigObjectId equals r.ConfigObjectId into rNullable
                    from release in rNullable.DefaultIfEmpty()
                    select new { biz.AppId, release };

            var qResult = await qRelease.OrderByDescending(x => x.release.CreationTime).AsNoTracking().ToListAsync();
            foreach (var appId in appIds)
            {
                var app = qResult.FirstOrDefault(x => x.AppId == appId);
                if (app != null && app.release != null)
                {
                    result.Add((appId, app.release));
                }
            }
            return result;
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
