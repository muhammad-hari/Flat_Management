using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Infrastructure.Identity;

namespace MyApp.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<MyApp.Core.Entities.ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Identity entities sudah otomatis dari IdentityDbContext
        public DbSet<FileUpload> Files { get; set; } = default!;
        public DbSet<Position> Positions { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<UserType> UserTypes { get; set; } = default!;
        public DbSet<Rank> Ranks { get; set; } = default!;
        public DbSet<Employee> Employees { get; set; } = default!;
        public DbSet<Building> Buildings { get; set; } = default!;
        public DbSet<Room> Rooms { get; set; } = default!;
        public DbSet<RoomCategory> RoomCategories { get; set; } = default!;
        public DbSet<RoomStatus> RoomStatus { get; set; } = default!;
        public DbSet<RoomCondition> RoomConditions { get; set; } = default!;
        public DbSet<Occupant> Occupants { get; set; } = default!;
        public DbSet<OccupantHistory> OccupantHistories { get; set; } = default!;
        public DbSet<Visitor> Visitors { get; set; } = default!;
        public DbSet<Vendor> Vendors { get; set; } = default!;
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; } = default!;
        public DbSet<InventoryType> InventoryTypes { get; set; } = default!;
        public DbSet<Repository> Repositories { get; set; } = default!;
        public DbSet<Inventory> Inventories { get; set; } = default!;
        public DbSet<InventoryRequest> InventoryRequests { get; set; } = default!;
        public DbSet<InventoryHistory> InventoryHistories { get; set; } = default!;
        public DbSet<Weapon> Weapons { get; set; } = default!;
        public DbSet<Alsus> Alsuses { get; set; } = default!;
        public DbSet<Area> Areas { get; set; } = default!;
        public DbSet<Card> Cards { get; set; } = default!;
        public DbSet<GateDevice> GateDevices { get; set; } = default!;
        // Menu Management
        public DbSet<Menu> Menus { get; set; } = default!;
        public DbSet<MenuPermission> MenuPermissions { get; set; } = default!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = default!;
        public DbSet<BackupSchedule> BackupSchedules { get; set; } = default!;
        public DbSet<BackupHistory> BackupHistories { get; set; } = default!;
        public DbSet<RestoreHistory> RestoreHistories { get; set; } = default!;
        public DbSet<AssignmentWeapon> AssignmentWeapons { get; set; } = default!;
        
        public DbSet<AssignmentAlsus> AssignmentAlsuses { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Atur kolom binary agar bisa menampung file besar
            modelBuilder.Entity<Occupant>()
                .Property(o => o.DocumentData)
                .HasColumnType("LONGBLOB");

            modelBuilder.Entity<Occupant>()
                .Property(o => o.PhotoData)
                .HasColumnType("LONGBLOB");

            // Pastikan semua Id auto increment
            modelBuilder.Entity<FileUpload>().Property(f => f.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Position>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Role>().Property(r => r.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UserType>().Property(t => t.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Rank>().Property(r => r.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Employee>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Building>().Property(b => b.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Room>().Property(r => r.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<RoomCategory>().Property(rc => rc.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<RoomStatus>().Property(rs => rs.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<RoomCondition>().Property(rc => rc.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Occupant>().Property(o => o.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<OccupantHistory>().Property(h => h.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Visitor>().Property(v => v.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Vendor>().Property(v => v.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<MaintenanceRequest>().Property(v => v.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<InventoryRequest>().Property(v => v.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Weapon>().Property(w => w.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Alsus>().Property(a => a.Id).ValueGeneratedOnAdd();

            // Menu Configuration
            modelBuilder.Entity<Menu>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.Code).IsUnique();

                entity.HasOne(e => e.Parent)
                    .WithMany(e => e.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // MenuPermission Configuration
            modelBuilder.Entity<MenuPermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasOne(e => e.Menu)
                    .WithMany(e => e.MenuPermissions)
                    .HasForeignKey(e => e.MenuId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.MenuId, e.RoleId }).IsUnique();
            });

            // SystemSetting Configuration
            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
                
                // Update: Change from 500 to TEXT/LONGTEXT for large data like base64 images
                entity.Property(e => e.Value)
                    .HasColumnType("LONGTEXT") // For MySQL: supports up to 4GB
                    .IsRequired(false);
                
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Key).IsUnique();
            });

            // BackupSchedule Configuration
            modelBuilder.Entity<BackupSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Frequency).HasMaxLength(50).IsRequired();
                entity.Property(e => e.BackupPath).HasMaxLength(500).IsRequired();
                
                entity.HasMany(e => e.BackupHistories)
                    .WithOne(e => e.BackupSchedule)
                    .HasForeignKey(e => e.BackupScheduleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // BackupHistory Configuration
            modelBuilder.Entity<BackupHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.BackupType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartedAt);
            });

            // RestoreHistory Configuration
            modelBuilder.Entity<RestoreHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartedAt);
            });

            // AssignmentWeapon Configuration
            modelBuilder.Entity<AssignmentWeapon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Weapon)
                    .WithMany()
                    .HasForeignKey(e => e.WeaponId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AssignmentAlsus Configuration
            modelBuilder.Entity<AssignmentAlsus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Alsus)
                    .WithMany()
                    .HasForeignKey(e => e.AlsusId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
