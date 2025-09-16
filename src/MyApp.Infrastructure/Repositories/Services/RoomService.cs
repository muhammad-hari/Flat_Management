using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(int id) =>
        await _context.Rooms.FindAsync(id);

    public async Task<List<Room>> GetAllAsync() =>
        await _context.Rooms.ToListAsync();

    public async Task AddAsync(Room Room)
    {
        await _context.Rooms.AddAsync(Room);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room Room)
    {
        _context.Rooms.Update(Room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var Room = await _context.Rooms.FindAsync(id);
        if (Room != null)
        {
            _context.Rooms.Remove(Room);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Room>> GetAllWithRelationsAsync()
    {
        return await _context.Rooms
            .Include(r => r.Category)   // relasi ke RoomCategory
            .Include(r => r.Status)     // relasi ke RoomStatus
            .Include(r => r.Condition)  // relasi ke RoomCondition
            .AsNoTracking()
            .ToListAsync();
    }


}
