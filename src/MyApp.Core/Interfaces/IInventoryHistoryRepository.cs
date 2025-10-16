using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IInventoryHistoryRepository
{
    Task<InventoryHistory?> GetByIdAsync(int id);
    Task<List<InventoryHistory>> GetAllAsync();
    Task AddAsync(InventoryHistory Request);
    Task UpdateAsync(InventoryHistory Request);
    Task DeleteAsync(int id);
    // Quick helpers
    Task<List<InventoryHistory>> GetActiveRequestsAsync();
}
