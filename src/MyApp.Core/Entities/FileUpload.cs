namespace MyApp.Core.Entities;

public class FileUpload
{
    public int Id { get; set; }

    /// <summary>Nama entitas pemilik, misal "Building", "User", "Room"</summary>
    public string OwnerType { get; set; } = string.Empty;

    /// <summary>Id entitas pemilik</summary>
    public int OwnerId { get; set; }

    /// <summary>Path atau URL file</summary>
    public string Path { get; set; } = string.Empty;
    public bool IsImage => ContentType != null && ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}
