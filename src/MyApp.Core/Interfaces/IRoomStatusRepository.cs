
using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IRoomStatusRepository
{
    Task<List<RoomStatus>> GetAllAsync();
    Task<RoomStatus?> GetByIdAsync(int id);
    Task AddAsync(RoomStatus entity);
    Task UpdateAsync(RoomStatus entity);
    Task DeleteAsync(int id);
    Task<int> SaveChangesAsync();
}
