using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(int id);
    Task<List<Room>> GetAllAsync();
    Task AddAsync(Room Room);
    Task UpdateAsync(Room Room);
    Task DeleteAsync(int id);
    Task<List<Room>> GetAllWithRelationsAsync();
}
