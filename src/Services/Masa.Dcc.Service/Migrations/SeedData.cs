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

            var env = services.GetRequiredService<IWebHostEnvironment>();
            var environment = builder.Environment.EnvironmentName;
            var configObjectDomainService = services.GetRequiredService<ConfigObjectDomainService>();

            await InitDccLabelDataAsync(context);
            await InitPmConfigAsync(env.ContentRootPath, environment, configObjectDomainService);
        }

        public static async Task InitDccLabelDataAsync(DccDbContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
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

            await context.Set<Label>().AddRangeAsync(projectTypes);
            await context.SaveChangesAsync();
        }

        public static async Task InitPmConfigAsync(string contentRootPath, string environment,
            ConfigObjectDomainService configObjectDomainService)
        {
            var pmConfigs = GetPMConfig(contentRootPath, environment);
            foreach (var pmConfig in pmConfigs)
            {
                await configObjectDomainService.InitConfigObjectAsync(environment,
                    "default",
                    "masa-pm-service-admin",
                    new Dictionary<string, string>
                    {
                        {"Appsettings",pmConfig }
                    },
                    false);
            }
        }

        private static List<string> GetPMConfig(string contentRootPath, string environment)
        {
            List<string> contents = new List<string>();
            var filePath = CombineFilePath(contentRootPath, "Config/PM", "appsettings.json", environment);
            var appsettingsContent = File.ReadAllText(filePath);

            contents.Add(appsettingsContent);
            return contents;
        }

        private static string CombineFilePath(string contentRootPath, string filePath, string fileName, string environment)
        {
            string extension = Path.GetExtension(fileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var environmentFileName = $"{fileNameWithoutExtension}.{environment}{extension}";
            var environmentFilePath = Path.Combine(contentRootPath, filePath, environmentFileName);
            if (File.Exists(environmentFilePath))
            {
                return environmentFilePath;
            }
            return Path.Combine(contentRootPath, filePath, fileName);
        }
    }
}
