
using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IVendorRepository
{
    Task<List<Vendor>> GetAllAsync();
    Task<Vendor?> GetByIdAsync(int id);
    Task AddAsync(Vendor entity);
    Task UpdateAsync(Vendor entity);
    Task<bool> DeleteAsync(int id);
    Task<int> SaveChangesAsync();
}
