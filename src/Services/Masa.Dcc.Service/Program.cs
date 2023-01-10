// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddObservable(builder.Logging, builder.Configuration, false);
}

builder.Services.AddMasaIdentity(options =>
{
    options.UserName = "name";
    options.UserId = "sub";
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

builder.WebHost.UseKestrel(option =>
{
    option.ConfigureHttpsDefaults(options =>
    options.ServerCertificate = new X509Certificate2(Path.Combine("Certificates", "7348307__lonsid.cn.pfx"), "cqUza0MN"));
});

builder.Services.AddDaprClient();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.Authority = AppSettings.Get("IdentityServerUrl");
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters.ValidateAudience = false;
    options.MapInboundClaims = false;
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprStarter(opt =>
    {
        opt.DaprHttpPort = 3600;
        opt.DaprGrpcPort = 3601;
    });
}

builder.Services.AddMultilevelCache(distributedCacheAction: distributedCacheOptions =>
{
    distributedCacheOptions.UseStackExchangeRedisCache();
}, options =>
{
    options.SubscribeKeyPrefix = "masa.dcc:";
    options.SubscribeKeyType = SubscribeKeyType.SpecificPrefix;
});

builder.Services.AddPmClient(AppSettings.Get("PmClientAddress"));

builder.Services
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddTransient(typeof(IMiddleware<>), typeof(LogMiddleware<>))
    .AddFluentValidation(options =>
    {
        options.RegisterValidatorsFromAssemblyContaining<Program>();
    })
    .AddTransient(typeof(IMiddleware<>), typeof(ValidatorMiddleware<>))
    .AddDomainEventBus(options =>
    {
        options.UseIntegrationEventBus<IntegrationEventLogService>(options => options.UseDapr().UseEventLog<DccDbContext>())
               .UseEventBus(eventBusBuilder =>
               {
                   eventBusBuilder.UseMiddleware(typeof(DisabledCommandMiddleware<>));
               })
               .UseUoW<DccDbContext>(dbOptions => dbOptions.UseSqlServer().UseFilter())
               .UseEventLog<DccDbContext>()
               .UseRepository<DccDbContext>();
    });

var app = builder.AddServices(options =>
{
    options.DisableAutoMapRoute = true; // todo :remove it before v1.0
});

//seed data
await builder.SeedDataAsync();

if (app.Environment.IsDevelopment())
{
    app.UseMasaExceptionHandler(options =>
    {
        options.ExceptionHandler = context =>
        {
            if (context.Exception is not UserFriendlyException)
            {
                context.ToResult(context.Exception.Message);
            }
        };
    });
}
else
{
    app.UseMasaExceptionHandler();
}

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCloudEvents();
app.UseEndpoints(endpoints =>
{
    endpoints.MapSubscribeHandler();
});
app.UseHttpsRedirection();

app.Run();
