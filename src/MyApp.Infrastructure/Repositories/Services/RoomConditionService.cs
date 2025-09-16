using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Infrastructure.Data
{
    public class RoomConditionRepository : IRoomConditionRepository
    {
        private readonly AppDbContext _context;

        public RoomConditionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RoomCondition?> GetByIdAsync(int id) =>
            await _context.RoomConditions.FindAsync(id);

        public async Task<List<RoomCondition>> GetAllAsync() =>
            await _context.RoomConditions.ToListAsync();

        public async Task AddAsync(RoomCondition RoomCondition)
        {
            await _context.RoomConditions.AddAsync(RoomCondition);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RoomCondition RoomCondition)
        {
            var existing = await _context.RoomConditions.FindAsync(RoomCondition.Id);
            if (existing != null)
            {
                existing.Name = RoomCondition.Name;
                existing.Details = RoomCondition.Details;
                existing.IsActive = RoomCondition.IsActive;
                await _context.SaveChangesAsync();
            }
        }


        public async Task DeleteAsync(int id)
        {
            var RoomCondition = await _context.RoomConditions.FindAsync(id);
            if (RoomCondition != null)
            {
                _context.RoomConditions.Remove(RoomCondition);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
