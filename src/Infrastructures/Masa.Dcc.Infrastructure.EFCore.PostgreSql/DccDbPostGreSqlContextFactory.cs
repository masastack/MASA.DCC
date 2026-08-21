// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure;

internal class DccDbPostGreSqlContextFactory : IDesignTimeDbContextFactory<DccDbContext>
{
    const string ConnectionStringKey = "MasaDccPgsqlStaging";

    public DccDbContext CreateDbContext(string[] args)
    {
        DccDbContext.RegistAssembly(typeof(DccDbPostGreSqlContextFactory).Assembly);
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(DccDbPostGreSqlContextFactory).Assembly, optional: true)
            .Build();

        var connectionString = configuration[ConnectionStringKey]!;
        var optionsBuilder = new MasaDbContextOptionsBuilder<DccDbContext>();
        optionsBuilder.DbContextOptionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("Masa.Dcc.Infrastructure.EFCore.PostgreSql"));

        return new DccDbContext(optionsBuilder.MasaOptions);
    }
}
