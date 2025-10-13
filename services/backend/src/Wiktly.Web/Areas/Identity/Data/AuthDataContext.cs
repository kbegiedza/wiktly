using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Wiktly.Web.Areas.Identity.Data;

public class AuthDataContext : IdentityDbContext<IdentityUser>
{
    public AuthDataContext(DbContextOptions<AuthDataContext> options)
        : base(options)
    {
    }
}
