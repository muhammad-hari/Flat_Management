using Microsoft.AspNetCore.Identity;

namespace MyApp.Infrastructure.Identity
{
    // Gunakan int sebagai Id
    public class ApplicationUser : IdentityUser<int>
    {
        // Properti tambahan
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsBlocked { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
