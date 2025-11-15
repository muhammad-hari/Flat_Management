using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

namespace MyApp.Infrastructure.Repositories
{
    public class AssignmentWeaponRepository : IAssignmentWeaponRepository
    {
        private readonly AppDbContext _context;

        public AssignmentWeaponRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AssignmentWeapon>> GetAllAsync()
        {
            return await _context.AssignmentWeapons
                .Include(a => a.Employee)
                .Include(a => a.Weapon)
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync();
        }

        public async Task<AssignmentWeapon?> GetByIdAsync(int id)
        {
            return await _context.AssignmentWeapons
                .Include(a => a.Employee)
                .Include(a => a.Weapon)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(AssignmentWeapon entity)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    _context.AssignmentWeapons.Add(entity);

                    var weapon = await _context.Weapons.FindAsync(entity.WeaponId);
                    if (weapon != null)
                    {
                        weapon.IsAvailable = false;
                        weapon.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task UpdateAsync(AssignmentWeapon entity)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var existing = await _context.AssignmentWeapons.FindAsync(entity.Id);
                    if (existing != null)
                    {
                        existing.EmployeeId = entity.EmployeeId;
                        existing.WeaponId = entity.WeaponId;
                        existing.AssignedAt = entity.AssignedAt;
                        existing.ReturnedAt = entity.ReturnedAt;
                        existing.AssignedBy = entity.AssignedBy;
                        existing.ReturnedBy = entity.ReturnedBy;
                        existing.Note = entity.Note;

                        var weapon = await _context.Weapons.FindAsync(entity.WeaponId);
                        if (weapon != null)
                        {
                            weapon.IsAvailable = entity.ReturnedAt != null;
                            weapon.UpdatedAt = DateTime.UtcNow;
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                    }
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.AssignmentWeapons.FindAsync(id);
            if (entity != null)
            {
                _context.AssignmentWeapons.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}