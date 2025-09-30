namespace MyApp.Core.Entities
{
    public class Visitor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? NIK { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; } = null;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int? OccupantId { get; set; }
        public Occupant? Occupant { get; set; } = null!;
        public string? Purpose { get; set; }
        public string? Relation { get; set; }
        public int? RoomId { get; set; }
        public Room? Room { get; set; } = null!;
        public DateTime ArrivedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DepartedAt { get; set; } = null;
        public byte[]? PhotoData { get; set; }
        public string? DocumentName { get; set; }   // nama file asli
        public string? DocumentContentType { get; set; } // mime type
        public byte[]? DocumentData { get; set; }   // isi file (binary)

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
