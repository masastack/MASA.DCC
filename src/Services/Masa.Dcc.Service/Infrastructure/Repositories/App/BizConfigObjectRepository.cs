// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Dcc.Contracts.Admin;

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
                .Where(bizConfigObject => bizConfigObject.EnvironmentClusterId == envClusterId &&
                                          bizConfigObject.BizConfigId == bizConfigId)
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

        public async Task<List<(int projectId, ConfigObjectRelease release)>>
            GetProjectLatestReleaseConfigAsync(
                List<BuildingBlocks.StackSdks.Pm.Model.ProjectModel> projects, int? envClusterId = null)
        {

            List<(int projectId, ConfigObjectRelease release)> result = new();
            if (projects?.Any() != true)
            {
                return result;
            }
            var identities = projects.Select(x => x.Identity + DccConst.BizConfigSuffix);
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


            var qBizConfigs = Context.Set<BizConfigObject>()
                   .Include(x => x.BizConfig)
                   .Where(x => identities.Contains(x.BizConfig.Identity))
                   .Select(b => new { b.BizConfig.Identity, b.ConfigObjectId });
            if (envClusterId.HasValue)
            {
                qBizConfigs = Context.Set<BizConfigObject>().AsNoTracking()
                    .Include(x => x.BizConfig)
                    .Where(x => identities.Contains(x.BizConfig.Identity) &&
                                x.EnvironmentClusterId == envClusterId.Value)
                    .Select(b => new { b.BizConfig.Identity, b.ConfigObjectId });
            }

            var qReleases = from biz in qBizConfigs
                            join r in Context.Set<ConfigObjectRelease>() on biz.ConfigObjectId equals r.ConfigObjectId into rNullable
                            from release in rNullable.DefaultIfEmpty()
                            select new { biz.Identity, release };

            var qResult = await qReleases.OrderByDescending(x => x.release.CreationTime).AsNoTracking().ToListAsync();

            foreach (var project in projects)
            {
                var m = qResult.FirstOrDefault(x => x.Identity == project.Identity + DccConst.BizConfigSuffix);
                if (m != null && m.release != null)
                {
                    result.Add((project.Id, m.release));
                }
            }

            return result;
        }
    }
}
