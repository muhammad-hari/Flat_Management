
using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<ApplicationUser?> GetUserByUsernameAsync(string username);
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<bool> CreateUserAsync(ApplicationUser user, string password);
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(string id);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<bool> AddUserToRoleAsync(ApplicationUser user, string roleName);
        Task<bool> RemoveUserFromRoleAsync(ApplicationUser user, string roleName);
    }
}