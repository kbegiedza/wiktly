using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Ulfsoft.Extensions.DependencyInjection;
using Wiktly.Web.Areas.Identity.Data;
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

        builder.AddAuthentication();

        var services = builder.Services;

        services.AddFeatures();

        return builder;
    }

    private static IHostApplicationBuilder AddAuthentication(this IHostApplicationBuilder hostBuilder)
    {
        var services = hostBuilder.Services;

        // TODO: Replace with real values
        const string authority = "https://localhost";
        const string aud = "authenticated";
        const string iss = "issuer";
        var key = default(SecurityKey);

        var databaseConfig = hostBuilder.Configuration.GetRequiredConfiguration<NpgsqlConfiguration>("Authentication:Persistence");

        // Add framework services.
        services.AddDbContext<AuthDataContext>(options =>
            options.ConfigureWarnings(b => b.Log(CoreEventId.ManyServiceProvidersCreatedWarning))
                   .UseNpgsql(databaseConfig.ConnectionString));

        services.AddMvc();

        services.AddIdentityCore<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AuthDataContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

        services.AddAuthentication(o =>
                {
                    o.DefaultScheme = IdentityConstants.ApplicationScheme;
                    o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies(b => { });

        services.AddTransient<IEmailSender, Wiktly.Web.Areas.Identity.Services.NoOpEmailSender>();

        services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = iss,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidAudience = aud,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                    };
                });

        return hostBuilder;
    }

    private static IHostApplicationBuilder AddCommonServices(this IHostApplicationBuilder hostBuilder)
    {
        var services = hostBuilder.Services;
        var configuration = hostBuilder.Configuration;

        services.AddHttpClient();
        services.AddHttpContextAccessor();

        services.AddRazorPages();

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