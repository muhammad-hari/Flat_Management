using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IMaintenanceRequestRepository
{
    Task<MaintenanceRequest?> GetByIdAsync(int id);
    Task<List<MaintenanceRequest>> GetAllAsync();
    Task AddAsync(MaintenanceRequest MaintenanceRequest);
    Task UpdateAsync(MaintenanceRequest MaintenanceRequest);
    Task<bool> DeleteAsync(int id);

}
