using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

public class OccupantRepository : IOccupantRepository
{
    private readonly AppDbContext _context;

    public OccupantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Occupant?> GetByIdAsync(int id) =>
        await _context.Occupants.FindAsync(id);

    public async Task<List<Occupant>> GetAllAsync()
    {
        return await _context.Occupants
            .Include(o => o.Employee)
            .Include(o => o.Room)
            .AsNoTracking()  // <-- ini penting
            .ToListAsync();
    }


    public async Task AddAsync(Occupant Occupant)
    {
        await _context.Occupants.AddAsync(Occupant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Occupant Occupant)
    {
        _context.Occupants.Update(Occupant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDocumentAsync(int id, byte[] docData, string docName, string contentType)
    {
        var occ = await _context.Occupants.FindAsync(id);
        if (occ == null) return;

        occ.DocumentData = docData;
        occ.DocumentName = docName;
        occ.DocumentContentType = contentType;
        occ.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var Occupant = await _context.Occupants.FindAsync(id);
        if (Occupant != null)
        {
            _context.Occupants.Remove(Occupant);
            await _context.SaveChangesAsync();
        }
    }
}
