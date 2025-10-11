using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Ulfsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddConfiguration<T>(this IHostApplicationBuilder builder)
        where T : class
    {
        var config = builder.Configuration.GetRequiredConfiguration<T>();

        builder.Services.TryAddSingleton(config);

        return builder;

    }
}
