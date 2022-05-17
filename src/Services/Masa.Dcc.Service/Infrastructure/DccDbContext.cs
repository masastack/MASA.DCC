// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System.Reflection;

namespace Masa.Dcc.Service.Infrastructure
{
    public class DccDbContext : MasaDbContext
    {
        public DccDbContext(MasaDbContextOptions<DccDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreatingExecuting(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreatingExecuting(builder);
        }
    }
}
