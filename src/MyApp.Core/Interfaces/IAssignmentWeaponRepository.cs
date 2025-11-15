using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces
{
    public interface IAssignmentWeaponRepository
    {
        Task<List<AssignmentWeapon>> GetAllAsync();
        Task<AssignmentWeapon?> GetByIdAsync(int id);
        Task AddAsync(AssignmentWeapon entity);
        Task UpdateAsync(AssignmentWeapon entity);
        Task DeleteAsync(int id);
    }
}