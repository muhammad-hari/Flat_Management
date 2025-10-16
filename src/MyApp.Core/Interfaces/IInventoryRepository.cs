using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IInventoryRepository
{
    Task<Inventory?> GetByIdAsync(int id);
    Task<List<Inventory>> GetAllAsync();
    Task AddAsync(Inventory inventory);
    Task UpdateAsync(Inventory inventory);
    Task DeleteAsync(int id);
}
