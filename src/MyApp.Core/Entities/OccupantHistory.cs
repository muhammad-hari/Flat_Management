namespace MyApp.Core.Entities
{
    public class OccupantHistory
    {
        public int Id { get; set; }
        public int OccupantId { get; set; }
        public Occupant Occupant { get; set; } = null!;
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;
        public string? Action { get; set; }
        public string? Notes { get; set; }
        public byte[]? PhotoData { get; set; }
        public string? DocumentName { get; set; }   // nama file asli
        public string? DocumentContentType { get; set; } // mime type
        public byte[]? DocumentData { get; set; }   // isi file (binary)
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
