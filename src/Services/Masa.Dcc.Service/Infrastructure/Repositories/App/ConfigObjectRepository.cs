// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories
{
    public class ConfigObjectRepository : Repository<DccDbContext, ConfigObject>, IConfigObjectRepository
    {
        public ConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }

        public async Task<ConfigObject> GetConfigObjectWithReleaseHistoriesAsync(int Id)
        {
            var configObject = await Context.Set<ConfigObject>()
                .Where(configObject => configObject.Id == Id)
                .Include(configObject => configObject.ConfigObjectRelease)
                .FirstOrDefaultAsync();

            return configObject ?? throw new Exception("Config object does not exist");
        }

        public async Task<List<ConfigObject>> GetRelationConfigObjectWithReleaseHistoriesAsync(int Id)
        {
            var configObjects = await Context.Set<ConfigObject>()
                .Where(configObject => configObject.RelationConfigObjectId == Id)
                .Include(configObject => configObject.ConfigObjectRelease)
                .ToListAsync();

            return configObjects;
        }

        public async Task<List<(ConfigObject ConfigObject, int AppId, int EnvironmentClusterId)>> GetNewestConfigObjectReleaseWithAppInfo()
        {
            var result = await Context.Set<ConfigObject>()
                .Join(
                    Context.Set<ConfigObjectRelease>(),
                    configObject => configObject.Id,
                    configObjectRelease => configObjectRelease.ConfigObjectId,
                    (configObject, configObjectRelease) => new
                    {
                        ConfigObject = configObject,
                        ConfigObjectReleaseId = configObjectRelease.Id,
                    }
                )
                .Join(
                    Context.Set<AppConfigObject>(),
                    configObjectGroup => configObjectGroup.ConfigObject.Id,
                    appConfigObject => appConfigObject.ConfigObjectId,
                    (configObjectGroup, appConfigObject) => new
                    {
                        configObjectGroup.ConfigObject,
                        configObjectGroup.ConfigObjectReleaseId,
                        appConfigObject.AppId,
                        appConfigObject.EnvironmentClusterId
                    }
                )
                .GroupBy(config => config.ConfigObject.Id)
                .Select(config => config.OrderByDescending(c => c.ConfigObjectReleaseId).First())
                .ToListAsync();

            return result
                .Select(config =>
                new ValueTuple<ConfigObject, int, int>(config.ConfigObject, config.AppId, config.EnvironmentClusterId))
                .ToList();
        }
    }
}
