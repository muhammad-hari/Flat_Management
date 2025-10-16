using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Infrastructure.Data
{
    public class RepositoryRepository : IRepositoryRepository
    {
        private readonly AppDbContext _context;

        public RepositoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Repository?> GetByIdAsync(int id) =>
            await _context.Repositories.FindAsync(id);

        public async Task<List<Repository>> GetAllAsync() =>
            await _context.Repositories.ToListAsync();

        public async Task AddAsync(Repository Repository)
        {
            await _context.Repositories.AddAsync(Repository);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Repository Repository)
        {
            var existing = await _context.Repositories.FindAsync(Repository.Id);
            if (existing != null)
            {
                existing.Name = Repository.Name;
                existing.Details = Repository.Details;
                existing.IsActive = Repository.IsActive;
                await _context.SaveChangesAsync();
            }
        }


        public async Task DeleteAsync(int id)
        {
            var Repository = await _context.Repositories.FindAsync(id);
            if (Repository != null)
            {
                _context.Repositories.Remove(Repository);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
