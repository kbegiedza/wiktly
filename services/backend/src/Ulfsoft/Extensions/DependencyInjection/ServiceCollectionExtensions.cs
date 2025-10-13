using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Ulfsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddConfiguration<T>(this IHostApplicationBuilder builder)
        where T : class
    {
        var configuration = builder.Configuration;

        var config = configuration.GetRequiredConfiguration<T>();

        builder.Services
               .AddOptions<T>()
               .Bind(configuration.GetSection(typeof(T).Name));

        builder.Services.TryAddSingleton(config);

        return builder;
    }
}
