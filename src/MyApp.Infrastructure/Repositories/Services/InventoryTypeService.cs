using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Infrastructure.Data
{
    public class InventoryTypeRepository : IInventoryTypeRepository
    {
        private readonly AppDbContext _context;

        public InventoryTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryType?> GetByIdAsync(int id) =>
            await _context.InventoryTypes.FindAsync(id);

        public async Task<List<InventoryType>> GetAllAsync() =>
            await _context.InventoryTypes.ToListAsync();

        public async Task AddAsync(InventoryType InventoryType)
        {
            await _context.InventoryTypes.AddAsync(InventoryType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InventoryType InventoryType)
        {
            var existing = await _context.InventoryTypes.FindAsync(InventoryType.Id);
            if (existing != null)
            {
                existing.TypeName = InventoryType.TypeName;
                existing.Details = InventoryType.Details;
                existing.IsActive = InventoryType.IsActive;
                await _context.SaveChangesAsync();
            }
        }


        public async Task DeleteAsync(int id)
        {
            var InventoryType = await _context.InventoryTypes.FindAsync(id);
            if (InventoryType != null)
            {
                _context.InventoryTypes.Remove(InventoryType);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
