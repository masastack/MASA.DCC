// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.Repositories.App;

internal class AppConfigObjectRepository : EFBaseRepository<DccDbContext, AppConfigObject>, IAppConfigObjectRepository
{
    public AppConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }

    public async Task<List<AppConfigObject>> GetListByEnvClusterIdAsync(int envClusterId, int appId,
        bool getLatestRelease)
    {
        var configData = await Context.Set<AppConfigObject>()
            .Where(appConfigObject => appConfigObject.EnvironmentClusterId == envClusterId && appConfigObject.AppId == appId)
            .Include(appConfigObject => appConfigObject.ConfigObject)
            .AsNoTracking().ToListAsync();

        if (getLatestRelease) await Context.GetLatestReleaseOfConfigData(configData);

        return configData;
    }

    public async Task<List<(int AppId, ConfigObjectRelease Release)>> GetAppLatestReleaseConfigAsync(IEnumerable<int> appIds, int? envClusterId)
    {
        List<(int AppId, ConfigObjectRelease Release)> result = new();
        if (appIds?.Any() != true)
        {
            return result;
        }
        Expression<Func<AppConfigObject, bool>> condition = appConfig => appIds.Contains(appConfig.AppId);
        if (envClusterId.HasValue)
        {
            condition.And(x => x.EnvironmentClusterId == envClusterId.Value);
        }
        var qConfigs = Context.Set<AppConfigObject>()
            .Where(condition)
            .Select(b => new { b.ConfigObjectId, b.AppId });

        var qRelease = from biz in qConfigs
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


    public async Task<AppConfigObject> GetbyConfigObjectIdAsync(int configObjectId)
    {
        var result = await Context.Set<AppConfigObject>()
            .FirstOrDefaultAsync(p => p.ConfigObjectId == configObjectId);

        return result ?? new(0, 0);
    }
}
