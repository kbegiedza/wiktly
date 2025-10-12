using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;
using Ulfsoft.Extensions.DependencyInjection;
using Wiktly.Web.Areas.Identity.Configuration;
using Wiktly.Web.Areas.Identity.Data;
using Wiktly.Web.Areas.Identity.Services;
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
        const string issuer = "https://id.ulfsoft.com";
        var key = default(SecurityKey);

        hostBuilder.AddConfiguration<AuthenticationConfiguration>();

        var configuration = hostBuilder.Configuration;
        var authConfig = configuration.GetRequiredConfiguration<AuthenticationConfiguration>();

        services.AddDbContext<AuthDataContext>(options =>
        {
            options.ConfigureWarnings(b => b.Log(CoreEventId.ManyServiceProvidersCreatedWarning))
                   .UseNpgsql(authConfig.ConnectionString);

            options.UseOpenIddict();
        });

        services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                           .UseDbContext<AuthDataContext>();
                })
                .AddServer(options =>
                {
                    options.SetTokenEndpointUris("/api/v1/auth/login");

                    options.AllowPasswordFlow();

                    // options.SetIssuer(issuer);

                    var symmetricKey = Convert.FromBase64String(authConfig.EncryptionKey);
                    options.AddEncryptionKey(new SymmetricSecurityKey(symmetricKey));

                    // TODO: replace with .AddEncryptionCertificate
                    // see https://documentation.openiddict.com/configuration/encryption-and-signing-credentials
                    options.AddEphemeralSigningKey();

                    var coreBuilder = options.UseAspNetCore()
                                             .EnableTokenEndpointPassthrough();

                    if (hostBuilder.Environment.IsDevelopment())
                    {
                        coreBuilder.EnableStatusCodePagesIntegration();

                        options.DisableAccessTokenEncryption();
                    }
                })
                .AddValidation(option =>
                {
                    option.UseLocalServer();

                    option.UseAspNetCore();
                });

        services.AddMvc();

        services.AddIdentityCore<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AuthDataContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

        // TODO: Replace with real email sender
        services.AddTransient<IEmailSender, Areas.Identity.Services.NoOpEmailSender>();

        services.AddHostedService<AuthInitService>();

        const string cookieOrBearerScheme = "CookieOrBearerScheme";

        const bool useOpenIddictValidation = true;

        var authBuilder = services.AddAuthentication(o => { o.DefaultScheme = cookieOrBearerScheme; })
                                  .AddPolicyScheme(cookieOrBearerScheme, cookieOrBearerScheme, o =>
                                  {
                                      o.ForwardDefaultSelector = context =>
                                      {
                                          if (!context.Request.Path.StartsWithSegments("/api"))
                                          {
                                              return IdentityConstants.ApplicationScheme;
                                          }

                                          if (useOpenIddictValidation)
                                          {
                                              return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                                          }

                                          return JwtBearerDefaults.AuthenticationScheme;

                                      };
                                  });

        authBuilder.AddIdentityCookies(o => { });

        if (!useOpenIddictValidation)
        {
            authBuilder.AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = aud,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                };
            });
        }

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
