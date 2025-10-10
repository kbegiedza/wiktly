using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Ulfsoft.Web.Mvc;

/// <summary>
/// Base controller for API endpoints. Provides a logger instance for derived controllers.
/// </summary>
[ApiController]
[Route(RoutePrefix)]
public class BaseApiController : ControllerBase
{
    private const string RoutePrefix = "v{version:apiVersion}/[controller]";

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseApiController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to be used by the controller.</param>
    public BaseApiController(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets the logger instance for the controller.
    /// </summary>
    protected ILogger Logger { get; private set; }

    protected bool TryGetUserId(out Guid userId)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            userId = Guid.Empty;

            return false;
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is not null
            && Guid.TryParse(userIdClaim.Value, out userId))
        {
            return true;
        }

        userId = Guid.Empty;

        return false;
    }
}