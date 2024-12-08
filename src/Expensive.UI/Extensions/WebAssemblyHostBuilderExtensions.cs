using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Expensive.UI.Extensions;

public static class WebAssemblyHostBuilderExtensions
{
    public static WebAssemblyHostBuilder AddRootComponents(this WebAssemblyHostBuilder builder)
    {
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        return builder;
    }
    
    public static WebAssemblyHostBuilder AddApplicationServices(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddApplicationServices(builder.HostEnvironment.BaseAddress);
        return builder;
    }
}