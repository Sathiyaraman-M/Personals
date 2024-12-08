using System.Collections;

namespace Expensive.Server.Configurations;

public static class EnvironmentVariablesConfiguration
{
    internal static IEnumerable<string> Keys(string rawKey)
    {
        yield return rawKey;

        var parts = rawKey.Split('_').Where(p => p != "").ToArray();

        for (var i = 1; i < parts.Length; i++)
        {
            var beforeColon = parts.Take(i);
            var afterColon = parts.Skip(i);

            yield return string.Join("", beforeColon) + ":" + string.Join("", afterColon);
        }
    }
}

public class EnvironmentalVariablesConfigurationProvider(string prefix) : ConfigurationProvider
{
    public override void Load()
    {
        Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
        {
            var (key, value) = ((string)environmentVariable.Key, (string?)environmentVariable.Value);
            var payload = PayloadFrom(key, value);
            foreach (var record in payload)
            {
                Data[record.Key] = record.Value;
            }
        }
    }

    private IEnumerable<KeyValuePair<string, string?>> PayloadFrom(string key, string? value)
    {
        if (prefix == "")
        {
            return EnvironmentVariablesConfiguration.Keys(key).Select(k => new KeyValuePair<string, string?>(k, value));
        }

        return key.StartsWith(prefix, StringComparison.InvariantCulture)
            ? EnvironmentVariablesConfiguration.Keys(key.Replace(prefix, ""))
                .Select(k => new KeyValuePair<string, string?>(k, value))
            : [];
    }
}

public class EnvironmentalVariablesConfigurationSource(string prefix) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new EnvironmentalVariablesConfigurationProvider(prefix);
}