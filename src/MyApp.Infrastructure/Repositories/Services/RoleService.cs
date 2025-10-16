using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Infrastructure.Data
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(int id) =>
            await _context.Roles.FindAsync(id);

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
        }


        public async Task<List<Role>> GetAllAsync() =>
            await _context.Roles.ToListAsync();

        public async Task AddAsync(Role Role)
        {
            await _context.Roles.AddAsync(Role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Role Role)
        {
            var existing = await _context.Roles.FindAsync(Role.Id);
            if (existing != null)
            {
                existing.RoleName = Role.RoleName;
                existing.Details = Role.Details;
                existing.IsActive = Role.IsActive;
                await _context.SaveChangesAsync();
            }
        }


        public async Task DeleteAsync(int id)
        {
            var Role = await _context.Roles.FindAsync(id);
            if (Role != null)
            {
                _context.Roles.Remove(Role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
