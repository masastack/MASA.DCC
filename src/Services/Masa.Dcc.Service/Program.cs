// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMasaIdentityModel(options =>
{
    options.Environment = "environment";
    options.UserName = "name";
    options.UserId = "sub";
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

var redisOptions = builder.Configuration.GetSection("redis").Get<RedisConfigurationOptions>();
builder.Services.AddMasaRedisCache(redisOptions)
                .AddMasaMemoryCache(options =>
                {
                    options.SubscribeKeyType = SubscribeKeyTypes.SpecificPrefix;
                    options.SubscribeKeyPrefix = "masa.dcc:";
                });

builder.Services.AddPmClient(AppSettings.Get("PmClientAddress"));

var app = builder.Services
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
               .UseEventBus()
               .UseUoW<DccDbContext>(dbOptions => dbOptions.UseSqlServer().UseFilter())
               .UseEventLog<DccDbContext>()
               .UseRepository<DccDbContext>();
    })
    .AddServices(builder);

app.UseMasaExceptionHandler();

//seed data
await app.SeedDataAsync();

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
