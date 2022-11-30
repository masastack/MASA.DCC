// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddObservable(builder.Logging, builder.Configuration, false);
}

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
builder.WebHost.UseKestrel(option =>
{
    option.ConfigureHttpsDefaults(options =>
    options.ServerCertificate = new X509Certificate2(Path.Combine("Certificates", "7348307__lonsid.cn.pfx"), "cqUza0MN"));
});

builder.Services.AddMasaConfiguration(option => option.UseDcc());

builder.Services.AddDaprClient();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<TokenProvider>();

builder.Services.AddMasaOpenIdConnect(builder.Services.GetMasaConfiguration().ConfigurationApi.GetPublic());
builder.AddMasaStackComponentsForServer("wwwroot/i18n", builder.Configuration["AuthServiceBaseAddress"], builder.Configuration["McServiceBaseAddress"]);

builder.Services.AddScoped<HttpClientAuthorizationDelegatingHandler>();
builder.Services.AddCaller(Assembly.Load("Masa.Dcc.ApiGateways.Caller"));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprStarter(opt =>
    {
        opt.DaprHttpPort = 3700;
        opt.DaprGrpcPort = 3701;
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
