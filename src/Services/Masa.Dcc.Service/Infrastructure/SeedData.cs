﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Service.Admin.Infrastructure
{
    public static class IHostExtensions
    {
        public static async Task SeedDataAsync(this WebApplicationBuilder builder)
        {
            var services = builder.Services.BuildServiceProvider().CreateScope().ServiceProvider;
            var context = services.GetRequiredService<DccDbContext>();
            var labelDomainService = services.GetRequiredService<LabelDomainService>();
            var configObjectDomainService = services.GetRequiredService<ConfigObjectDomainService>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();
            var env = services.GetRequiredService<IWebHostEnvironment>();
            var contentRootPath = env.ContentRootPath;
            var masaConfig = builder.Services.GetMasaStackConfig();
            var pmClient = services.GetRequiredService<IPmClient>();

            string system = "system";
            var userSetter = services.GetService<IUserSetter>();
            var auditUser = new IdentityUser() { Id = masaConfig.GetDefaultUserId().ToString(), UserName = system };
            var userSetterHandle = userSetter!.Change(auditUser);

            await MigrateAsync(context);

            unitOfWork.UseTransaction = false;

            await InitDccDataAsync(context, labelDomainService);
            await InitPublicConfigAsync(context, masaConfig, contentRootPath, pmClient, configObjectDomainService);

            userSetterHandle.Dispose();
        }

        private static async Task MigrateAsync(DccDbContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }
        }

        private static async Task InitDccDataAsync(DccDbContext context, LabelDomainService labelDomainService)
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
                                Name ="JSON",
                                Code ="JSON"
                            },
                            new LabelValueDto
                            {
                                Name ="Properties",
                                Code ="Properties"
                            },
                            new LabelValueDto
                            {
                                Name ="RAW",
                                Code ="RAW"
                            },
                            new LabelValueDto
                            {
                                Name ="XML",
                                Code ="XML"
                            },
                            new LabelValueDto
                            {
                                Name ="YAML",
                                Code ="YAML"
                            }
                        }
                    }
                };

                foreach (var label in labels)
                {
                    await labelDomainService.AddLabelAsync(label);
                }
            }

            if (!context.Set<PublicConfig>().Any())
            {
                var publicConfig = new PublicConfig("Public", "public-$Config", "Public config");
                await context.Set<PublicConfig>().AddAsync(publicConfig);
            }

            await context.SaveChangesAsync();
        }

        public static async Task InitPublicConfigAsync(
            DccDbContext context,
            IMasaStackConfig masaConfig,
            string contentRootPath,
            IPmClient pmClient,
            ConfigObjectDomainService configObjectDomainService)
        {
            //TODO:InitConfigObjectAsync method repeat call pmClient.EnvironmentService.GetListAsync,should be optimized
            var environments = await pmClient.EnvironmentService.GetListAsync();
            foreach (var environment in environments)
            {
                var publicConfigs = new Dictionary<string, string>
                {
                    { "$public.AliyunPhoneNumberLogin",GetAliyunPhoneNumberLogin(contentRootPath,environment.Name) },
                    { "$public.Email",GetEmail(contentRootPath,environment.Name) },
                    { "$public.Sms",GetSms(contentRootPath,environment.Name) },
                    { "$public.Cdn",GetCdn(contentRootPath,environment.Name) },
                    { "$public.WhiteListOptions",GetWhiteListOptions(contentRootPath,environment.Name) },
                    { "$public.i18n.en-us",GetI8nUs(contentRootPath,environment.Name) },
                    { "$public.i18n.zh-cn",GetI8nCn(contentRootPath,environment.Name) }
                };

                await configObjectDomainService.InitConfigObjectAsync(environment.Name, masaConfig.Cluster, "public-$Config", publicConfigs, false);

                var encryptionPublicConfigs = new Dictionary<string, string>
                {
                    { "$public.Oss",GetOss(contentRootPath, environment.Name) }
                };
                await configObjectDomainService.InitConfigObjectAsync(environment.Name, masaConfig.Cluster, "public-$Config", encryptionPublicConfigs, true);

                await context.SaveChangesAsync();
            }
        }

        private static string GetI8nUs(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.i18n.en-us.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetI8nCn(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.i18n.zh-cn.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetCdn(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.Cdn.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetWhiteListOptions(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.WhiteListOptions.json", environment);
            return File.ReadAllText(filePath);
        }

        private static string GetOss(string contentRootPath, string environment)
        {
            var filePath = CombineFilePath(contentRootPath, "$public.Oss.json", environment);
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
