
using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string role);
    Task AddAsync(Role entity);
    Task UpdateAsync(Role entity);
    Task DeleteAsync(int id);
    Task<int> SaveChangesAsync();
}
