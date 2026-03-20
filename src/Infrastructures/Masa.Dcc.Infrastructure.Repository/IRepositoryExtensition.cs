// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.Ddd.Domain.Entities;

namespace Masa.Dcc.Infrastructure.Repository.Label;

public interface IRepositoryExtensition<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
{
    IQueryable<TEntity> AsQueryable();
}
