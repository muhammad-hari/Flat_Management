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
                // Seed default system settings
                await SeedDefaultSystemSettings(context);

                // Prepare all menu seeders
                var allMenusToSeed = new List<Menu>
                {
                    new() { Code = "dashboard", Name = "Dashboard", IconName = "layout-dashboard", Color = "text-yellow-400", Url = "/dashboard", Order = 1 },
                    new() { Code = "administration", Name = "Administration", IconName = "users", Color = "text-blue-400", Order = 2 },
                    new() { Code = "occupants-management", Name = "Occupants Management", IconName = "home", Color = "text-orange-400", Order = 3 },
                    new() { Code = "visitors-management", Name = "Visitors Management", IconName = "user-check", Color = "text-orange-400", Order = 4 },
                    new() { Code = "employees", Name = "Employees", IconName = "briefcase", Color = "text-cyan-400", Url = "/employees", Order = 5 },
                    new() { Code = "building-management", Name = "Building Management", IconName = "building", Color = "text-yellow-400", Order = 6 },
                    new() { Code = "weapon-management", Name = "Weapon Management", IconName = "shield", Color = "text-red-400", Order = 7 },
                    new() { Code = "alsus-management", Name = "Alsus Management", IconName = "shield", Color = "text-red-400", Order = 8 },
                    new() { Code = "inventory-management", Name = "Inventory Management", IconName = "package", Color = "text-indigo-400", Order = 9 },
                    new() { Code = "access-control", Name = "Access Control", IconName = "key", Color = "text-emerald-400", Order = 10 },
                    new() { Code = "access-menu", Name = "Access Menu", IconName = "key", Color = "text-emerald-400", Url = "/access-menu", Order = 11 },
                    new() { Code = "master-data", Name = "Master Data", IconName = "folder-open", Color = "text-amber-400", Order = 12 },
                    new() { Code = "data-care", Name = "Data Care", IconName = "folder-open", Color = "text-amber-400", Order = 13 }
                };

                // Get existing menu codes from DB
                var existingMenuCodes = await context.Menus.Select(m => m.Code).ToListAsync();

                // Find missing parent menus
                var missingMenus = allMenusToSeed.Where(m => !existingMenuCodes.Contains(m.Code)).ToList();
                if (missingMenus.Any())
                {
                    await context.Menus.AddRangeAsync(missingMenus);
                    await context.SaveChangesAsync();
                }

                // Submenus seeder logic
                async Task SeedSubmenus(string parentCode, List<Menu> submenus)
                {
                    var parent = await context.Menus.FirstOrDefaultAsync(m => m.Code == parentCode);
                    if (parent == null) return;
                    var existingSubCodes = await context.Menus.Where(m => m.ParentId == parent.Id).Select(m => m.Code).ToListAsync();
                    var missingSubs = submenus.Where(m => !existingSubCodes.Contains(m.Code)).ToList();
                    foreach (var sub in missingSubs)
                    {
                        sub.ParentId = parent.Id;
                    }
                    if (missingSubs.Any())
                    {
                        await context.Menus.AddRangeAsync(missingSubs);
                        await context.SaveChangesAsync();
                    }
                }

                // Occupants Management
                await SeedSubmenus("occupants-management", new List<Menu>
                {
                    new() { Code = "occupants", Name = "Occupants", IconName = "calendar", Color = "text-teal-400", Url = "/occupants", Order = 1 },
                    new() { Code = "history", Name = "Histories", IconName = "calendar", Color = "text-teal-400", Url = "/occupant-history", Order = 2 }
                });

                // Weapon Management
                await SeedSubmenus("weapon-management", new List<Menu>
                {
                    new() { Code = "weapons", Name = "Weapons", IconName = "calendar", Color = "text-teal-400", Url = "/weapons", Order = 1 },
                    new() { Code = "weapons-assignment", Name = "Weapon Assignment", IconName = "calendar", Color = "text-teal-400", Url = "/weapons/assignment", Order = 2 }
                });

                // Alsus Management
                await SeedSubmenus("alsus-management", new List<Menu>
                {
                    new() { Code = "alsus", Name = "Alsus", IconName = "calendar", Color = "text-teal-400", Url = "/alsus", Order = 1 },
                    new() { Code = "alsus-assignment", Name = "Alsus Assignment", IconName = "calendar", Color = "text-teal-400", Url = "/alsus/assignment", Order = 2 }
                });

                // Administration
                await SeedSubmenus("administration", new List<Menu>
                {
                    new() { Code = "users", Name = "Users", IconName = "users", Color = "text-teal-400", Url = "/users", Order = 1 },
                    new() { Code = "role-permissions", Name = "Roles & Permissions", IconName = "shield", Color = "text-teal-400", Url = "/role-permission", Order = 2 },
                    new() { Code = "system-settings", Name = "System Settings", IconName = "cogs", Color = "text-teal-400", Url = "/system-settings", Order = 4 }
                });

                // Access Control
                await SeedSubmenus("access-control", new List<Menu>
                {
                    new() { Code = "handle-locks", Name = "Handle Lock Access", IconName = "shield", Color = "text-teal-400", Url = "/access-control/handle-locks", Order = 1 },
                    new() { Code = "gates", Name = "Gate Access", IconName = "shield", Color = "text-teal-400", Url = "/access-control/gates", Order = 2 }
                });

                // Data Care
                await SeedSubmenus("data-care", new List<Menu>
                {
                    new() { Code = "backup-data", Name = "Backup", IconName = "user-check", Color = "text-teal-400", Url = "/data-care/backup-data", Order = 1 },
                    new() { Code = "restore-data", Name = "Restore", IconName = "calendar", Color = "text-teal-400", Url = "/data-care/restore-data", Order = 2 }
                });

                // Visitors Management
                await SeedSubmenus("visitors-management", new List<Menu>
                {
                    new() { Code = "visitors", Name = "Visitors", IconName = "user-check", Color = "text-teal-400", Url = "/visitors", Order = 1 },
                    new() { Code = "visitor-analytics", Name = "Analytics", IconName = "calendar", Color = "text-teal-400", Url = "/visitor-analytics", Order = 2 }
                });

                // Building Management
                await SeedSubmenus("building-management", new List<Menu>
                {
                    new() { Code = "buildings", Name = "Building", IconName = "building", Color = "text-teal-400", Url = "/master-data/buildings", Order = 1 },
                    new() { Code = "rooms", Name = "Rooms", IconName = "settings", Color = "text-pink-400", Url = "/rooms", Order = 2 },
                    new() { Code = "maintenance-requests", Name = "Maintenance Requests", IconName = "clipboard-list", Color = "text-teal-400", Url = "/maintenance-requests", Order = 3 },
                    new() { Code = "vendors", Name = "Vendors", IconName = "truck", Color = "text-teal-400", Url = "/vendors", Order = 4 }
                });

                // Inventory Management
                await SeedSubmenus("inventory-management", new List<Menu>
                {
                    new() { Code = "inventories", Name = "Inventories", IconName = "package", Color = "text-teal-400", Url = "/inventories", Order = 1 },
                    new() { Code = "request-inventory", Name = "Request Item", IconName = "clipboard-list", Color = "text-teal-400", Url = "/request-inventory", Order = 2 },
                    new() { Code = "inventory-history", Name = "Histories", IconName = "clipboard-list", Color = "text-teal-400", Url = "/inventory-history", Order = 3 }
                });

                // Master Data
                await SeedSubmenus("master-data", new List<Menu>
                {
                    new() { Code = "position", Name = "Position", IconName = "briefcase", Color = "text-teal-400", Url = "/master-data/position", Order = 1 },
                    new() { Code = "role", Name = "Role", IconName = "shield", Color = "text-teal-400", Url = "/master-data/role", Order = 2 },
                    new() { Code = "user-type", Name = "User Type", IconName = "user-circle", Color = "text-teal-400", Url = "/master-data/user-type", Order = 3 },
                    new() { Code = "rank", Name = "Rank", IconName = "rank", Color = "text-teal-400", Url = "/master-data/rank", Order = 4 },
                    new() { Code = "room-category", Name = "Room Category", IconName = "building", Color = "text-teal-400", Url = "/master-data/room-category", Order = 5 },
                    new() { Code = "room-status", Name = "Room Status", IconName = "building", Color = "text-teal-400", Url = "/master-data/room-status", Order = 6 },
                    new() { Code = "room-condition", Name = "Room Condition", IconName = "building", Color = "text-teal-400", Url = "/master-data/room-condition", Order = 7 },
                    new() { Code = "inventory-type", Name = "Inventory Type", IconName = "user-circle", Color = "text-teal-400", Url = "/master-data/inventory-type", Order = 8 },
                    new() { Code = "repositories", Name = "Repository", IconName = "user-circle", Color = "text-teal-400", Url = "/master-data/repository", Order = 8 }
                });

                // Seed permissions for all roles
                await SeedPermissionsAsync(context, roleManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding menus: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedDefaultSystemSettings(AppDbContext context)
        {
            try
            {
                // Check if settings already exist
                var existingSettings = await context.SystemSettings.AnyAsync();
                if (existingSettings)
                {
                    Console.WriteLine("System settings already exist");
                    return;
                }

                Console.WriteLine("Seeding default system settings...");

                var defaultSettings = new List<SystemSetting>
                {
                    new SystemSetting
                    {
                        Key = "SiteName",
                        Value = "Flat Management System",
                        Description = "The name of the application displayed in the header and title",
                        CreatedAt = DateTime.Now
                    }
                };

                await context.SystemSettings.AddRangeAsync(defaultSettings);
                await context.SaveChangesAsync();

                Console.WriteLine("Default system settings seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding system settings: {ex.Message}");
            }
        }

        private static async Task SeedPermissionsAsync(
            AppDbContext context,
            RoleManager<IdentityRole<int>> roleManager)
        {
            try
            {
                // Get all menus from DB
                var allMenus = await context.Menus.ToListAsync();

                // Get all roles to seed permissions for
                var roles = new[] { "Admin", "Manager" };

                foreach (var roleName in roles)
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role == null) continue;

                    // Get existing permissions for this role
                    var existingPermissions = await context.MenuPermissions
                        .Where(mp => mp.RoleId == role.Id)
                        .Select(mp => mp.MenuId)
                        .ToListAsync();

                    // Find menus that do not have permissions for this role
                    var missingMenuIds = allMenus
                        .Where(m => !existingPermissions.Contains(m.Id))
                        .Select(m => m.Id)
                        .ToList();

                    if (missingMenuIds.Any())
                    {
                        var newPermissions = new List<MenuPermission>();
                        foreach (var menuId in missingMenuIds)
                        {
                            var menu = allMenus.First(m => m.Id == menuId);

                            // For Manager, skip "users" menu
                            if (roleName == "Manager" && menu.Code == "users")
                                continue;

                            newPermissions.Add(new MenuPermission
                            {
                                MenuId = menuId,
                                RoleId = role.Id,
                                CanView = true,
                                CanCreate = true,
                                CanUpdate = true,
                                CanDelete = true
                            });
                        }

                        if (newPermissions.Any())
                        {
                            await context.MenuPermissions.AddRangeAsync(newPermissions);
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding permissions: {ex.Message}");
                throw;
            }
        }

        public static async Task SeedMasterDataAsync(AppDbContext context)
        {
            // Seed Positions
            if (!await context.Positions.AnyAsync())
            {
                var positions = new List<Position>
                {
                    new() { Name = "Kapolres", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true },
                    new() {Name = "Wakapolres", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kasat Reskrim", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kasat Intelkam", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kasat Lantas", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kasat Sabhara", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kasat Binmas", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kanit Reskrim", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kanit Intelkam", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kanit Lantas", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kanit Sabhara", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kanit Binmas", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Bhabinkamtibmas", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Operator", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true}
                };
                await context.Positions.AddRangeAsync(positions);
            }

            // Seed Ranks
            if (!await context.Ranks.AnyAsync())
            {
                var ranks = new List<Rank>
                {
                    new() {Name = "Jenderal Polisi", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Komjen Polisi", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Irjen Polisi", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Brigjen Polisi", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kombes Polisi", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "AKBP", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Kompol", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "AKP", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Iptu", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Ipda", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Aiptu", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Aipda", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Bripka", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Brigadir", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Briptu", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Bripda", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true}
                };
                await context.Ranks.AddRangeAsync(ranks);
            }

            // Seed Room Categories
            if (!await context.RoomCategories.AnyAsync())
            {
                var categories = new List<RoomCategory>
                {
                    new() {Name = "Ruang Kapolres", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() { Name = "Ruang Wakapolres" , CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() { Name = "Ruang Sat Reskrim", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true },
                    new() {Name = "Ruang Sat Intelkam", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Ruang Sat Lantas", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Ruang Sat Sabhara", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Ruang Sat Binmas", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Ruang Tahanan", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Ruang Rapat", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Ruang Arsip", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Ruang Senjata", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Ruang Inventaris", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true}
                };
                await context.RoomCategories.AddRangeAsync(categories);
            }

            // Seed Room Status
            if (!await context.RoomStatus.AnyAsync())
            {
                var status = new List<RoomStatus>
                {
                    new() {Name = "Tersedia", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Terpakai", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Dalam Perbaikan", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {Name = "Tidak Layak", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true}
                };
                await context.RoomStatus.AddRangeAsync(status);
            }

            // Seed Room Condition
            if (!await context.RoomConditions.AnyAsync())
            {
                var conditions = new List<RoomCondition>
                {
                    new() { Name = "Baik", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true },
                    new() { Name = "Cukup", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true },
                    new() { Name = "Rusak Ringan", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true },
                    new() {Name = "Rusak Berat", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true}
                };
                await context.RoomConditions.AddRangeAsync(conditions);
            }

            // Seed Inventory Types
            if (!await context.InventoryTypes.AnyAsync())
            {
                var inventoryTypes = new List<InventoryType>
                {
                    new() { TypeName = "Senjata Api", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true },
                    new() {TypeName = "Amunisi", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {TypeName = "Kendaraan Dinas", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {TypeName = "Peralatan Komunikasi", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {TypeName = "Peralatan Kantor", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {TypeName = "Peralatan IT", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {TypeName = "Dokumen", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true},
                    new() {TypeName = "Seragam", CreatedBy = "system", CreatedAt = DateTime.UtcNow, IsActive = true}
                };
                await context.InventoryTypes.AddRangeAsync(inventoryTypes);
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedEmployeesAsync(AppDbContext context)
        {
            if (!await context.Employees.AnyAsync())
            {
                var ranks = await context.Ranks.ToListAsync();
                var positions = await context.Positions.ToListAsync();

                var employees = new List<Employee>
                {
                    new()
                    {
                        Name = "AKBP Budi Santoso",
                        NRP = "76010101",
                        Gender = "Laki-laki",
                        Email = "budi.santoso@polri.go.id",
                        DateOfBirth = new DateOnly(1976, 1, 1),
                        Status = "Aktif",
                        RankId = ranks.FirstOrDefault(r => r.Name == "AKBP")?.Id ?? ranks.First().Id,
                        PositionId = positions.FirstOrDefault(p => p.Name == "Kapolres")?.Id ?? positions.First().Id,
                        Address = "Jl. Sudirman No. 1, Jakarta",
                        Phone = "081234567890",
                        JoinDate = new DateOnly(2000, 1, 1),
                        IsActive = true,
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Position = positions.FirstOrDefault(p => p.Name == "Kapolres"),
                        Rank = ranks.FirstOrDefault(r => r.Name == "AKBP"),
                    },
                    new()
                    {
                        Name = "Kompol Siti Aminah",
                        NRP = "80020202",
                        Gender = "Perempuan",
                        Email = "siti.aminah@polri.go.id",
                        DateOfBirth = new DateOnly(1980, 2, 2),
                        Status = "Aktif",
                        RankId = ranks.FirstOrDefault(r => r.Name == "Kompol")?.Id ?? ranks.First().Id,
                        PositionId = positions.FirstOrDefault(p => p.Name == "Kasat Reskrim")?.Id ?? positions.First().Id,
                        Address = "Jl. Thamrin No. 2, Bandung",
                        Phone = "081298765432",
                        JoinDate = new DateOnly(2005, 2, 2),
                        IsActive = true,
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Position = positions.FirstOrDefault(p => p.Name == "Kasat Reskrim"),
                        Rank = ranks.FirstOrDefault(r => r.Name == "Kompol"),
                    },
                    new()
                    {
                        Name = "AKP Andi Wijaya",
                        NRP = "85030303",
                        Gender = "Laki-laki",
                        Email = "andi.wijaya@polri.go.id",
                        DateOfBirth = new DateOnly(1985, 3, 3),
                        Status = "Aktif",
                        RankId = ranks.FirstOrDefault(r => r.Name == "AKP")?.Id ?? ranks.First().Id,
                        PositionId = positions.FirstOrDefault(p => p.Name == "Kasat Intelkam")?.Id ?? positions.First().Id,
                        Address = "Jl. Merdeka No. 3, Surabaya",
                        Phone = "081212345678",
                        JoinDate = new DateOnly(2010, 3, 3),
                        IsActive = true,
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Position = positions.FirstOrDefault(p => p.Name == "Kasat Intelkam"),
                        Rank = ranks.FirstOrDefault(r => r.Name == "AKP"),
                    },
                    new()
                    {
                        Name = "Iptu Rina Dewi",
                        NRP = "90040404",
                        Gender = "Perempuan",
                        Email = "rina.dewi@polri.go.id",
                        DateOfBirth = new DateOnly(1990, 4, 4),
                        Status = "Aktif",
                        RankId = ranks.FirstOrDefault(r => r.Name == "Iptu")?.Id ?? ranks.First().Id,
                        PositionId = positions.FirstOrDefault(p => p.Name == "Kanit Lantas")?.Id ?? positions.First().Id,
                        Address = "Jl. Diponegoro No. 4, Semarang",
                        Phone = "081223344556",
                        JoinDate = new DateOnly(2015, 4, 4),
                        IsActive = true,
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Position = positions.FirstOrDefault(p => p.Name == "Kanit Lantas"),
                        Rank = ranks.FirstOrDefault(r => r.Name == "Iptu"),
                    },
                    new()
                    {
                        Name = "Aiptu Joko Prasetyo",
                        NRP = "95050505",
                        Gender = "Laki-laki",
                        Email = "joko.prasetyo@polri.go.id",
                        DateOfBirth = new DateOnly(1995, 5, 5),
                        Status = "Aktif",
                        RankId = ranks.FirstOrDefault(r => r.Name == "Aiptu")?.Id ?? ranks.First().Id,
                        PositionId = positions.FirstOrDefault(p => p.Name == "Bhabinkamtibmas")?.Id ?? positions.First().Id,
                        Address = "Jl. Gajah Mada No. 5, Yogyakarta",
                        Phone = "081234567891",
                        JoinDate = new DateOnly(2020, 5, 5),
                        IsActive = true,
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Position = positions.FirstOrDefault(p => p.Name == "Bhabinkamtibmas"),
                        Rank = ranks.FirstOrDefault(r => r.Name == "Aiptu"),
                    }
                };

                await context.Employees.AddRangeAsync(employees);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedWeaponsAsync(AppDbContext context)
        {
            if (!await context.Weapons.AnyAsync())
            {
                var weapons = new List<Weapon>
                {
                    new()
                    {
                        Name = "Pistol Glock 17",
                        Code = "GLK17-001",
                        Description = "Senjata api standar Polri, kaliber 9mm.",
                        IsAvailable = true,
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        DocumentName = "Glock17_Manual.pdf",
                        DocumentContentType = "application/pdf",
                    },
                    new()
                    {
                        Name = "Revolver S&W",
                        Code = "SW-002",
                        Description = "Senjata api revolver Smith & Wesson, kaliber .38.",
                        IsAvailable = true,
                        CreatedBy = "system",
                        DocumentName = "SW_Revolver_Manual.pdf",
                        DocumentContentType = "application/pdf",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Name = "Senapan SS1-V1",
                        Code = "SS1-003",
                        Description = "Senapan serbu SS1-V1, kaliber 5.56mm.",
                        IsAvailable = true,
                        CreatedBy = "system",
                        DocumentName = "SS1V1_Manual.pdf",
                        DocumentContentType = "application/pdf",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Name = "Shotgun Remington 870",
                        Code = "REM870-004",
                        Description = "Senjata api shotgun Remington 870.",
                        IsAvailable = true,
                        DocumentContentType = "application/pdf",
                        DocumentName = "Remington870_Manual.pdf",
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Name = "Pistol HS-9",
                        Code = "HS9-005",
                        Description = "Pistol HS-9, kaliber 9mm.",
                        DocumentName = "HS9_Specifications.pdf",
                        IsAvailable = true,
                        DocumentContentType = "application/pdf",
                        CreatedBy = "system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.Weapons.AddRangeAsync(weapons);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedAlsusAsync(AppDbContext context)
        {
            if (!await context.Alsuses.AnyAsync())
            {
                var alsusList = new List<Alsus>
                {
                    new() { Name = "Rompi Anti Peluru", Code = "ALSUS-001", Description = "Rompi pelindung tubuh dari peluru.", IsAvailable = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Name = "Tameng Polisi", Code = "ALSUS-002", Description = "Tameng pelindung untuk pengendalian massa.", IsAvailable = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Name = "Helm Taktis", Code = "ALSUS-003", Description = "Helm pelindung kepala untuk operasi khusus.", IsAvailable = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Name = "Gas Mask", Code = "ALSUS-004", Description = "Masker pelindung dari gas berbahaya.", IsAvailable = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Name = "Baton Polisi", Code = "ALSUS-005", Description = "Tongkat polisi untuk pengamanan.", IsAvailable = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Name = "Handcuff", Code = "ALSUS-006", Description = "Borgol untuk penahanan tersangka.", IsAvailable = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Name = "Flashlight Taktis", Code = "ALSUS-007", Description = "Senter taktis untuk operasi malam.", IsAvailable = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Name = "Megaphone", Code = "ALSUS-008", Description = "Alat pengeras suara untuk komunikasi massa.", IsAvailable = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Name = "Body Camera", Code = "ALSUS-009", Description = "Kamera yang dipasang di tubuh untuk dokumentasi.", IsAvailable = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Name = "Drone Surveillance", Code = "ALSUS-010", Description = "Drone untuk pemantauan area.", IsAvailable = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };

                await context.Alsuses.AddRangeAsync(alsusList);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedRepositoriesAsync(AppDbContext context)
        {
            if (!await context.Repositories.AnyAsync())
            {
                var repositories = new List<Repository>
                {
                    new() { Name = "Repository Intelijen", Details = "Data dan dokumen intelijen kepolisian", IsActive = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                    new() { Name = "Repository Reskrim", Details = "Dokumen kasus dan penyidikan kriminal", IsActive = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                    new() { Name = "Repository Lantas", Details = "Data pelanggaran dan kecelakaan lalu lintas", IsActive = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                    new() { Name = "Repository Sabhara", Details = "Dokumen pengamanan dan patroli Sabhara", IsActive = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                    new() { Name = "Repository Binmas", Details = "Data pembinaan masyarakat dan kegiatan Binmas", IsActive = true, CreatedBy = "system", CreatedAt = DateTime.UtcNow }
                };

                await context.Repositories.AddRangeAsync(repositories);
                await context.SaveChangesAsync();
            }
        }
    }
}