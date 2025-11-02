using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Core.Entities;

namespace MyApp.Infrastructure.Data
{
    public static class MenuSeeder
    {
        public static async Task SeedMenusAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            try
            {
                // Check if menus already exist
                var existingMenus = await context.Menus.AnyAsync();
                if (existingMenus)
                {
                    Console.WriteLine("Menus already exist, checking permissions...");
                    
                    // Check if we need to seed permissions
                    await SeedPermissionsAsync(context, roleManager);
                    return;
                }

                Console.WriteLine("Starting menu seeding...");

                // Seed parent menus
                var menus = new List<Menu>
                {
                    new() { Code = "dashboard", Name = "Dashboard", IconName = "layout-dashboard", Color = "text-yellow-400", Url = "/dashboard", Order = 1 },
                    new() { Code = "users", Name = "Users", IconName = "users", Color = "text-blue-400", Url = "/users", Order = 2 },
                    new() { Code = "occupants-management", Name = "Occupants Management", IconName = "home", Color = "text-orange-400", Order = 3 },
                    new() { Code = "visitors-management", Name = "Visitors Management", IconName = "user-check", Color = "text-orange-400", Order = 4 },
                    new() { Code = "employees", Name = "Employees", IconName = "briefcase", Color = "text-cyan-400", Url = "/employees", Order = 5 },
                    new() { Code = "building-management", Name = "Building Management", IconName = "building", Color = "text-yellow-400", Order = 6 },
                    new() { Code = "weapon-alus", Name = "Weapon and Alus", IconName = "shield", Color = "text-red-400", Url = "/weapon-alsus", Order = 7 },
                    new() { Code = "inventory-management", Name = "Inventory Management", IconName = "package", Color = "text-indigo-400", Order = 8 },
                    new() { Code = "access-control", Name = "Access Control", IconName = "key", Color = "text-emerald-400", Url = "/access-control", Order = 9 },
                    new() { Code = "access-menu", Name = "Access Menu", IconName = "key", Color = "text-emerald-400", Url = "/access-menu", Order = 10 },
                    new() { Code = "master-data", Name = "Master Data", IconName = "folder-open", Color = "text-amber-400", Order = 11 }
                };

                await context.Menus.AddRangeAsync(menus);
                await context.SaveChangesAsync();
                Console.WriteLine("Parent menus seeded successfully");

                // Add submenus - Occupants Management
                var occupantsParent = await context.Menus.FirstAsync(m => m.Code == "occupants-management");
                var occupantsSubmenus = new List<Menu>
                {
                    new() { Code = "occupants", Name = "Occupants", IconName = "calendar", Color = "text-teal-400", Url = "/occupants", Order = 1, ParentId = occupantsParent.Id },
                    new() { Code = "history", Name = "Histories", IconName = "calendar", Color = "text-teal-400", Url = "/occupant-history", Order = 2, ParentId = occupantsParent.Id }
                };
                await context.Menus.AddRangeAsync(occupantsSubmenus);

                // Add submenus - Visitors Management
                var visitorsParent = await context.Menus.FirstAsync(m => m.Code == "visitors-management");
                var visitorsSubmenus = new List<Menu>
                {
                    new() { Code = "visitors", Name = "Visitors", IconName = "user-check", Color = "text-teal-400", Url = "/visitors", Order = 1, ParentId = visitorsParent.Id },
                    new() { Code = "visitor-analytics", Name = "Analytics", IconName = "calendar", Color = "text-teal-400", Url = "/visitor-analytics", Order = 2, ParentId = visitorsParent.Id }
                };
                await context.Menus.AddRangeAsync(visitorsSubmenus);

                // Add submenus - Building Management
                var buildingParent = await context.Menus.FirstAsync(m => m.Code == "building-management");
                var buildingSubmenus = new List<Menu>
                {
                    new() { Code = "buildings", Name = "Building", IconName = "building", Color = "text-teal-400", Url = "/master-data/buildings", Order = 1, ParentId = buildingParent.Id },
                    new() { Code = "rooms", Name = "Rooms", IconName = "settings", Color = "text-pink-400", Url = "/rooms", Order = 2, ParentId = buildingParent.Id },
                    new() { Code = "maintenance-requests", Name = "Maintenance Requests", IconName = "clipboard-list", Color = "text-teal-400", Url = "/maintenance-requests", Order = 3, ParentId = buildingParent.Id },
                    new() { Code = "vendors", Name = "Vendors", IconName = "truck", Color = "text-teal-400", Url = "/vendors", Order = 4, ParentId = buildingParent.Id }
                };
                await context.Menus.AddRangeAsync(buildingSubmenus);

                // Add submenus - Inventory Management
                var inventoryParent = await context.Menus.FirstAsync(m => m.Code == "inventory-management");
                var inventorySubmenus = new List<Menu>
                {
                    new() { Code = "inventories", Name = "Inventories", IconName = "package", Color = "text-teal-400", Url = "/inventories", Order = 1, ParentId = inventoryParent.Id },
                    new() { Code = "inventory-checks", Name = "Request Item", IconName = "clipboard-list", Color = "text-teal-400", Url = "/inventory-checks", Order = 2, ParentId = inventoryParent.Id },
                    new() { Code = "inventory-reports", Name = "Histories", IconName = "clipboard-list", Color = "text-teal-400", Url = "/inventory-reports", Order = 3, ParentId = inventoryParent.Id }
                };
                await context.Menus.AddRangeAsync(inventorySubmenus);

                // Add submenus - Master Data
                var masterDataParent = await context.Menus.FirstAsync(m => m.Code == "master-data");
                var masterDataSubmenus = new List<Menu>
                {
                    new() { Code = "position", Name = "Position", IconName = "briefcase", Color = "text-teal-400", Url = "/master-data/position", Order = 1, ParentId = masterDataParent.Id },
                    new() { Code = "role", Name = "Role", IconName = "shield", Color = "text-teal-400", Url = "/master-data/role", Order = 2, ParentId = masterDataParent.Id },
                    new() { Code = "user-type", Name = "User Type", IconName = "user-circle", Color = "text-teal-400", Url = "/master-data/user-type", Order = 3, ParentId = masterDataParent.Id },
                    new() { Code = "rank", Name = "Rank", IconName = "rank", Color = "text-teal-400", Url = "/master-data/rank", Order = 4, ParentId = masterDataParent.Id },
                    new() { Code = "room-category", Name = "Room Category", IconName = "building", Color = "text-teal-400", Url = "/master-data/room-category", Order = 5, ParentId = masterDataParent.Id },
                    new() { Code = "room-status", Name = "Room Status", IconName = "building", Color = "text-teal-400", Url = "/master-data/room-status", Order = 6, ParentId = masterDataParent.Id },
                    new() { Code = "room-condition", Name = "Room Condition", IconName = "building", Color = "text-teal-400", Url = "/master-data/room-condition", Order = 7, ParentId = masterDataParent.Id },
                    new() { Code = "inventory-type", Name = "Inventory Type", IconName = "user-circle", Color = "text-teal-400", Url = "/master-data/inventory-type", Order = 8, ParentId = masterDataParent.Id }
                };
                await context.Menus.AddRangeAsync(masterDataSubmenus);
                await context.SaveChangesAsync();

                // Seed permissions for all roles
                await SeedPermissionsAsync(context, roleManager);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static async Task SeedPermissionsAsync(
            AppDbContext context, 
            RoleManager<IdentityRole<int>> roleManager)
        {
            try
            {
                // Get all menus
                var allMenus = await context.Menus.ToListAsync();

                // ===== SEED ADMIN PERMISSIONS (Full Access) =====
                var adminRole = await roleManager.FindByNameAsync("Admin");
                if (adminRole != null)
                {
                    
                    // Check if Admin already has permissions
                    var existingAdminPermissions = await context.MenuPermissions
                        .Where(mp => mp.RoleId == adminRole.Id)
                        .CountAsync();

                    if (existingAdminPermissions == 0)
                    {
                        var adminPermissions = new List<MenuPermission>();
                        foreach (var menu in allMenus)
                        {
                            adminPermissions.Add(new MenuPermission
                            {
                                MenuId = menu.Id,
                                RoleId = adminRole.Id,
                                CanView = true,
                                CanCreate = true,
                                CanUpdate = true,
                                CanDelete = true
                            });
                        }

                        await context.MenuPermissions.AddRangeAsync(adminPermissions);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                    }
                }
                else
                {
                }

                // ===== SEED MANAGER PERMISSIONS (All except Users) =====
                var managerRole = await roleManager.FindByNameAsync("Manager");
                if (managerRole != null)
                {
                    
                    // Check if Manager already has permissions
                    var existingManagerPermissions = await context.MenuPermissions
                        .Where(mp => mp.RoleId == managerRole.Id)
                        .CountAsync();

                    if (existingManagerPermissions == 0)
                    {
                        var managerPermissions = new List<MenuPermission>();
                        foreach (var menu in allMenus)
                        {
                            // Skip "users" menu for Manager
                            if (menu.Code == "users")
                            {
                                continue;
                            }

                            managerPermissions.Add(new MenuPermission
                            {
                                MenuId = menu.Id,
                                RoleId = managerRole.Id,
                                CanView = true,
                                CanCreate = true,
                                CanUpdate = true,
                                CanDelete = true
                            });
                        }

                        await context.MenuPermissions.AddRangeAsync(managerPermissions);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}