// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Dcc.Service.Admin.Domain.Label.Aggregates;

namespace Masa.Dcc.Service.Admin.Migrations
{
    public static class IHostExtensions
    {
        public static async Task SeedDataAsync(this IHost host)
        {
            await using var scope = host.Services.CreateAsyncScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<DccDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                //context.Database.Migrate();
            }

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

            context.Set<Label>().AddRange(projectTypes);
            context.SaveChanges();
        }
    }
}
