using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.Models
{
    public class ProfileModel
    {
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name must be between {2} and {1} characters", MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        public IList<string>? Roles { get; set; }
    }
}