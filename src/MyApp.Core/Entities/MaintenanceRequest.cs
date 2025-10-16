namespace MyApp.Core.Entities;

public class MaintenanceRequest
{
    public int Id { get; set; }

    // --- Jadwal dan Lokasi ---
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddDays(1);

    // Relasi ke Gedung/Ruangan. Jika BuildingId diisi, permintaan mungkin untuk seluruh gedung.
    // Jika RoomId diisi, permintaan spesifik untuk ruangan tsb.
    public int BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    // RoomId bersifat nullable. Jika diisi, request ini spesifik untuk ruangan tsb.
    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    // --- Detail Permintaan ---
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty; // User yang membuat permintaan

    // --- Status & Dampak ---
    // Status pekerjaan: Pending, InProgress, Completed, Cancelled
    public string Status { get; set; } = "Pending";

    // Jika True, properti IsAvailable pada Room/Building yang terkait akan diubah menjadi false.
    // Properti IsAvailable Room akan kembali menjadi true saat Status menjadi 'Completed'.
    public bool IsRoomAffected { get; set; } = false;

    // --- Audit ---
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}