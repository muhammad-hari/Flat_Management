using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IAlsusRepository
{
    Task<Alsus?> GetByIdAsync(int id);
    Task<List<Alsus>> GetAllAsync();
    Task AddAsync(Alsus entity);
    Task UpdateAsync(Alsus entity);
    Task<bool> DeleteAsync(int id);
}
