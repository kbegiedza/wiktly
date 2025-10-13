using Asp.Versioning;
using AspNetCore.Swagger.Themes;
using NSwag;
using NSwag.Generation.Processors.Security;
using Ulfsoft.Extensions.DependencyInjection;
using Wiktly.Web.Configuration;
using OpenApiSecurityScheme = NSwag.OpenApiSecurityScheme;
using OpenApiServer = NSwag.OpenApiServer;

namespace Wiktly.Web.AppInitialization;

public static class ApiVersioningInitializer
{
    public static IHostApplicationBuilder AddApiVersioning(this IHostApplicationBuilder hostBuilder)
    {
        var configuration = hostBuilder.Configuration;
        var services = hostBuilder.Services;

        var hostingConfiguration = configuration.GetRequiredConfiguration<HostingConfiguration>();
        var versioningConfiguration = new VersioningConfiguration()
        {
            ApiVersions =
            [
                new ApiVersion(1, 0)
            ]
        };

        foreach (var version in versioningConfiguration.ApiVersions)
        {
            services.AddOpenApiDocument(options =>
            {
                const string securitySchemeName = "Bearer";

                options.Title = "Wiktly API";
                options.Description = "Wiktly API Documentation";

                options.DocumentName = $"v{version.MajorVersion}";
                options.ApiGroupNames = [$"v{version.MajorVersion}"];
                options.Version = version.ToString();

                options.AddSecurity(securitySchemeName, new OpenApiSecurityScheme
                {
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    Type = OpenApiSecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                });

                options.PostProcess += document =>
                {
                    var url = hostingConfiguration.Url;
                    if (url is not null)
                    {
                        var baseServer = new OpenApiServer
                        {
                            Url = url,
                            Description = "Wiktly API Server"
                        };

                        document.Servers.Clear();
                        document.Servers.Add(baseServer);
                    }
                };

                options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(securitySchemeName));
            });
        }

        services.AddApiVersioning(options => { options.AssumeDefaultVersionWhenUnspecified = false; })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return hostBuilder;
    }

    public static WebApplication UseApiVersioning(this WebApplication app, IConfiguration configuration)
    {
        var hostingConfiguration = configuration.GetRequiredConfiguration<HostingConfiguration>();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi(settings =>
            {
                settings.Path = "/openapi/{documentName}.json";
                settings.PostProcess = (document, request) =>
                {
                    var url = hostingConfiguration.Url;
                    if (url is not null)
                    {
                        var baseServer = new OpenApiServer
                        {
                            Url = url,
                            Description = "Wiktly API Server"
                        };

                        document.Servers.Clear();
                        document.Servers.Add(baseServer);
                    }
                };
            });

            app.UseSwaggerUi(ModernStyle.Light, settings =>
            {
                settings.EnableAllAdvancedOptions();

                settings.Path = "/openapi";
                settings.DocumentPath = "/openapi/{documentName}.json";
                settings.ServerUrl = hostingConfiguration.Url;
            });
        }

        return app;
    }
}