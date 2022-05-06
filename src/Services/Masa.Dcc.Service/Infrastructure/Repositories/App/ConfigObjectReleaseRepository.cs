// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure.Repositories.App
{
    public class ConfigObjectReleaseRepository : Repository<DccDbContext, ConfigObjectRelease>, IConfigObjectReleaseRepository
    {
        public ConfigObjectReleaseRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}
