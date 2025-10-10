using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ulfsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddConfiguration<T>(this IHostApplicationBuilder builder)
        where T : class
    {
        var config = builder.Configuration.GetRequiredConfiguration<T>();

        builder.Services.AddSingleton(config);
        return builder;

    }
}
