namespace MyApp.Core.Entities
{
    public class Menu
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? IconName { get; set; }
        public string? Color { get; set; }
        public string? Url { get; set; }
        public int? ParentId { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Menu? Parent { get; set; }
        public ICollection<Menu> Children { get; set; } = new List<Menu>();
        public ICollection<MenuPermission> MenuPermissions { get; set; } = new List<MenuPermission>();
    }
}