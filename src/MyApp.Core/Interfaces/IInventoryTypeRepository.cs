
using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IInventoryTypeRepository
{
    Task<List<InventoryType>> GetAllAsync();
    Task<InventoryType?> GetByIdAsync(int id);
    Task AddAsync(InventoryType entity);
    Task UpdateAsync(InventoryType entity);
    Task DeleteAsync(int id);
    Task<int> SaveChangesAsync();
}
