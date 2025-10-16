namespace MyApp.Core.Entities
{
    public class InventoryRequest
    {
        public int Id { get; set; }

        // Foreign key to Inventory
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }

        // Request information
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        // Status: Requested, Borrowed, Returned, Cancelled
        public string Status { get; set; } = "Requested";
        public bool Returned => Status == "Returned";

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}

