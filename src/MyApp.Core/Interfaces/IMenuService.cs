using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces
{
    public interface IMenuService
    {
        Task<List<MenuDto>> GetMenusByUserRolesAsync(List<string> roles);
        Task<MenuPermissionDto?> GetMenuPermissionAsync(string menuCode, List<string> roles);
        Task<List<Menu>> GetAllMenusAsync();
        Task<bool> HasPermissionAsync(string menuCode, List<string> roles, string permissionType);
    }

    public class MenuDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? IconName { get; set; }
        public string? Color { get; set; }
        public string? Url { get; set; }
        public int Order { get; set; }
        public List<MenuDto> Children { get; set; } = new();
        public MenuPermissionDto Permissions { get; set; } = new();
    }

    public class MenuPermissionDto
    {
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
    }
}