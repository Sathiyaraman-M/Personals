using Personals.Server.Extensions;
using FluentMigrator.Runner;
using Personals.Server.Middlewares;

namespace Personals.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddTraditionalFormatEnvironmentVariables();

        builder.Services.AddApplicationServices();

        var app = builder.Build();

        app.UseCors();

        if (builder.Configuration["REDIRECT_HTTPS"] == "true")
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        app.UseBlazorFrameworkFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/healthz", () => "Hello World!");
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.UseMiddleware<ErrorHandlerMiddleware>();

        app.Services.CreateScope()
            .ServiceProvider.GetRequiredService<IMigrationRunner>()
            .MigrateUp();

        await app.RunAsync();
    }
}