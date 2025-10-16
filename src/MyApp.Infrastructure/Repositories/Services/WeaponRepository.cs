using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

namespace MyApp.Infrastructure.Repositories.Services;

public class WeaponRepository : IWeaponRepository
{
    private readonly AppDbContext _context;

    public WeaponRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Weapon entity)
    {
        await _context.Set<Weapon>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var e = await _context.Set<Weapon>().FindAsync(id);
        if (e == null) return false;
        _context.Set<Weapon>().Remove(e);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Weapon>> GetAllAsync()
    {
        return await _context.Set<Weapon>().ToListAsync();
    }

    public async Task<Weapon?> GetByIdAsync(int id)
    {
        return await _context.Set<Weapon>().FindAsync(id);
    }

    public async Task UpdateAsync(Weapon entity)
    {
        var tracked = _context.ChangeTracker.Entries<Weapon>().FirstOrDefault(e => e.Entity.Id == entity.Id);
        if (tracked != null) _context.Entry(tracked.Entity).State = EntityState.Detached;
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
