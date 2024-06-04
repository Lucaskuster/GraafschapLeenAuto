using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GraafschapLeenAuto.Shared.Clients;
using GraafschapLeenAuto.Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using GraafschapLeenAuto.Blazor.Handler;
using GraafschapLeenAuto.Shared.Interfaces;

namespace GraafschapLeenAuto.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped<LocalStorageService>();
            builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

            builder.Services.AddScoped<AuthorizationMessageHandler>();

            builder.Services.AddScoped<UserHttpClient>();
            builder.Services.AddHttpClient(nameof(UserHttpClient)).AddHttpMessageHandler<AuthorizationMessageHandler>(); 

            builder.Services.AddScoped<AuthHttpClient>();

            builder.Services.AddScoped<GraafschapLeenAutoAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<GraafschapLeenAutoAuthenticationStateProvider>());
            builder.Services.AddAuthorizationCore();

            await builder.Build().RunAsync();
        }
    }
}
