using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces
{
    public interface IAssignmentAlsusRepository
    {
        Task<List<AssignmentAlsus>> GetAllAsync();
        Task<AssignmentAlsus?> GetByIdAsync(int id);
        Task AddAsync(AssignmentAlsus entity);
        Task UpdateAsync(AssignmentAlsus entity);
        Task DeleteAsync(int id);
    }
}