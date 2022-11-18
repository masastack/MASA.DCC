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
            var eventBus = services.GetRequiredService<IEventBus>();
            var env = services.GetRequiredService<IWebHostEnvironment>();
            var contentRootPath = env.ContentRootPath;

            await MigrateAsync(context);
            await InitDccDataAsync(context, eventBus);
            await InitPublicConfigAsync(contentRootPath, builder.Environment.EnvironmentName, eventBus);
        }

        private static async Task MigrateAsync(DccDbContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }
        }

        private static async Task InitDccDataAsync(DccDbContext context, IEventBus eventBus)
        {
            if (!context.Set<Label>().Any())
            {
                var labels = new List<UpdateLabelDto>
                {
                    new UpdateLabelDto
                    {
                        TypeName = "项目类型",
                        TypeCode = "ProjectType",
                        Description = "项目类型",
                        LabelValues = new List<LabelValueDto>
                        {
                            new LabelValueDto
                            {
                                Name ="BasicAbility",
                                Code ="BasicAbility"
                            },
                            new LabelValueDto
                            {
                                Name ="DataFactory",
                                Code ="DataFactory"
                            },
                            new LabelValueDto
                            {
                                Name ="Operator",
                                Code ="Operator"
                            },
                            new LabelValueDto
                            {
                                Name ="其他",
                                Code ="Other"
                            }
                        }
                    },
                    new UpdateLabelDto
                    {
                        TypeName = "配置对象类型",
                        TypeCode = "ConfigObjectFormat",
                        Description = "配置对象类型",
                        LabelValues = new List<LabelValueDto>
                        {
                            new LabelValueDto
                            {
                                Name ="Json",
                                Code ="Json"
                            },
                            new LabelValueDto
                            {
                                Name ="Properties",
                                Code ="Properties"
                            },
                            new LabelValueDto
                            {
                                Name ="Raw",
                                Code ="Raw"
                            },
                            new LabelValueDto
                            {
                                Name ="Xml",
                                Code ="Xml"
                            },
                            new LabelValueDto
                            {
                                Name ="Yaml",
                                Code ="Yaml"
                            }
                        }
                    }
                };

                foreach (var label in labels)
                {
                    await eventBus.PublishAsync(new AddLabelCommand(label));
                }
            }

            if (!context.Set<PublicConfig>().Any())
            {
                var publicConfig = new PublicConfig("Public", "public-$Config", "Public config");
                await context.Set<PublicConfig>().AddAsync(publicConfig);
                await context.SaveChangesAsync();
            }
        }

        public static async Task InitPublicConfigAsync(string contentRootPath, string environment,
            IEventBus eventBus)
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

            await eventBus.PublishAsync(
                new InitConfigObjectCommand(environment, "default", "public-$Config", publicConfigs, false));
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
