using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Wiktly.Web.Persistence.EntityFramework;

public class WiktlyDbContext : IdentityDbContext
{
    private readonly ILogger<WiktlyDbContext> _logger;

    public WiktlyDbContext(ILogger<WiktlyDbContext> logger,
                           DbContextOptions<WiktlyDbContext> options)
        : base(options)
    {
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _logger.LogTrace("Searching for assembly to apply configurations");

        var assembly = Assembly.GetAssembly(typeof(WiktlyDbContext)) ?? Assembly.GetCallingAssembly();

        _logger.LogTrace("Applying configurations from assembly: {Assembly}", assembly.FullName);

        modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
}