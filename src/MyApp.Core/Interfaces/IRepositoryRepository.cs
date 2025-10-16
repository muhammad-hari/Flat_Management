
using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IRepositoryRepository
{
    Task<List<Repository>> GetAllAsync();
    Task<Repository?> GetByIdAsync(int id);
    Task AddAsync(Repository entity);
    Task UpdateAsync(Repository entity);
    Task DeleteAsync(int id);
    Task<int> SaveChangesAsync();
}
