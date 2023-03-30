// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Infrastructure
{
    public class DccDbContext : MasaDbContext<DccDbContext>
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
