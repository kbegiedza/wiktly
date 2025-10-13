using Microsoft.Extensions.Configuration;

namespace Ulfsoft.Extensions.DependencyInjection;

public static class ConfigurationSettingsExtensions
{
    public static T? GetConfiguration<T>(this IConfiguration configuration)
    {
        var section = configuration.GetSection(typeof(T).Name);

        return section.Get<T>();
    }

    public static T? GetConfiguration<T>(this IConfiguration configuration, string parentSection)
    {
        var section = configuration.GetSection(parentSection)
                                   .GetSection(typeof(T).Name);

        return section.Get<T>();
    }

    public static T GetRequiredConfiguration<T>(this IConfiguration configuration)
    {
        var settings = configuration.GetSection(typeof(T).Name)
                                    .Get<T>();

        if (settings != null)
        {
            return settings;
        }

        throw new KeyNotFoundException($"Unable to find {typeof(T).Name} in injected configuration");
    }

    public static T GetRequiredConfiguration<T>(this IConfiguration configuration, string parentSection)
    {
        var settings = configuration.GetSection(parentSection)
                                    .GetSection(typeof(T).Name)
                                    .Get<T>();

        if (settings != null)
        {
            return settings;
        }

        throw new KeyNotFoundException($"Unable to find {parentSection}:{typeof(T).Name} in injected configuration");
    }
}
