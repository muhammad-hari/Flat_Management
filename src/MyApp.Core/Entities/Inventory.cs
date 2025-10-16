using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Core.Entities
{
    public class Inventory
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }

        public int? InventoryTypeId { get; set; }
        public InventoryType? InventoryType { get; set; }

        public int? RepositoryId { get; set; }
        public Repository? Repository { get; set; }

        public string? Description { get; set; }

        public BarcodeType BarcodeToGenerate { get; set; } = BarcodeType.QRCode;

        public string? GeneratedBarcodeValue { get; set; }
        public bool IsAvailable { get; set; } = true;
        public byte[]? PhotoData { get; set; }
        public string? DocumentName { get; set; }   // nama file asli
        public string? DocumentContentType { get; set; } // mime type
        public byte[]? DocumentData { get; set; }   // isi file (binary)
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public bool IsSelected { get; set; } // untuk checkbox

    }

}
