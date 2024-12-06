// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure;

internal class DccDbPostGreSqlContextFactory : IDesignTimeDbContextFactory<DccDbContext>
{
    public DccDbContext CreateDbContext(string[] args)
    {
        DccDbContext.RegistAssembly(typeof(DccDbPostGreSqlContextFactory).Assembly);
        var optionsBuilder = new MasaDbContextOptionsBuilder<DccDbContext>();
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder
            .AddJsonFile("appsettings.json")
            .Build();
        optionsBuilder.DbContextOptionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Masa.Dcc.Infrastructure.EFCore.PostgreSql"));

        return new DccDbContext(optionsBuilder.MasaOptions);
    }
}
