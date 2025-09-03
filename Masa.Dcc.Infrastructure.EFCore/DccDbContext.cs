// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.EFCore;

public class DccDbContext : MasaDbContext<DccDbContext>
{
    public DccDbContext(MasaDbContextOptions<DccDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
    }

    public static Assembly Assembly { get; private set; } = typeof(DccDbContext).Assembly;

    public static void RegistAssembly(Assembly assembly)
    {
        Assembly = assembly;
    }

    protected override void OnModelCreatingExecuting(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly);        
        base.OnModelCreatingExecuting(builder);
    }
}
