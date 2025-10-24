// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.Repository.Repositories.App;

internal class ConfigObjectReleaseRepository : EFBaseRepository<DccDbContext, ConfigObjectRelease>, IConfigObjectReleaseRepository
{
    public ConfigObjectReleaseRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
}
