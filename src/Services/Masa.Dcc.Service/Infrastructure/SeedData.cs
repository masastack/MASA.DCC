// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System.Text.Encodings.Web;
using Npgsql.Internal;

namespace Masa.Dcc.Service.Admin.Infrastructure;

public static class IHostExtensions
{
    static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };


    public static async Task SeedDataAsync(this WebApplicationBuilder builder)
    {
        var services = builder.Services.BuildServiceProvider().CreateScope().ServiceProvider;
        var context = services.GetRequiredService<DccDbContext>();
        var labelDomainService = services.GetRequiredService<LabelDomainService>();
        var configObjectDomainService = services.GetRequiredService<InitConfigObjectDomainService>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var contentRootPath = env.ContentRootPath;
        var masaConfig = builder.Services.GetMasaStackConfig();

        string system = "system";
        var userSetter = services.GetService<IUserSetter>();
        var auditUser = new IdentityUser() { Id = masaConfig.GetDefaultUserId().ToString(), UserName = system };
        var userSetterHandle = userSetter!.Change(auditUser);

        await MigrateAsync(context);

        unitOfWork.UseTransaction = false;

        await InitDccDataAsync(context, labelDomainService);
        await InitPublicConfigAsync(services, context, masaConfig, contentRootPath, configObjectDomainService);

        userSetterHandle.Dispose();
    }

    private static async Task MigrateAsync(DccDbContext context)
    {
        var pendings = await context.Database.GetPendingMigrationsAsync();
        if (pendings != null && pendings.Any())
            await context.Database.MigrateAsync();
    }

    private static async Task InitDccDataAsync(DccDbContext context, LabelDomainService labelDomainService)
    {
        if (!await context.Set<Label>().AnyAsync())
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

        if (!await context.Set<PublicConfig>().AnyAsync())
        {
            var publicConfig = new PublicConfig("Public", DccConst.DEFAULT_PUBLIC_ID, "Public config");
            await context.Set<PublicConfig>().AddAsync(publicConfig);
        }

        await context.SaveChangesAsync();
    }

    public static async Task InitPublicConfigAsync(
        IServiceProvider service,
        DccDbContext context,
        IMasaStackConfig masaConfig,
        string contentRootPath,
        InitConfigObjectDomainService configObjectDomainService)
    {
        var pmClient = service.GetRequiredService<IPmClient>();
        var envClusters = await pmClient.ClusterService.GetEnvironmentClustersAsync();
        if (envClusters == null || envClusters.Count == 0)
            throw new UserFriendlyException("pm环境数据未初始化");
        foreach (var environment in envClusters)
        {
            await InitEnvConfigObjects(environment.Id, environment.EnvironmentName, service, context, masaConfig, contentRootPath, configObjectDomainService);
        }
    }

    private static async Task InitEnvConfigObjects(int envClusterId, string environment, IServiceProvider service, DccDbContext context,
        IMasaStackConfig masaConfig,
        string contentRootPath,
        InitConfigObjectDomainService configObjectDomainService)
    {
        if (await context.Set<ConfigObject>().AnyAsync())
        {
            return;
        }

        var publicConfigs = new Dictionary<string, string>
            {
                { DccConst.DEFAULT_CONFIG_NAME,JsonSerializer.Serialize(GetConfigMap(service),_serializerOptions)},
                { "$public.AliyunPhoneNumberLogin",GetAliyunPhoneNumberLogin(contentRootPath,environment) },
                { "$public.Email",GetEmail(contentRootPath,environment) },
                { "$public.Sms",GetSms(contentRootPath,environment) },
                { "$public.Cdn",GetCdn(contentRootPath,environment) },
                { "$public.WhiteListOptions",GetWhiteListOptions(contentRootPath,environment) },
                { "$public.i18n.en-us",GetI8nUs(contentRootPath,environment) },
                { "$public.i18n.zh-cn",GetI8nCn(contentRootPath,environment) }
            };
        await configObjectDomainService.InitConfigObjectAsync(environment, masaConfig.Cluster, envClusterId, DccConst.DEFAULT_PUBLIC_ID, publicConfigs, ConfigObjectType.Public, false);

        var encryptionPublicConfigs = new Dictionary<string, string>
            {
                { "$public.Oss",GetOss(contentRootPath, environment) }
            };
        await configObjectDomainService.InitConfigObjectAsync(environment, masaConfig.Cluster, envClusterId, DccConst.DEFAULT_PUBLIC_ID, encryptionPublicConfigs, ConfigObjectType.Public, true);

        await context.SaveChangesAsync();
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

    private static Dictionary<string, string> GetConfigMap(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        string environment = configuration.GetValue<string>(MasaStackConfigConstant.ENVIRONMENT)!;
        environment = string.IsNullOrWhiteSpace(environment) ? configuration["ASPNETCORE_ENVIRONMENT"]! : environment;

        var configs = new Dictionary<string, string>()
        {
            { MasaStackConfigConstant.VERSION, configuration.GetValue<string>(MasaStackConfigConstant.VERSION)! },
            { MasaStackConfigConstant.IS_DEMO, configuration.GetValue<bool>(MasaStackConfigConstant.IS_DEMO).ToString() },
            { MasaStackConfigConstant.DOMAIN_NAME, configuration.GetValue<string>(MasaStackConfigConstant.DOMAIN_NAME)! },
            { MasaStackConfigConstant.NAMESPACE, configuration.GetValue<string>(MasaStackConfigConstant.NAMESPACE)! },
            { MasaStackConfigConstant.CLUSTER, configuration.GetValue<string>(MasaStackConfigConstant.CLUSTER)! },
            { MasaStackConfigConstant.OTLP_URL, configuration.GetValue < string >(MasaStackConfigConstant.OTLP_URL)! },
            { MasaStackConfigConstant.REDIS, configuration.GetValue<string>(MasaStackConfigConstant.REDIS)! },
            { MasaStackConfigConstant.CONNECTIONSTRING, configuration.GetValue<string>(MasaStackConfigConstant.CONNECTIONSTRING)! },
            { MasaStackConfigConstant.MASA_STACK, configuration.GetValue<string>(MasaStackConfigConstant.MASA_STACK)! },
            { MasaStackConfigConstant.ELASTIC, configuration.GetValue<string>(MasaStackConfigConstant.ELASTIC)! },
            { MasaStackConfigConstant.ENVIRONMENT, environment },
            { MasaStackConfigConstant.ADMIN_PWD, configuration.GetValue<string>(MasaStackConfigConstant.ADMIN_PWD)! },
            { MasaStackConfigConstant.DCC_SECRET, configuration.GetValue<string>(MasaStackConfigConstant.DCC_SECRET)! },
            { MasaStackConfigConstant.SUFFIX_IDENTITY, configuration.GetValue<string>(MasaStackConfigConstant.SUFFIX_IDENTITY)! }
        };
        return configs;
    }
}
