using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IOccupantHistoryRepository
{
    Task<OccupantHistory?> GetByIdAsync(int id);
    Task<OccupantHistory?> GetByOccupantIdAsync(int employeeId);
    Task<List<OccupantHistory>> GetAllAsync();
    Task AddAsync(OccupantHistory OccupantHistory);
    Task UpdateAsync(OccupantHistory OccupantHistory);
    Task DeleteAsync(int id);
}
