using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Ulfsoft.Extensions.DependencyInjection;
using Wiktly.Web.Areas.Identity.Configuration;
using Wiktly.Web.Areas.Identity.Data;
using Wiktly.Web.Areas.Identity.Services;
using Wiktly.Web.Constants;

namespace Wiktly.Web.AppInitialization;

public static class AuthInitialization
{
    public static IHostApplicationBuilder AddAuthentication(this IHostApplicationBuilder hostBuilder, AuthMode authMode)
    {
        var services = hostBuilder.Services;

        hostBuilder.AddConfiguration<AuthenticationConfiguration>();

        switch (authMode)
        {
            case AuthMode.OpenIddict:
                hostBuilder.AddOpenIddict();
                break;
            case AuthMode.Invalid:
            default:
                throw new NotSupportedException($"Unsupported authentication mode: '{authMode}'");
        }
        
        services.AddIdentityCore<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AuthDataContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

        // TODO: Replace with real email sender
        services.AddTransient<IEmailSender, Areas.Identity.Services.NoOpEmailSender>();

        services.AddHostedService<AuthInitService>();

        var authBuilder = services.AddAuthentication(o =>
                                  {
                                      o.DefaultScheme = AuthenticationSchemas.CookieOrTokenScheme.Key;
                                  })
                                  .AddPolicyScheme(AuthenticationSchemas.CookieOrTokenScheme.Key,
                                      AuthenticationSchemas.CookieOrTokenScheme.Name, o =>
                                      {
                                          o.ForwardDefaultSelector = context =>
                                          {
                                              var isApiRequest = context.Request.Path.StartsWithSegments("/api/",
                                                  StringComparison.OrdinalIgnoreCase);
                                              if (!isApiRequest)
                                              {
                                                  return IdentityConstants.ApplicationScheme;
                                              }

                                              if (authMode == AuthMode.OpenIddict)
                                              {
                                                  return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                                              }

                                              return JwtBearerDefaults.AuthenticationScheme;
                                          };
                                      });

        authBuilder.AddIdentityCookies();

        return hostBuilder;
    }

    private static IHostApplicationBuilder AddOpenIddict(this IHostApplicationBuilder hostBuilder)
    {
        var services = hostBuilder.Services;
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
                    options.SetTokenEndpointUris("/api/v1/auth/token");

                    options.AllowPasswordFlow()
                           .AllowRefreshTokenFlow();

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

        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;

            options.SignIn.RequireConfirmedAccount = false;
        });

        return hostBuilder;
    }
}
