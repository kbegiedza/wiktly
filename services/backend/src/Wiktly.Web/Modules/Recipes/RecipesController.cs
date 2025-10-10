using Ulfsoft.Web.Mvc;

namespace Wiktly.Web.Modules.Recipes;

public class RecipesController : BaseApiController
{
    public RecipesController(ILogger<RecipesController> logger)
        : base(logger)
    {
    }
}