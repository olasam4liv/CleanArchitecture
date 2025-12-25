
using System.Collections;

namespace Web.Api.Extensions;

public static class ConfigurationExtension
{
    public static IConfiguration ReplaceEnvironmentVariables(this IConfiguration configuration)
    {
        IDictionary env = Environment.GetEnvironmentVariables();
        var dict = new Dictionary<string, string?>();
        foreach (KeyValuePair<string, string?> kvp in configuration.AsEnumerable())
        {
            if (kvp.Value is not null && kvp.Value.Contains("${"))
            {
                string key = kvp.Value.Replace("${", "").Replace("}", "");
                dict[kvp.Key] = env[key]?.ToString();
            }
        }

        return new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddInMemoryCollection(dict)
            .Build();
    }
}