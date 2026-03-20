// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.Repositories.App;

internal class BizConfigObjectRepository : EFBaseRepository<DccDbContext, BizConfigObject>, IBizConfigObjectRepository
{
    public BizConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }

    public async Task<List<BizConfigObject>> GetListByEnvClusterIdAsync(int envClusterId, int bizConfigId,
        bool getLatestRelease)
    {
        var configData = await Context.Set<BizConfigObject>()
            .Where(bizConfigObject => bizConfigObject.EnvironmentClusterId == envClusterId &&
                                      bizConfigObject.BizConfigId == bizConfigId)
            .Include(bizConfigObject => bizConfigObject.ConfigObject)
            .ToListAsync();

        if (getLatestRelease) await Context.GetLatestReleaseOfConfigData(configData);

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

    public async Task<List<(ConfigObjectRelease Release, int ProjectId)>>
        GetProjectLatestReleaseConfigAsync(
            List<ProjectModel> projects, int? envClusterId = null)
    {

        List<(ConfigObjectRelease Release, int ProjectId)> result = new();
        if (projects?.Any() != true)
        {
            return result;
        }

        var identities = projects.Select(x => (x.Identity + DccConst.BizConfigSuffix).ToLower());
        //var bizConfigs = await qBizConfigs.ToListAsync();

        //var qReleases = Context.Set<ConfigObjectRelease>().GroupBy(r => r.ConfigObjectId)
        //    .Select(x => x.OrderByDescending(x => x.CreationTime).FirstOrDefault());
        ////select new { release = g.OrderByDescending(x => x.CreationTime).FirstOrDefault(), biz.BizConfig }
        //;
        //var test = await qReleases.ToListAsync();
        //var q = from biz in qBizConfigs
        //        join r in qReleases on biz.ConfigObjectId equals r.ConfigObjectId into rNullable
        //    from release in rNullable.DefaultIfEmpty()
        //       // where identities.Contains(biz.BizConfig.Identity)
        //    select new { biz.BizConfig.Identity };
        // TODO Group by is not support to join,it support in EFCore 7.0 https://github.com/dotnet/efcore/issues/24106

        Expression<Func<BizConfigObject, bool>> condition = x => identities.Contains(x.BizConfig.Identity);
        if (envClusterId.HasValue)
        {
            condition.And(x => x.EnvironmentClusterId == envClusterId.Value);
        }
        var qConfigs = Context.Set<BizConfigObject>()
               .Include(x => x.BizConfig)
               .Where(condition)
               .Select(b => new { b.BizConfig.Identity, b.ConfigObjectId });

        var qReleases = from biz in qConfigs
                        join r in Context.Set<ConfigObjectRelease>() on biz.ConfigObjectId equals r.ConfigObjectId into rNullable
                        from release in rNullable.DefaultIfEmpty()
                        select new { biz.Identity, release };

        var qResult = await qReleases.OrderByDescending(x => x.release.CreationTime).AsNoTracking().ToListAsync();

        foreach (var project in projects)
        {
            var m = qResult.FirstOrDefault(x => string.Equals(x.Identity,
                project.Identity + DccConst.BizConfigSuffix, StringComparison.InvariantCultureIgnoreCase));
            if (m != null && m.release != null)
            {
                result.Add((m.release, project.Id));
            }
        }

        return result;
    }

    public async Task<BizConfigObject> GetByConfigObjectIdAsync(int configObjectId)
    {
        var result = await Context.Set<BizConfigObject>()
            .FirstOrDefaultAsync(p => p.ConfigObjectId == configObjectId);

        return result ?? new(0, 0);
    }
}
