using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IBuildingRepository
{
    Task<Building?> GetByIdAsync(int id);
    Task<List<Building>> GetAllAsync();
    Task AddAsync(Building Building);
    Task UpdateAsync(Building Building);
    Task DeleteAsync(int id);
}
