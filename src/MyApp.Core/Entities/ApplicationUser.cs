using Microsoft.AspNetCore.Identity;

namespace MyApp.Core.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsBlocked { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public string? PhotoUrl { get; set; }
        public byte[]? PhotoData { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
