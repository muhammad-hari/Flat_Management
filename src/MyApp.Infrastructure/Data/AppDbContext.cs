using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;

namespace MyApp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Position> Positions { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<UserType> UserTypes { get; set; } = default!;
        public DbSet<Rank> Ranks { get; set; } = default!;
        public DbSet<Employee> Employees { get; set; } = default!;
        public DbSet<Room> Rooms { get; set; } = default!;
        public DbSet<RoomCategory> RoomCategories { get; set; } = default!;
        public DbSet<RoomStatus> RoomStatus { get; set; } = default!;
        public DbSet<RoomCondition> RoomConditions { get; set; } = default!;
        public DbSet<Occupant> Occupants { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Atur kolom binary agar bisa menampung file besar
            modelBuilder.Entity<Occupant>()
                .Property(o => o.DocumentData)
                .HasColumnType("LONGBLOB");

            modelBuilder.Entity<Occupant>()
                .Property(o => o.PhotoData)
                .HasColumnType("LONGBLOB");
        }
    }
}
