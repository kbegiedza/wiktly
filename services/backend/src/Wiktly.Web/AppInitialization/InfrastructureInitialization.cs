using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Ulfsoft.Extensions.DependencyInjection;
using Wiktly.Web.Configuration;
using Wiktly.Web.Infrastructure.Persistence.EntityFramework;

namespace Wiktly.Web.AppInitialization;

public static class InfrastructureInitializer
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder hostBuilder)
    {
        hostBuilder.AddDatabase();

        return hostBuilder;
    }

    private static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder hostBuilder)
    {
        var services = hostBuilder.Services;
        var dbConfig = hostBuilder.Configuration.GetRequiredConfiguration<NpgsqlConfiguration>("Persistence");

        services.AddDbContextPool<WiktlyDbContext>(options =>
            options.UseNpgsql(dbConfig.ConnectionString, builder =>
            {
                builder.ConfigureDataSource(dataSourceBuilder =>
                {
                    dataSourceBuilder.EnableDynamicJson()
                        .ConfigureJsonOptions(new JsonSerializerOptions()
                        {
                            Converters =
                            {
                                new JsonStringEnumConverter()
                            }
                        });
                });
            })
        );

        return hostBuilder;
    }
}