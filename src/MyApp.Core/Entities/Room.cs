namespace MyApp.Core.Entities;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RoomNo { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public RoomCategory Category { get; set; } = null!;
    public int StatusId { get; set; }
    public RoomStatus Status { get; set; } = null!;
    public int ConditionId { get; set; }
    public RoomCondition Condition { get; set; } = null!;
    public int TotalOccupant { get; set; }
    public int Floor { get; set; }
    public int Size { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string Details { get; set; } = string.Empty;
    public byte[]? PhotoData { get; set; }
    public string? DocumentName { get; set; }   // nama file asli
    public string? DocumentContentType { get; set; } // mime type
    public byte[]? DocumentData { get; set; }   // isi file (binary)

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
