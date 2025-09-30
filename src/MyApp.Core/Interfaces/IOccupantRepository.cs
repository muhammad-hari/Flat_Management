using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IOccupantRepository
{
    Task<Occupant?> GetByIdAsync(int id);
    Task<Occupant?> GetByEmployeeIdAsync(int employeeId);
    Task<List<Occupant>> GetAllAsync();
    Task AddAsync(Occupant Occupant);
    Task UpdateAsync(Occupant Occupant);
    Task DeleteAsync(int id);
}
