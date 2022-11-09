// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Migrations
{
    public static class IHostExtensions
    {
        public static async Task SeedDataAsync(this WebApplicationBuilder builder)
        {
            var services = builder.Services.BuildServiceProvider();
            var context = services.GetRequiredService<DccDbContext>();

            await MigrateAsync(context);
            await InitDccDataAsync(context);
        }

        private static async Task MigrateAsync(DccDbContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }
        }

        private static async Task InitDccDataAsync(DccDbContext context)
        {
            if (context.Set<Label>().Any())
            {
                return;
            }

            var projectTypes = new List<Label>
            {
                new Label("BasicAbility", "BasicAbility", "ProjectType", "项目类型"),
                new Label("DataFactory", "DataFactory", "ProjectType", "项目类型"),
                new Label("Operator", "Operator", "ProjectType", "项目类型"),
                new Label("Other", "其他", "ProjectType", "项目类型"),

                new Label("Json", "Json", "ConfigObjectFormat", "配置对象类型"),
                new Label("Properties", "Properties", "ConfigObjectFormat", "配置对象类型"),
                new Label("Raw", "Raw", "ConfigObjectFormat", "配置对象类型"),
                new Label("Xml", "Xml", "ConfigObjectFormat", "配置对象类型"),
                new Label("Yaml", "Yaml", "ConfigObjectFormat", "配置对象类型"),
            };

            var publicConfig = new PublicConfig("Public", "public-$Config", "Public config");

            await context.Set<Label>().AddRangeAsync(projectTypes);
            await context.Set<PublicConfig>().AddAsync(publicConfig);
            await context.SaveChangesAsync();
        }
    }
}
