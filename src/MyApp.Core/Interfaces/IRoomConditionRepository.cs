
using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IRoomConditionRepository
{
    Task<List<RoomCondition>> GetAllAsync();
    Task<RoomCondition?> GetByIdAsync(int id);
    Task AddAsync(RoomCondition entity);
    Task UpdateAsync(RoomCondition entity);
    Task DeleteAsync(int id);
    Task<int> SaveChangesAsync();
}
