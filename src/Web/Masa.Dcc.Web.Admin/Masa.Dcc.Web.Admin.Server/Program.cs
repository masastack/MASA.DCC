// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddMasaStackComponentsAsync(project: MasaStackProject.DCC);

var masaStackConfig = builder.Services.GetMasaStackConfig();
builder.Services.AddValidatorsFromAssembly(typeof(LabelValueModel).Assembly, includeInternalTypes: true);

MasaOpenIdConnectOptions masaOpenIdConnectOptions = new MasaOpenIdConnectOptions
{
    Authority = masaStackConfig.GetSsoDomain(),
    ClientId = masaStackConfig.GetWebId(MasaStackProject.DCC),
    Scopes = new List<string> { "offline_access" }
};

string dccServiceAddress = masaStackConfig.GetDccServiceDomain();
#if DEBUG
dccServiceAddress = "http://localhost:6196";
#endif

builder.Services.AddDccApiGateways(option =>
{
    option.DccServiceAddress = dccServiceAddress;
    option.AuthorityEndpoint = masaOpenIdConnectOptions.Authority;
    option.ClientId = masaOpenIdConnectOptions.ClientId;
    option.ClientSecret = masaOpenIdConnectOptions.ClientSecret;
});

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseKestrel(option =>
    {
        option.ConfigureHttpsDefaults(options =>
        {
            options.ServerCertificate = X509Certificate2.CreateFromPemFile("./ssl/tls.crt", "./ssl/tls.key");
            options.CheckCertificateRevocation = false;
        });
    });
}

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

IdentityModelEventSource.ShowPII = true;
builder.Services.AddMasaOpenIdConnect(masaOpenIdConnectOptions);

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
