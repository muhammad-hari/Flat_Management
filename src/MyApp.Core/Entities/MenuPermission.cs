namespace MyApp.Core.Entities
{
    public class MenuPermission
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int RoleId { get; set; }
        public bool CanView { get; set; } = false;
        public bool CanCreate { get; set; } = false;
        public bool CanUpdate { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Menu Menu { get; set; } = null!;
    }
}