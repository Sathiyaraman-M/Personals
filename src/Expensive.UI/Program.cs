using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Expensive.UI.Extensions;

await WebAssemblyHostBuilder
    .CreateDefault(args)
    .AddRootComponents()
    .AddApplicationServices()
    .Build()
    .RunAsync();