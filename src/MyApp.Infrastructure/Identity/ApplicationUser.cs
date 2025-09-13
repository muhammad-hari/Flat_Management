using Microsoft.AspNetCore.Identity;

namespace MyApp.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
