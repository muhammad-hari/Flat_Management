using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

public interface IWeaponRepository
{
    Task<Weapon?> GetByIdAsync(int id);
    Task<List<Weapon>> GetAllAsync();
    Task AddAsync(Weapon entity);
    Task UpdateAsync(Weapon entity);
    Task<bool> DeleteAsync(int id);
}
