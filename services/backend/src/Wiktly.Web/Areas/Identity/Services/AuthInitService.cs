using OpenIddict.Abstractions;
using Wiktly.Web.Areas.Identity.Data;

namespace Wiktly.Web.Areas.Identity.Services;

public class AuthInitService : IHostedService
{
    private readonly ILogger<AuthInitService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AuthInitService(IServiceProvider serviceProvider, ILogger<AuthInitService> logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const string appName = "wiktly-app";

        _logger.LogInformation("Starting");

        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AuthDataContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var existingApp = await appManager.FindByClientIdAsync(appName, cancellationToken);

        if (existingApp is not null)
        {
            _logger.LogInformation("Default application '{AppName}' already exists", appName);

            return;
        }

        _logger.LogInformation("Creating new application '{AppName}'", appName);

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = appName,
            ClientSecret = "FCDCD1EF-4D9E-4BA5-BC7B-D20FF1C78CEF",
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

        _logger.LogInformation("Application '{AppName}' created successfully", appName);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}