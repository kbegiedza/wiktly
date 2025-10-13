using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Wiktly.Web.Areas.Identity.Configuration;
using Wiktly.Web.Areas.Identity.Data;

namespace Wiktly.Web.Areas.Identity.Services;

public class AuthInitService : IHostedService
{
    private readonly ILogger<AuthInitService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<AuthenticationConfiguration> _configuration;

    public AuthInitService(IServiceProvider serviceProvider,
                           ILogger<AuthInitService> logger,
                           IOptions<AuthenticationConfiguration> configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting");

        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AuthDataContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var requiredApplications = _configuration.Value.DefaultApplications;

        foreach (var app in requiredApplications)
        {
            var existingApp = await appManager.FindByClientIdAsync(app.ClientId, cancellationToken);

            if (existingApp is not null)
            {
                _logger.LogInformation("Default application '{AppName}' already exists", app);
                continue;
            }

            _logger.LogInformation("Creating new application '{AppName}'", app.Name);

            await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                DisplayName = app.Name,
                ClientId = app.ClientId,
                ClientSecret = app.ClientSecret,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles
                }
            }, cancellationToken);

            _logger.LogInformation("Application '{AppName}' created successfully", app.Name);
        }

        _logger.LogInformation("All {ApplicationsCount} applications in place", requiredApplications.Length);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
