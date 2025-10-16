using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.Entities
{
    public class Weapon
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Document and photo
        public string? DocumentName { get; set; }
        public byte[]? DocumentData { get; set; }
        public string? DocumentContentType { get; set; }

        public string? PhotoName { get; set; }
        public byte[]? PhotoData { get; set; }
        public string? PhotoContentType { get; set; }

        public bool IsAvailable { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
