namespace MyApp.Core.Entities
{
    public class Occupant
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;
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
