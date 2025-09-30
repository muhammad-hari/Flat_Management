using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

public class BuildingRepository : IBuildingRepository
{
    private readonly AppDbContext _context;

    public BuildingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Building?> GetByIdAsync(int id) =>
        await _context.Buildings.FindAsync(id);

    public async Task<List<Building>> GetAllAsync() =>
        await _context.Buildings
                         .AsNoTracking()
                         .ToListAsync();
    public async Task AddAsync(Building Building)
    {
        await _context.Buildings.AddAsync(Building);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Building building)
    {
        var local = _context.Set<Building>()
                            .Local
                            .FirstOrDefault(e => e.Id == building.Id);

        if (local != null)
            _context.Entry(local).State = EntityState.Detached;

        _context.Attach(building);
        _context.Entry(building).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }


    public async Task DeleteAsync(int id)
    {
        var Building = await _context.Buildings.FindAsync(id);
        if (Building != null)
        {
            _context.Buildings.Remove(Building);
            await _context.SaveChangesAsync();
        }
    }
}
