namespace MyApp.Core.Entities
{
    public class Vendor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? SecondPhone { get; set; }
        public string? Address { get; set; }
        public string? Director { get; set; }
        public string? BusinnesType { get; set; }
        public string? Details { get; set; }
        public byte[]? PhotoData { get; set; }
        public string? DocumentName { get; set; }   // nama file asli
        public string? DocumentContentType { get; set; } // mime type
        public byte[]? DocumentData { get; set; }   // isi file (binary)
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
