// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.Repositories.App;

internal class AppPinRepository : EFBaseRepository<DccDbContext, AppPin>, IAppPinRepository
{
    public AppPinRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    { }

    public async Task<List<AppPin>> GetListAsync(List<int> appIds)
    {
        var result = await Context.Set<AppPin>()
            .IgnoreQueryFilters()
            .Where(app => appIds.Contains(app.AppId))
            .GroupBy(app => app.AppId)
            .Select(app => app.OrderByDescending(a => a.Id).First())
            .ToListAsync();

        return result;
    }
}
