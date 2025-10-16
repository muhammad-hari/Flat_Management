using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;

namespace MyApp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }


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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Atur kolom binary agar bisa menampung file besar
            modelBuilder.Entity<Occupant>()
                .Property(o => o.DocumentData)
                .HasColumnType("LONGBLOB");

            modelBuilder.Entity<Occupant>()
                .Property(o => o.PhotoData)
                .HasColumnType("LONGBLOB");

            // Pastikan semua Id auto increment
            modelBuilder.Entity<User>().Property(u => u.Id).ValueGeneratedOnAdd();
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
        }
    }
}
