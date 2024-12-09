using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Personals.UI.Extensions;

await WebAssemblyHostBuilder
    .CreateDefault(args)
    .AddRootComponents()
    .AddApplicationServices()
    .Build()
    .RunAsync();