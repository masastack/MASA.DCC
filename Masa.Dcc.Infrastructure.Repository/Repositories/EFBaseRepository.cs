// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.Data;
using Masa.BuildingBlocks.Ddd.Domain.Entities;

namespace Masa.Dcc.Infrastructure.Repository.Repositories;

public class EFBaseRepository<TDbContext, TEntity> : Repository<TDbContext, TEntity>, IRepositoryExtensition<TEntity>
where TEntity : class, IEntity
where TDbContext : DbContext, IMasaDbContext
{
    public EFBaseRepository(TDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }

    public IQueryable<TEntity> AsQueryable()
    {
        return Context.Set<TEntity>();
    }
}
