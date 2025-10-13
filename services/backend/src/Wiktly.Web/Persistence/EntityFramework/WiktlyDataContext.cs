using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Wiktly.Web.Persistence.EntityFramework;

public class WiktlyDataContext: DbContext
{
    private readonly ILogger<WiktlyDataContext> _logger;

    public WiktlyDataContext(ILogger<WiktlyDataContext> logger,
                           DbContextOptions<WiktlyDataContext> options)
        : base(options)
    {
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _logger.LogTrace("Searching for assembly to apply configurations");

        var assembly = Assembly.GetAssembly(typeof(WiktlyDataContext)) ?? Assembly.GetCallingAssembly();

        _logger.LogTrace("Applying configurations from assembly: {Assembly}", assembly.FullName);

        modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
}
