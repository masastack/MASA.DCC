// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.Repositories.App;

internal class PublicConfigObjectRepository : EFBaseRepository<DccDbContext, PublicConfigObject>, IPublicConfigObjectRepository
{
    public PublicConfigObjectRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }

    public async Task<List<PublicConfigObject>> GetListByEnvClusterIdAsync(int? envClusterId, int publicConfigId,
        bool getLatestRelease)
    {
        Expression<Func<PublicConfigObject, bool>> condition = p => p.PublicConfigId == publicConfigId;
        if (envClusterId.HasValue && envClusterId != 0)
            condition = condition.And(p => p.EnvironmentClusterId == envClusterId);

        var configData = await Context.Set<PublicConfigObject>()
            .Where(condition)
            .Include(publicConfigObject => publicConfigObject.ConfigObject)
            .ToListAsync();

        if (getLatestRelease) await Context.GetLatestReleaseOfConfigData(configData);

        return configData;
    }

    public async Task<PublicConfigObject> GetByConfigObjectIdAsync(int configObjectId)
    {
        var result = await Context.Set<PublicConfigObject>()
            .FirstOrDefaultAsync(p => p.ConfigObjectId == configObjectId);

        return result ?? new(0, 0);
    }

    public async Task<List<PublicConfigObject>> GetListByPublicConfigIdAsync(int publicConfigId)
    {
        var configData = await Context.Set<PublicConfigObject>()
            .Include(pubConfig => pubConfig.ConfigObject)
            .Where(pubConfig => pubConfig.PublicConfigId == publicConfigId)
            .ToListAsync();

        return configData;
    }
}
