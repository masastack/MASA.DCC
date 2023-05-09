// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories;

public static class RepositoryHelper
{
    public static async Task<List<T>> GetLatestReleaseOfConfigData<T>(this DccDbContext context,
        List<T> configData) where T : ConfigObjectBase
    {
        var objectIds = configData.Select(x => x.ConfigObjectId).Distinct().ToList();

        if (objectIds.Any())
        {
            var group = await context.Set<ConfigObjectRelease>()
                .Where(x => objectIds.Contains(x.ConfigObjectId)).AsNoTracking()
                .GroupBy(x => x.ConfigObjectId)
                .Select(x => x.OrderByDescending(x => x.CreationTime).FirstOrDefault()).ToListAsync();
            foreach (var config in configData)
            {
                var r = group.FirstOrDefault(x => x?.ConfigObjectId == config.ConfigObjectId);
                if (r != null)
                {
                    config.ConfigObject.ConfigObjectRelease.Clear();
                    config.ConfigObject.ConfigObjectRelease.Add(r);
                }
            }
        }

        return configData;

    }
}
