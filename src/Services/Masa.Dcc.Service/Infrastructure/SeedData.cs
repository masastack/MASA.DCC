// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System.Text.Encodings.Web;

namespace Masa.Dcc.Service.Admin.Infrastructure;

internal static class IHostExtensions
{
    static readonly Dictionary<string, string> dicConfigFiles = new()
    {
        { "$public.AliyunPhoneNumberLogin","$public.AliyunPhoneNumberLogin.json"},
        {"$public.Email","$public.Email.json"},
        {"$public.Sms","$public.Sms.json"},
        {"$public.Cdn","$public.Cdn.json"},
        {"$public.WhiteListOptions","$public.WhiteListOptions.json"},
        {"$public.i18n.en-us","$public.i18n.en-us.json"},
        {"$public.i18n.zh-cn","$public.i18n.zh-cn.json"},
    };

    public static async Task SeedDataAsync(this WebApplicationBuilder builder)
    {
        var services = builder.Services.BuildServiceProvider().CreateScope().ServiceProvider;
        var context = services.GetRequiredService<DccDbContext>();

        await MigrateAsync(context);

        var labelDomainService = services.GetRequiredService<LabelDomainService>();
        var configObjectDomainService = services.GetRequiredService<InitConfigObjectDomainService>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var contentRootPath = env.ContentRootPath;
        var masaConfig = builder.Services.GetMasaStackConfig();
        var pmClient = services.GetRequiredService<IPmClient>();

        string system = "system";
        var userSetter = services.GetService<IUserSetter>();
        var auditUser = new IdentityUser() { Id = masaConfig.GetDefaultUserId().ToString(), UserName = system };
        var userSetterHandle = userSetter!.Change(auditUser);

        unitOfWork.UseTransaction = false;
        var configs = GetConfigMap(builder.Services);

        await InitDccDataAsync(context, labelDomainService);

        await InitPublicConfigAsync(context, masaConfig, contentRootPath, pmClient, configObjectDomainService, configs);
        userSetterHandle.Dispose();
    }

    private static async Task MigrateAsync(DccDbContext context)
    {
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
            var publicConfig = new PublicConfig("Public", "public-$Config", "Public config");
            await context.Set<PublicConfig>().AddAsync(publicConfig);
        }

        await context.SaveChangesAsync();
    }

    private static async Task InitPublicConfigAsync(
        DccDbContext context,
        IMasaStackConfig masaConfig,
        string contentRootPath,
        IPmClient pmClient,
        InitConfigObjectDomainService configObjectDomainService,
        Dictionary<string, string> configs)
    {
        var environments = await pmClient.EnvironmentService.GetListAsync();
        ArgumentNullException.ThrowIfNull(environments);
        if (environments == null || environments.Count == 0)
            throw new ArgumentException("Environments must having one value");
        if (!environments.Any(env => env.Name == masaConfig.Environment))
            throw new ArgumentException($"masastack envinoment:{masaConfig.Environment} is not in pm's envinoments: {string.Join(',', environments.Select(env => env.Name))}");

        var appid = "public-$Config";
        var publicConfigs = new Dictionary<string, string>() {
                {"$public.DefaultConfig", JsonSerializer.Serialize(configs, new JsonSerializerOptions{ Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,WriteIndented = true})}
        };
        foreach (var (key, value) in dicConfigFiles)
        {
            var filePath = CombineFilePath(contentRootPath, value, masaConfig.Environment);
            publicConfigs.Add(key, await GetFileContentAsync(filePath));
        }
        await configObjectDomainService.InitConfigObjectAsync(masaConfig.Environment, masaConfig.Cluster, appid, publicConfigs, ConfigObjectType.Public, false);

        var encryptionPublicConfigs = new Dictionary<string, string>
        {
                { "$public.Oss",await GetFileContentAsync(CombineFilePath(contentRootPath,"$public.Oss.json",masaConfig.Environment)) }
        };
        await configObjectDomainService.InitConfigObjectAsync(masaConfig.Environment, masaConfig.Cluster, appid, encryptionPublicConfigs, ConfigObjectType.Public, true);
        await context.SaveChangesAsync();

    }

    private static Dictionary<string, string> GetConfigMap(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

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

    private static async Task<string> GetFileContentAsync(string path)
    {
        using var fs = new FileStream(path, FileMode.Open);
        using var reader = new StreamReader(fs);
        return await reader.ReadToEndAsync();
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
