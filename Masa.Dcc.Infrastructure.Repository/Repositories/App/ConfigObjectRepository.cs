// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.Repositories.App;

internal class ConfigObjectRepository : Repository<DccDbContext, ConfigObject>, IConfigObjectRepository
{
    public ConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }

    public async Task<ConfigObject> GetConfigObjectWithReleaseHistoriesAsync(int id)
    {
        var configObject = await Context.Set<ConfigObject>()
            .Where(configObject => configObject.Id == id)
            .Include(configObject => configObject.ConfigObjectRelease)
            .FirstOrDefaultAsync();

        return configObject ?? throw new Exception("Config object does not exist");
    }

    public async Task<List<ConfigObject>> GetRelationConfigObjectWithReleaseHistoriesAsync(int id)
    {
        var configObjects = await Context.Set<ConfigObject>()
            .Where(configObject => configObject.RelationConfigObjectId == id)
            .Include(configObject => configObject.ConfigObjectRelease)
            .ToListAsync();

        return configObjects;
    }

    public async Task<List<(ConfigObject ConfigObject, int ObjectId, int EnvironmentClusterId)>> GetNewestConfigObjectReleaseWithAppInfo()
    {
        var configObjectReleaseQuery = Context.Set<ConfigObject>()
            .Join(
                Context.Set<ConfigObjectRelease>(),
                configObject => configObject.Id,
                configObjectRelease => configObjectRelease.ConfigObjectId,
                (configObject, configObjectRelease) => new
                {
                    ConfigObject = configObject,
                    ConfigObjectReleaseId = configObjectRelease.Id,
                }
            );

        var result = await configObjectReleaseQuery
            .Join(
                Context.Set<AppConfigObject>(),
                configObjectGroup => configObjectGroup.ConfigObject.Id,
                appConfigObject => appConfigObject.ConfigObjectId,
                (configObjectGroup, appConfigObject) => new
                {
                    configObjectGroup.ConfigObject,
                    configObjectGroup.ConfigObjectReleaseId,
                    configObjectGroup.ConfigObject.Type,
                    appConfigObject.AppId,
                    appConfigObject.EnvironmentClusterId
                }
            )
            .GroupBy(config => config.ConfigObject.Id)
            .Select(config => config.OrderByDescending(c => c.ConfigObjectReleaseId).First())
            .ToListAsync();

        var publicResult = await configObjectReleaseQuery
            .Join(
                Context.Set<PublicConfigObject>(),
                configObjectGroup => configObjectGroup.ConfigObject.Id,
                publicConfigObject => publicConfigObject.ConfigObjectId,
                (configObjectGroup, publicConfigObject) => new
                {
                    configObjectGroup.ConfigObject,
                    configObjectGroup.ConfigObjectReleaseId,
                    configObjectGroup.ConfigObject.Type,
                    publicConfigObject.PublicConfigId,
                    publicConfigObject.EnvironmentClusterId
                }
            )
            .GroupBy(config => config.ConfigObject.Id)
            .Select(config => config.OrderByDescending(c => c.ConfigObjectReleaseId).First())
            .ToListAsync();

        return result
            .Select(config =>
            new ValueTuple<ConfigObject, int, int>(config.ConfigObject, config.AppId, config.EnvironmentClusterId))
            .Union(publicResult.Select(config =>
            new ValueTuple<ConfigObject, int, int>(config.ConfigObject, config.PublicConfigId, config.EnvironmentClusterId)))
            .ToList();
    }

    public async Task<List<ConfigObject>> GetConfigObjectsByNameAsync(string Name)
    {
        var names = await Context.Set<ConfigObject>().Select(configObject => new { configObject.Id, configObject.Name }).ToListAsync();
        if (names == null || names.Count == 0)
            return [];
        Name = Name.ToLower().Trim();
        var matches = names.Where(item => Name.Equals(item.Name.ToLower())).ToList();
        if (matches == null || matches.Count == 0)
            return [];

        var ids = matches.Select(item => item.Id).ToList();
        return await Context.Set<ConfigObject>().Include(configObject => configObject.ConfigObjectRelease).Where(item => ids.Contains(item.Id)).ToListAsync();
    }

    public async Task<int> GetIdAsync(string Name, ConfigObjectType Type)
    {
        var names = await Context.Set<ConfigObject>().Where(item => item.Type == Type).Select(configObject => new { configObject.Id, configObject.Name }).ToListAsync();
        if (names == null || names.Count == 0)
            return default;
        Name = Name.ToLower().Trim();
        var matches = names.Where(item => Name.Equals(item.Name.ToLower())).ToList();
        if (matches == null || matches.Count == 0)
            return default;
        return matches[0].Id;
    }
}
