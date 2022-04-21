using Masa.Dcc.Web.Admin.Rcl.Global;
using Masa.Dcc.Web.Admin.Rcl;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Masa.Dcc.Web.Admin.WebAssembly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, TestAuthStateProvider>();

await builder.Services.AddGlobalForWasmAsync(builder.HostEnvironment.BaseAddress);

builder.RootComponents.Add(typeof(App), "#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddMasaBlazor(builder =>
{
    builder.UseTheme(option =>
    {
        option.Primary = "#4318FF";
        option.Accent = "#4318FF";
    });
});

await builder.Build().RunAsync();
