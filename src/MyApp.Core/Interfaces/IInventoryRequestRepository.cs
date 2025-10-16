using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IInventoryRequestRepository
{
    Task<InventoryRequest?> GetByIdAsync(int id);
    Task<List<InventoryRequest>> GetAllAsync();
    Task AddAsync(InventoryRequest Request);
    Task UpdateAsync(InventoryRequest Request);
    Task DeleteAsync(int id);
    // Quick helpers
    Task<List<InventoryRequest>> GetActiveRequestsAsync();
}
