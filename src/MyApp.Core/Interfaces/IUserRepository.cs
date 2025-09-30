using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUserNameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}
