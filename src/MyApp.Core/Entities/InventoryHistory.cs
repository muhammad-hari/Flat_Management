using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Core.Entities
{
    public class InventoryHistory
    {
        public int Id { get; set; }
        public string? ItemName { get; set; }
        public string? Code { get; set; }
        public int? InventoryId { get; set; }
        public Inventory? Inventory { get; set; }
        public string? Description { get; set; }
        public string? Action { get; set; }
        public string? BorrowerName { get; set; }
        public string? PerformedBy { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
    }

}
