using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Infrastructure.Data
{
    public class RoomStatusRepository : IRoomStatusRepository
    {
        private readonly AppDbContext _context;

        public RoomStatusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RoomStatus?> GetByIdAsync(int id) =>
            await _context.RoomStatus.FindAsync(id);

        public async Task<List<RoomStatus>> GetAllAsync() =>
            await _context.RoomStatus.ToListAsync();

        public async Task AddAsync(RoomStatus RoomStatus)
        {
            await _context.RoomStatus.AddAsync(RoomStatus);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RoomStatus RoomStatus)
        {
            var existing = await _context.RoomStatus.FindAsync(RoomStatus.Id);
            if (existing != null)
            {
                existing.Name = RoomStatus.Name;
                existing.Details = RoomStatus.Details;
                existing.IsActive = RoomStatus.IsActive;
                await _context.SaveChangesAsync();
            }
        }


        public async Task DeleteAsync(int id)
        {
            var RoomStatus = await _context.RoomStatus.FindAsync(id);
            if (RoomStatus != null)
            {
                _context.RoomStatus.Remove(RoomStatus);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
