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
        var initConfigObjectDomainService = services.GetRequiredService<InitConfigObjectDomainService>();
        var configPublishStore = services.GetRequiredService<IConfigPublishStore>();
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
        await InitPublicConfigAsync(
            services,
            context,
            masaConfig,
            contentRootPath,
            initConfigObjectDomainService,
            configPublishStore);

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
        InitConfigObjectDomainService initConfigObjectDomainService,
        IConfigPublishStore configPublishStore)
    {
        var configuration = service.GetRequiredService<IConfiguration>();
        var environmentName = configuration.GetValue<string>("MASA_ENVIRONMENT");
        if (string.IsNullOrWhiteSpace(environmentName))
            environmentName = configuration.GetValue<string>(MasaStackConfigConstant.ENVIRONMENT);
        if (string.IsNullOrWhiteSpace(environmentName))
            environmentName = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

        var clusterName = configuration.GetValue<string>("MASA_CLUSTER");
        if (string.IsNullOrWhiteSpace(clusterName))
            clusterName = masaConfig.Cluster;

        var environmentClusterId = await ResolveEnvironmentClusterIdAsync(service, context, configuration, environmentName, clusterName);

        // 1) Check DB initialization.
        var hasAnyConfigObject = await context.Set<ConfigObject>().AnyAsync();
        if (!hasAnyConfigObject)
        {
            Console.WriteLine($"[ConfigPublish] DB 未初始化，开始初始化: env={environmentName}, cluster={clusterName}, envClusterId={environmentClusterId}");
            await InitEnvConfigObjects(
                environmentClusterId,
                environmentName,
                service,
                context,
                masaConfig,
                contentRootPath,
                initConfigObjectDomainService);
        }

        // 2) Check publish store initialization by current storage type (Redis/Dapr),
        // and only initialize missing entries.
        Console.WriteLine($"[ConfigPublish] 开始检查并回填发布存储: env={environmentName}, cluster={clusterName}, envClusterId={environmentClusterId}");
        await BackfillPublicConfigToPublishStoreAsync(
            context,
            configPublishStore,
            environmentName,
            clusterName,
            environmentClusterId);
    }

    private static async Task<int> ResolveEnvironmentClusterIdAsync(
        IServiceProvider service,
        DccDbContext context,
        IConfiguration configuration,
        string environmentName,
        string clusterName)
    {
        try
        {
            var pmClient = service.GetRequiredService<IPmClient>();
            var envClusters = await pmClient.ClusterService.GetEnvironmentClustersAsync();
            var envCluster = envClusters?
                .FirstOrDefault(ec =>
                    string.Equals(ec.EnvironmentName, environmentName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(ec.ClusterName, clusterName, StringComparison.OrdinalIgnoreCase));
            if (envCluster != null && envCluster.Id > 0)
            {
                Console.WriteLine($"[ConfigPublish] 使用 PM 环境集群ID: env={environmentName}, cluster={clusterName}, envClusterId={envCluster.Id}");
                return envCluster.Id;
            }

            Console.WriteLine($"[ConfigPublish] 未在 PM 找到环境集群，转本地兜底: env={environmentName}, cluster={clusterName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ConfigPublish] 调用 PM 获取环境集群ID失败，转本地兜底: {ex.Message}");
        }

        var configuredEnvironmentClusterId = configuration.GetValue<int?>("MASA_ENVIRONMENT_CLUSTER_ID");
        if (configuredEnvironmentClusterId.HasValue && configuredEnvironmentClusterId.Value > 0)
        {
            Console.WriteLine($"[ConfigPublish] 使用配置环境集群ID: envClusterId={configuredEnvironmentClusterId.Value}");
            return configuredEnvironmentClusterId.Value;
        }

        var existedEnvironmentClusterId = await context.Set<PublicConfigObject>()
            .AsNoTracking()
            .Select(pco => (int?)pco.EnvironmentClusterId)
            .FirstOrDefaultAsync();
        if (existedEnvironmentClusterId.HasValue && existedEnvironmentClusterId.Value > 0)
        {
            Console.WriteLine($"[ConfigPublish] 使用数据库已有环境集群ID: envClusterId={existedEnvironmentClusterId.Value}");
            return existedEnvironmentClusterId.Value;
        }

        Console.WriteLine("[ConfigPublish] 未配置 MASA_ENVIRONMENT_CLUSTER_ID，且数据库无环境集群记录，默认使用 envClusterId=1");
        return 1;
    }

    private static async Task BackfillPublicConfigToPublishStoreAsync(
        DccDbContext context,
        IConfigPublishStore configPublishStore,
        string environmentName,
        string clusterName,
        int? environmentClusterId)
    {
        var publicConfigObjectsQuery = context.Set<PublicConfigObject>().AsNoTracking().AsQueryable();
        if (environmentClusterId.HasValue)
        {
            publicConfigObjectsQuery = publicConfigObjectsQuery.Where(pco => pco.EnvironmentClusterId == environmentClusterId.Value);
        }

        var publicConfigObjects = await publicConfigObjectsQuery.ToListAsync();
        List<ConfigObject> configObjects;
        if (publicConfigObjects.Any())
        {
            var configObjectIds = publicConfigObjects.Select(pco => pco.ConfigObjectId).ToList();
            configObjects = await context.Set<ConfigObject>()
                .AsNoTracking()
                .Where(co => configObjectIds.Contains(co.Id))
                .ToListAsync();
        }
        else
        {
            // Fallback for historical data where relation rows may be missing.
            configObjects = await context.Set<ConfigObject>()
                .AsNoTracking()
                .Where(co => co.Name == DccConst.DEFAULT_CONFIG_NAME || EF.Functions.Like(co.Name, "$public.%"))
                .ToListAsync();
        }

        if (!configObjects.Any())
        {
            Console.WriteLine($"[ConfigPublish] 未找到可回填的 ConfigObject: env={environmentName}, cluster={clusterName}, envClusterId={environmentClusterId}");
            return;
        }

        var writeCount = 0;
        Console.WriteLine($"configPublishStore type:{configPublishStore.GetType().FullName}");
        foreach (var configObject in configObjects)
        {
            var existing = await configPublishStore.GetAsync(
                environmentName,
                clusterName,
                DccConst.DEFAULT_PUBLIC_ID,
                configObject.Name);
            if (existing != null)
            {
                Console.WriteLine($"[ConfigPublish] 已存在发布存储记录，跳过回填: env={environmentName}, cluster={clusterName}, configObjectName={configObject.Name}");

                continue;
            }


            var model = new PublishReleaseModel
            {
                Content = configObject.Content,
                FormatLabelCode = configObject.FormatLabelCode,
                Encryption = configObject.Encryption
            };

            await configPublishStore.SetAsync(
                environmentName,
                clusterName,
                DccConst.DEFAULT_PUBLIC_ID,
                configObject.Name,
                model);
            writeCount++;
        }

        Console.WriteLine($"[ConfigPublish] 回填完成: env={environmentName}, cluster={clusterName}, 写入数量={writeCount}");
    }

    private static async Task InitEnvConfigObjects(int envClusterId, string environment, IServiceProvider service, DccDbContext context,
        IMasaStackConfig masaConfig,
        string contentRootPath,
        InitConfigObjectDomainService configObjectDomainService)
    {
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
