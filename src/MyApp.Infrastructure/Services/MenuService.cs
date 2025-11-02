using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

namespace MyApp.Infrastructure.Services
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MenuService> _logger;

        public MenuService(AppDbContext context, ILogger<MenuService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<MenuDto>> GetMenusByUserRolesAsync(List<string> roles)
        {
            try
            {
                // Get role IDs from role names using IdentityRole
                var roleIds = await _context.Set<IdentityRole<int>>()
                    .Where(r => roles.Contains(r.Name))
                    .Select(r => r.Id)
                    .ToListAsync();

                if (!roleIds.Any())
                {
                    _logger.LogWarning("No matching roles found for user");
                    return new List<MenuDto>();
                }

                // Get menus with permissions
                var menus = await _context.Menus
                    .Include(m => m.MenuPermissions)
                    .Where(m => m.IsActive && m.ParentId == null)
                    .OrderBy(m => m.Order)
                    .ToListAsync();

                var menuDtos = new List<MenuDto>();

                foreach (var menu in menus)
                {
                    var permissions = GetHighestPermissions(menu.MenuPermissions, roleIds);
                    
                    if (permissions.CanView)
                    {
                        var menuDto = new MenuDto
                        {
                            Code = menu.Code,
                            Name = menu.Name,
                            IconName = menu.IconName,
                            Color = menu.Color,
                            Url = menu.Url,
                            Order = menu.Order,
                            Permissions = permissions
                        };

                        // Get children
                        menuDto.Children = await GetChildMenusAsync(menu.Id, roleIds);
                        menuDtos.Add(menuDto);
                    }
                }

                return menuDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting menus by user roles");
                return new List<MenuDto>();
            }
        }

        private async Task<List<MenuDto>> GetChildMenusAsync(int parentId, List<int> roleIds)
        {
            var children = await _context.Menus
                .Include(m => m.MenuPermissions)
                .Where(m => m.IsActive && m.ParentId == parentId)
                .OrderBy(m => m.Order)
                .ToListAsync();

            var childDtos = new List<MenuDto>();

            foreach (var child in children)
            {
                var permissions = GetHighestPermissions(child.MenuPermissions, roleIds);
                
                if (permissions.CanView)
                {
                    var childDto = new MenuDto
                    {
                        Code = child.Code,
                        Name = child.Name,
                        IconName = child.IconName,
                        Color = child.Color,
                        Url = child.Url,
                        Order = child.Order,
                        Permissions = permissions
                    };

                    childDto.Children = await GetChildMenusAsync(child.Id, roleIds);
                    childDtos.Add(childDto);
                }
            }

            return childDtos;
        }

        private MenuPermissionDto GetHighestPermissions(
            ICollection<MenuPermission> menuPermissions, 
            List<int> roleIds)
        {
            var userPermissions = menuPermissions
                .Where(mp => roleIds.Contains(mp.RoleId))
                .ToList();

            if (!userPermissions.Any())
            {
                return new MenuPermissionDto();
            }

            // Get highest permissions (OR operation)
            return new MenuPermissionDto
            {
                CanView = userPermissions.Any(p => p.CanView),
                CanCreate = userPermissions.Any(p => p.CanCreate),
                CanUpdate = userPermissions.Any(p => p.CanUpdate),
                CanDelete = userPermissions.Any(p => p.CanDelete)
            };
        }

        public async Task<MenuPermissionDto?> GetMenuPermissionAsync(string menuCode, List<string> roles)
        {
            try
            {
                var roleIds = await _context.Set<IdentityRole<int>>()
                    .Where(r => roles.Contains(r.Name))
                    .Select(r => r.Id)
                    .ToListAsync();

                if (!roleIds.Any())
                    return null;

                var menu = await _context.Menus
                    .Include(m => m.MenuPermissions)
                    .FirstOrDefaultAsync(m => m.Code == menuCode && m.IsActive);

                if (menu == null)
                    return null;

                return GetHighestPermissions(menu.MenuPermissions, roleIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting menu permission");
                return null;
            }
        }

        public async Task<List<Menu>> GetAllMenusAsync()
        {
            return await _context.Menus
                .Include(m => m.Children)
                .Include(m => m.MenuPermissions)
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.Order)
                .ToListAsync();
        }

        public async Task<bool> HasPermissionAsync(string menuCode, List<string> roles, string permissionType)
        {
            var permission = await GetMenuPermissionAsync(menuCode, roles);
            if (permission == null)
                return false;

            return permissionType.ToLower() switch
            {
                "view" => permission.CanView,
                "create" => permission.CanCreate,
                "update" => permission.CanUpdate,
                "delete" => permission.CanDelete,
                _ => false
            };
        }
    }
}