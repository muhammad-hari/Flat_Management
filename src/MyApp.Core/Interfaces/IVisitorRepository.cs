using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IVisitorRepository
{
    Task<Visitor?> GetByIdAsync(int id);
    Task<List<Visitor>> GetAllAsync();
    Task AddAsync(Visitor Visitor);
    Task UpdateAsync(Visitor Visitor);
    Task DeleteAsync(int id);
}
