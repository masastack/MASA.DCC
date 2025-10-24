// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

ValidatorOptions.Global.LanguageManager = new MasaLanguageManager();
GlobalValidationOptions.SetDefaultCulture("zh-CN");

await builder.Services.AddMasaStackConfigAsync(project: MasaStackProject.DCC, app: MasaStackApp.Service);
var masaStackConfig = builder.Services.GetMasaStackConfig();
var connStr = masaStackConfig.GetValue(MasaStackConfigConstant.CONNECTIONSTRING);
var dbModel = JsonSerializer.Deserialize<DbModel>(connStr)!;
bool isPgsql = string.Equals(dbModel.DbType, "postgresql", StringComparison.CurrentCultureIgnoreCase);
var publicConfiguration = builder.Services.GetMasaConfiguration().ConfigurationApi.GetPublic();

var ossSptions = publicConfiguration.GetSection(DccConstants.OssKey).Get<OssOptions>()!;
builder.Services.AddObjectStorage(option => option.UseAliyunStorage(options =>
{
    options.AccessKeyId = ossSptions.AccessId;
    options.AccessKeySecret = ossSptions.AccessSecret;
    options.Endpoint = ossSptions.Endpoint;
    options.RoleArn = ossSptions.RoleArn;
    options.RoleSessionName = ossSptions.RoleSessionName;
    options.Sts = new Masa.Contrib.Storage.ObjectStorage.Aliyun.Options.AliyunStsOptions
    {
        RegionId = ossSptions.RegionId
    };
}));

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddObservable(builder.Logging, () => new MasaObservableOptions
    {
        ServiceNameSpace = builder.Environment.EnvironmentName,
        ServiceVersion = masaStackConfig.Version,
        ServiceName = masaStackConfig.GetServiceId(MasaStackProject.DCC),
        ServiceInstanceId = builder.Configuration.GetValue<string>("HOSTNAME")!
    }, () => masaStackConfig.OtlpUrl, true);
}

builder.Services.AddMasaIdentity(options =>
{
    options.UserName = IdentityClaimConsts.USER_NAME;
    options.UserId = IdentityClaimConsts.USER_ID;
    options.Role = IdentityClaimConsts.ROLES;
    options.Environment = IdentityClaimConsts.ENVIRONMENT;
    options.Mapping(nameof(MasaUser.CurrentTeamId), IdentityClaimConsts.CURRENT_TEAM);
    options.Mapping(nameof(MasaUser.StaffId), IdentityClaimConsts.STAFF);
    options.Mapping(nameof(MasaUser.Account), IdentityClaimConsts.ACCOUNT);
});

builder.Services.AddScoped(serviceProvider =>
{
    var masaUser = serviceProvider.GetRequiredService<IUserContext>().GetUser<MasaUser>() ?? new MasaUser();
    return masaUser;
});

builder.Services.AddAutoInject();
builder.Services.AddDaprClient();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.Authority = masaStackConfig.GetSsoDomain();
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters.ValidateAudience = false;
    options.MapInboundClaims = false;
    options.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (
            sender,
            certificate,
            chain,
            sslPolicyErrors) => true
    };
});

builder.Services
    .AddValidatorsFromAssemblyContaining<AddConfigObjectDto>()
    .AddFluentValidationAutoValidation(configuration =>
    {
        configuration.OverrideDefaultResultFactoryWith<CustomResultFactory>();
    });

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprStarter();
}

builder.Services.AddStackMiddleware();
builder.Services.AddI18n(Path.Combine("Assets", "I18n"));

var redisOption = new RedisConfigurationOptions
{
    Servers = new List<RedisServerOptions> {
        new()
        {
            Host= masaStackConfig.RedisModel.RedisHost,
            Port= masaStackConfig.RedisModel.RedisPort
        }
    },
    DefaultDatabase = masaStackConfig.RedisModel.RedisDb,
    Password = masaStackConfig.RedisModel.RedisPassword,
    ClientName = builder.Configuration.GetValue<string>("HOSTNAME") ?? masaStackConfig.GetServiceId(MasaStackProject.DCC)
};

builder.Services.AddMultilevelCache(distributedCacheAction: distributedCacheOptions =>
{
    distributedCacheOptions.UseStackExchangeRedisCache(redisOption);
}, options =>
{
    options.SubscribeKeyPrefix = "masa.dcc:";
    options.SubscribeKeyType = SubscribeKeyType.SpecificPrefix;
});

builder.Services.AddPmClient(masaStackConfig.GetPmServiceDomain());
builder.Services.AddAuthClient(authServiceBaseAddress: masaStackConfig.GetAuthServiceDomain(), redisOption);

builder.Services
#if DEBUG
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
     {
         options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
         {
             Name = "Authorization",
             Type = SecuritySchemeType.ApiKey,
             Scheme = "Bearer",
             BearerFormat = "JWT",
             In = ParameterLocation.Header,
             Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer xxxxxxxxxxxxxxx\"",
         });
         options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
     })
#endif   
    .AddDomainEventBus(options =>
    {
        var connStr = masaStackConfig.GetConnectionString(MasaStackProject.DCC.Name);
        if (isPgsql)
            DccDbContext.RegistAssembly(Assembly.Load("Masa.Dcc.Infrastructure.EFCore.PostgreSql"));
        else
            DccDbContext.RegistAssembly(Assembly.Load("Masa.Dcc.Infrastructure.EFCore.SqlServer"));

        options.UseIntegrationEventBus(options =>
                                                                options.UseDapr()
                                                                             .UseEventBus())
                     .UseUoW<DccDbContext>(dbOptions =>
                                                                            (isPgsql ? dbOptions.UsePgsql(connStr, options => options.MigrationsAssembly("Masa.Dcc.Infrastructure.EFCore.PostgreSql")) :
                                                                                            dbOptions.UseSqlServer(connStr, options => options.MigrationsAssembly("Masa.Dcc.Infrastructure.EFCore.SqlServer"))).UseFilter())
                     .UseRepository<DccDbContext>();
    });


builder.Services.AddAutoInject([typeof(IAppConfigObjectRepository).Assembly, typeof(LabelDomainService).Assembly, typeof(Masa.Dcc.Service.Admin.Services.AppService).Assembly]);


builder.Services.AddI18n(Path.Combine("Assets", "I18n"));

//seed data
await builder.SeedDataAsync();

var app = builder.AddServices(config =>
{
    config.DisableAutoMapRoute = true;
    config.RouteHandlerBuilder = (builder) =>
    {
        builder.RequireAuthorization();
        builder.AddFluentValidationAutoValidation();
    };
});

app.UseMasaExceptionHandler(opt =>
{
    opt.ExceptionHandler = context =>
    {
        if (context.Exception is UserFriendlyException userFriendlyException)
        {
            context.ToResult(userFriendlyException.ErrorCode!, 299);
        }
        else if (context.Exception is ValidationException validationException)
        {
            context.ToResult(validationException.Errors.Select(err => err.ToString()).FirstOrDefault()!);
        }
        else if (context.Exception is UserStatusException userStatusException)
        {
            context.ToResult(userStatusException.Message, 293);
        }
    };
});

// Configure the HTTP request pipeline.
#if DEBUG
app.UseSwagger();
app.UseSwaggerUI();
#endif

app.UseRouting();
app.UseI18n();

app.UseAuthentication();
app.UseAuthorization();

app.UseStackMiddleware();
app.UseMiddleware<CheckUserMiddleware>();

app.UseCloudEvents();
app.UseEndpoints(endpoints =>
{
    endpoints.MapSubscribeHandler();
});
app.UseHttpsRedirection();

app.UseI18n();

app.Run();
