using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Infrastructure.Data
{
    public class RoomCategoryRepository : IRoomCategoryRepository
    {
        private readonly AppDbContext _context;

        public RoomCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RoomCategory?> GetByIdAsync(int id) =>
            await _context.RoomCategories.FindAsync(id);

        public async Task<List<RoomCategory>> GetAllAsync() =>
            await _context.RoomCategories.ToListAsync();

        public async Task AddAsync(RoomCategory RoomCategory)
        {
            await _context.RoomCategories.AddAsync(RoomCategory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RoomCategory RoomCategory)
        {
            var existing = await _context.RoomCategories.FindAsync(RoomCategory.Id);
            if (existing != null)
            {
                existing.Name = RoomCategory.Name;
                existing.Details = RoomCategory.Details;
                existing.IsActive = RoomCategory.IsActive;
                await _context.SaveChangesAsync();
            }
        }


        public async Task DeleteAsync(int id)
        {
            var RoomCategory = await _context.RoomCategories.FindAsync(id);
            if (RoomCategory != null)
            {
                _context.RoomCategories.Remove(RoomCategory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
