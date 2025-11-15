using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

namespace MyApp.Infrastructure.Repositories.Services
{
    public class AssignmentAlsusRepository : IAssignmentAlsusRepository
    {
        private readonly AppDbContext _context;

        public AssignmentAlsusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AssignmentAlsus>> GetAllAsync()
        {
            return await _context.AssignmentAlsuses
                .Include(a => a.Employee)
                .Include(a => a.Alsus)
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync();
        }

        public async Task<AssignmentAlsus?> GetByIdAsync(int id)
        {
            return await _context.AssignmentAlsuses
                .Include(a => a.Employee)
                .Include(a => a.Alsus)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(AssignmentAlsus entity)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    _context.AssignmentAlsuses.Add(entity);

                    var alsus = await _context.Alsuses.FindAsync(entity.AlsusId);
                    if (alsus != null)
                    {
                        alsus.IsAvailable = false;
                        alsus.UpdatedAt = DateTime.UtcNow;
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

        public async Task UpdateAsync(AssignmentAlsus entity)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var existing = await _context.AssignmentAlsuses.FindAsync(entity.Id);
                    if (existing != null)
                    {
                        existing.EmployeeId = entity.EmployeeId;
                        existing.AlsusId = entity.AlsusId;
                        existing.AssignedAt = entity.AssignedAt;
                        existing.ReturnedAt = entity.ReturnedAt;
                        existing.AssignedBy = entity.AssignedBy;
                        existing.ReturnedBy = entity.ReturnedBy;
                        existing.Note = entity.Note;

                        var alsus = await _context.Alsuses.FindAsync(entity.AlsusId);
                        if (alsus != null)
                        {
                            alsus.IsAvailable = entity.ReturnedAt != null;
                            alsus.UpdatedAt = DateTime.UtcNow;
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
            var entity = await _context.AssignmentAlsuses.FindAsync(id);
            if (entity != null)
            {
                _context.AssignmentAlsuses.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}