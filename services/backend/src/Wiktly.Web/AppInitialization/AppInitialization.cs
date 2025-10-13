using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Logging;
using Ulfsoft.Extensions.DependencyInjection;
using Wiktly.Web.Configuration;
using Console = Ulfsoft.Constants.Console;

namespace Wiktly.Web.AppInitialization;

public static class AppInitialization
{
    public static WebApplicationBuilder Initialize(string[] args)
    {
        System.Console.WriteLine(Console.UlfArt);

        var builder = WebApplication.CreateBuilder(args);

        if (builder.Environment.IsDevelopment())
        {
            IdentityModelEventSource.ShowPII = true;
        }

        builder.AddCommonServices();
        builder.AddInfrastructure();

        builder.AddAuthentication(AuthMode.OpenIddict);

        var services = builder.Services;

        services.AddFeatures();

        return builder;
    }

    private static IHostApplicationBuilder AddCommonServices(this IHostApplicationBuilder hostBuilder)
    {
        var services = hostBuilder.Services;
        var configuration = hostBuilder.Configuration;

        services.AddHttpClient();
        services.AddHttpContextAccessor();

        services.AddRazorPages();
        services.AddMvc();

        hostBuilder.AddConventions();

        hostBuilder.AddCors();
        hostBuilder.AddApiVersioning();

        services.AddSingleton<IConfiguration>(configuration);

        hostBuilder.AddConfiguration<HostingConfiguration>();

        return hostBuilder;
    }

    private static IHostApplicationBuilder AddConventions(this IHostApplicationBuilder hostBuilder)
    {
        var services = hostBuilder.Services;

        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });

        services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

        return hostBuilder;
    }

    private static IHostApplicationBuilder AddCors(this IHostApplicationBuilder hostBuilder)
    {
        var configuration = hostBuilder.Configuration;
        var services = hostBuilder.Services;
        var hostingConfiguration = configuration.GetRequiredConfiguration<HostingConfiguration>();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyHeader()
                       .AllowAnyMethod()
                       .WithOrigins(hostingConfiguration.AllowedOrigins);
            });
        });

        return hostBuilder;
    }
}
