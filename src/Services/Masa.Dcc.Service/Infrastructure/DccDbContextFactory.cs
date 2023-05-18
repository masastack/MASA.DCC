// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure
{
    public class DccDbContextFactory : IDesignTimeDbContextFactory<DccDbContext>
    {
        public DccDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new MasaDbContextOptionsBuilder<DccDbContext>();
            var configurationBuilder = new ConfigurationBuilder();
            var configuration = configurationBuilder
                .AddJsonFile("appsettings.Development.json")
                .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new DccDbContext(optionsBuilder.MasaOptions);
        }
    }
}
