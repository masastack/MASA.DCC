﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Infrastructure.Repositories
{
    public class PublicConfigRepository : Repository<DccDbContext, PublicConfig>, IPublicConfigRepository
    {
        public PublicConfigRepository(DccDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}