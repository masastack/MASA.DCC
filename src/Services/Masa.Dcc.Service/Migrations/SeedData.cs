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
            var configObjectDomainService = services.GetRequiredService<ConfigObjectDomainService>();
            var env = services.GetRequiredService<IWebHostEnvironment>();
            var contentRootPath = env.ContentRootPath;

            await MigrateAsync(context);
            await InitDccDataAsync(context);
            await InitPublicConfigAsync(contentRootPath, builder.Environment.EnvironmentName, configObjectDomainService);
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
            if (!context.Set<Label>().Any())
            {
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

            if (!context.Set<PublicConfig>().Any())
            {
                var publicConfig = new PublicConfig("Public", "public-$Config", "Public config");
                await context.Set<PublicConfig>().AddAsync(publicConfig);
                await context.SaveChangesAsync();
            }
        }

        public static async Task InitPublicConfigAsync(string contentRootPath, string environment,
            ConfigObjectDomainService configObjectDomainService)
        {
            var publicConfigs = new Dictionary<string, string>
            {
                { "$public.RedisConfig",GetRedisConfig(contentRootPath,environment) },
                { "$public.AppSettings",GetAppSettings(contentRootPath,environment) },
                { "$public.Oidc",GetOidc(contentRootPath,environment) },
                { "$public.Oss",GetOss(contentRootPath,environment) },
                { "$public.ES.UserAutoComplete",GetESUserAutoComplete(contentRootPath,environment) },
                { "$public.AliyunPhoneNumberLogin",GetAliyunPhoneNumberLogin(contentRootPath,environment) },
                { "$public.Email",GetEmail(contentRootPath,environment) },
                { "$public.Sms",GetSms(contentRootPath,environment) },
                { "$public.Clients",GetClient(contentRootPath,environment) }
            };

            await configObjectDomainService.InitConfigObjectAsync(environment,
                "default",
                "public-$Config",
                publicConfigs,
                false);
        }


        private static string GetRedisConfig(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.RedisConfig.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetAppSettings(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.AppSettings.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetOidc(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.Oidc.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetOss(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.Oss.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetClient(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.Clients.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetESUserAutoComplete(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.ES.UserAutoComplete.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetAliyunPhoneNumberLogin(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.AliyunPhoneNumberLogin.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetEmail(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.Email.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetSms(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.Sms.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string CombineFilePath(string contentRootPath, string fileName, string environment)
        {
            var extension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var environmentFileName = $"{fileNameWithoutExtension}.{environment}{extension}";
            var environmentFilePath = Path.Combine(contentRootPath, "Setup", environmentFileName);
            if (File.Exists(environmentFilePath))
            {
                return environmentFilePath;
            }
            return Path.Combine(contentRootPath, "Setup", fileName);
        }
    }
}
