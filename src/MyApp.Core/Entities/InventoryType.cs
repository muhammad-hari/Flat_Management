namespace MyApp.Core.Entities;

// MyApp.Core.Entities/InventoryType.cs

using System.ComponentModel.DataAnnotations;

public class InventoryType
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nama tipe tidak boleh kosong")]
    [StringLength(100, ErrorMessage = "Nama tipe maksimal 100 karakter")]
    public string TypeName { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Detail maksimal 255 karakter")]
    public string Details { get; set; } = string.Empty;

    // Properti lainnya...
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
