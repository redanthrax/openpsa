using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using OpenPsa.Web;
using OpenPsa.Web.Features.Authentication.Services;
using OpenPsa.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddSingleton<ThemeService>();

builder.Services.AddHttpClient<IApiClient, ApiClient>(client => {
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]
        ?? builder.HostEnvironment.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<ITokenStore, LocalStorageTokenStore>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
