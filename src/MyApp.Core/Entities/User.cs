using System.ComponentModel.DataAnnotations;
namespace MyApp.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NRP { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }

        // Foreign keys
        [Required(ErrorMessage = "User Type is required")]
        public int? UserTypeId { get; set; }
        public UserType UserType { get; set; } = null!;
        [Required(ErrorMessage = "Role is required")]
        public int? RoleId { get; set; }
        public Role Role { get; set; } = null!;

        [Required(ErrorMessage = "Position is required")]
        public int? PositionId { get; set; }

        public Position Position { get; set; } = null!;
        public string Address { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone is required")]
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public byte[]? PhotoData { get; set; }
        public bool IsActive { get; set; } = false;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
