// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.EFCore.SqlServer;

internal class DccDbSqlServerContextFactory : IDesignTimeDbContextFactory<DccDbContext>
{
    public DccDbContext CreateDbContext(string[] args)
    {
        DccDbContext.RegistAssembly(typeof(DccDbSqlServerContextFactory).Assembly);
        var optionsBuilder = new MasaDbContextOptionsBuilder<DccDbContext>();
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder
            .AddJsonFile("migration-sqlserver.json")
            .Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), mbox => mbox.MigrationsAssembly("Masa.Dcc.Infrastructure.EFCore.SqlServer"));

        return new DccDbContext(optionsBuilder.MasaOptions);
    }
}
