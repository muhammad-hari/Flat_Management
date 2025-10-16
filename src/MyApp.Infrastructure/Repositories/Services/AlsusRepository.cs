using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

namespace MyApp.Infrastructure.Repositories.Services;

public class AlsusRepository : IAlsusRepository
{
    private readonly AppDbContext _context;

    public AlsusRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Alsus entity)
    {
        await _context.Set<Alsus>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var e = await _context.Set<Alsus>().FindAsync(id);
        if (e == null) return false;
        _context.Set<Alsus>().Remove(e);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Alsus>> GetAllAsync()
    {
        return await _context.Set<Alsus>().ToListAsync();
    }

    public async Task<Alsus?> GetByIdAsync(int id)
    {
        return await _context.Set<Alsus>().FindAsync(id);
    }

    public async Task UpdateAsync(Alsus entity)
    {
        var tracked = _context.ChangeTracker.Entries<Alsus>().FirstOrDefault(e => e.Entity.Id == entity.Id);
        if (tracked != null) _context.Entry(tracked.Entity).State = EntityState.Detached;
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
