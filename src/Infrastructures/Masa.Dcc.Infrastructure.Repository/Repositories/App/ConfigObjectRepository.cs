// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.Repositories.App;

internal class ConfigObjectRepository : EFBaseRepository<DccDbContext, ConfigObject>, IConfigObjectRepository
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

    public async Task<(long Total, List<ConfigObject> Items)> GetPaginatedListAsync(ConfigObjectSearchCondition condition, int page, int pageSize)
    {
        var query = Context.Set<ConfigObject>().AsNoTracking().AsQueryable();

        if (condition.EnvironmentClusterIds != null)
        {
            var envClusterIds = condition.EnvironmentClusterIds;
            query = query.Where(x =>
                (x.PublicConfigObject != null && envClusterIds.Contains(x.PublicConfigObject.EnvironmentClusterId)) ||
                (x.BizConfigObject != null && envClusterIds.Contains(x.BizConfigObject.EnvironmentClusterId)) ||
                (x.AppConfigObject != null && envClusterIds.Contains(x.AppConfigObject.EnvironmentClusterId)));
        }

        if (condition.AppIds != null || condition.BizIdentity != null)
        {
            var appIds = condition.AppIds ?? new List<int>();
            var bizIdentity = condition.BizIdentity;
            query = bizIdentity == null
                ? query.Where(x => x.AppConfigObject != null && appIds.Contains(x.AppConfigObject.AppId))
                : query.Where(x =>
                    (x.AppConfigObject != null && appIds.Contains(x.AppConfigObject.AppId)) ||
                    (x.BizConfigObject != null && x.BizConfigObject.BizConfig.Identity.ToLower() == bizIdentity));
        }

        if (!string.IsNullOrWhiteSpace(condition.Name))
        {
            var name = condition.Name.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(condition.Content))
        {
            var keyword = condition.Content.Trim().ToLower();

            // Encrypted content is stored as cipher text, so it has to be decrypted and matched in memory.
            var encryptedIds = new List<int>();
            if (condition.ContentDecryptor != null)
            {
                var encrypted = await query.Where(x => x.Encryption).Select(x => new { x.Id, x.Content }).ToListAsync();
                foreach (var item in encrypted)
                {
                    try
                    {
                        if (condition.ContentDecryptor(item.Content).Contains(keyword, StringComparison.OrdinalIgnoreCase))
                            encryptedIds.Add(item.Id);
                    }
                    catch
                    {
                        // Content can not be decrypted (e.g. empty placeholder), treat as not matched.
                    }
                }
            }

            query = query.Where(x =>
                (!x.Encryption && x.Content.ToLower().Contains(keyword)) ||
                (x.Encryption && encryptedIds.Contains(x.Id)));
        }

        var total = await query.LongCountAsync();
        var items = await query
            .Include(x => x.PublicConfigObject).ThenInclude(x => x.PublicConfig)
            .Include(x => x.BizConfigObject).ThenInclude(x => x.BizConfig)
            .Include(x => x.AppConfigObject)
            .OrderByDescending(x => x.ModificationTime)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (total, items);
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
