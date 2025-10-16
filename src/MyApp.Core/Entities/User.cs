using System.ComponentModel.DataAnnotations;
namespace MyApp.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Foreign keys

        [Required(ErrorMessage = "Role is required")]
        public int? RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public string PasswordHash { get; set; } = string.Empty;
        public byte[]? PhotoData { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsBlocked { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }

}
