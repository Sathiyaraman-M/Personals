using Expensive.Server.Configurations;

namespace Expensive.Server.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddTraditionalFormatEnvironmentVariables(this IConfigurationBuilder builder,
        string prefix = "")
    {
        return builder.Add(new EnvironmentalVariablesConfigurationSource(prefix));
    }
}