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
    }
}
