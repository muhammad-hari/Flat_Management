
using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IRoomCategoryRepository
{
    Task<List<RoomCategory>> GetAllAsync();
    Task<RoomCategory?> GetByIdAsync(int id);
    Task AddAsync(RoomCategory entity);
    Task UpdateAsync(RoomCategory entity);
    Task DeleteAsync(int id);
    Task<int> SaveChangesAsync();
}
