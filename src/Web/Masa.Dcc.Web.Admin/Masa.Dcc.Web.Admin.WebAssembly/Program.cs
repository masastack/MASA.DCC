// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddSingleton(sp => builder.Configuration);

await builder.Services.AddMasaStackConfigAsync(builder.Configuration);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.RootComponents.RegisterCustomElement<App>("mfe-dcc-app");

var masaStackConfig = builder.Services.GetMasaStackConfig();

MasaOpenIdConnectOptions masaOpenIdConnectOptions = new()
{
    Authority = masaStackConfig.GetSsoDomain(),
    ClientId = masaStackConfig.GetWebId(MasaStackProject.DCC),
    Scopes = new List<string> { "openid", "profile" }
};

await builder.AddMasaOpenIdConnectAsync(masaOpenIdConnectOptions);
builder.Services.AddDccApiGateways(option =>
{
    option.DccServiceAddress = masaStackConfig.GetDccServiceDomain();
    option.AuthorityEndpoint = masaOpenIdConnectOptions.Authority;
    option.ClientId = masaOpenIdConnectOptions.ClientId;
    option.ClientSecret = masaOpenIdConnectOptions.ClientSecret;
});

var projectPrefix = MasaStackProject.DCC.Name.ToLower();
string i18nDirectoryPath = builder.HostEnvironment.BaseAddress.TrimEnd('/').Replace(projectPrefix, $"stack/{projectPrefix}") + "/_content/Masa.Dcc.Web.Admin.Rcl/i18n";
builder.Services.AddMasaStackComponent(MasaStackProject.DCC, i18nDirectoryPath, microFrontend: true);

var host = builder.Build();
await host.Services.InitializeMasaStackApplicationAsync();
await host.RunAsync();
