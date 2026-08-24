// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Infrastructure.EFCore.SqlServer;

internal class DccDbSqlServerContextFactory : IDesignTimeDbContextFactory<DccDbContext>
{
    const string ConnectionStringKey = "MasaDccMssqlStaging";

    public DccDbContext CreateDbContext(string[] args)
    {
        DccDbContext.RegistAssembly(typeof(DccDbSqlServerContextFactory).Assembly);
        var configuration = new ConfigurationBuilder()
             .AddUserSecrets(typeof(DccDbSqlServerContextFactory).Assembly, optional: true)
             .Build();

        var connectionString = configuration[ConnectionStringKey];
        var optionsBuilder = new MasaDbContextOptionsBuilder<DccDbContext>();
        optionsBuilder.UseSqlServer(connectionString!, mbox => mbox.MigrationsAssembly("Masa.Dcc.Infrastructure.EFCore.SqlServer"));

        return new DccDbContext(optionsBuilder.MasaOptions);
    }
}
