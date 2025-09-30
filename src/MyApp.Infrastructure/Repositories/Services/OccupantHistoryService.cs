using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

public class OccupantHistoryRepository : IOccupantHistoryRepository
{
    private readonly AppDbContext _context;

    public OccupantHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OccupantHistory?> GetByIdAsync(int id) =>
        await _context.OccupantHistories.FindAsync(id);

    public async Task<OccupantHistory?> GetByOccupantIdAsync(int occupantId)
    {
        return await _context.OccupantHistories
            .Include(o => o.Occupant)
                .ThenInclude(e => e.Employee)
            .Include(o => o.Room)
                .ThenInclude(r => r.Building)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OccupantId == occupantId);
    }


    // OccupantHistoryRepository.cs
    public async Task<List<OccupantHistory>> GetAllAsync()
    {
        return await _context.OccupantHistories
            .Include(o => o.Occupant)
                .ThenInclude(e => e.Employee)
            .Include(o => o.Room)
                .ThenInclude(e => e.Building)
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task AddAsync(OccupantHistory OccupantHistory)
    {
        await _context.OccupantHistories.AddAsync(OccupantHistory);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(OccupantHistory OccupantHistory)
    {
        _context.OccupantHistories.Update(OccupantHistory);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDocumentAsync(int id, byte[] docData, string docName, string contentType)
    {
        var occ = await _context.OccupantHistories.FindAsync(id);
        if (occ == null) return;

        occ.DocumentData = docData;
        occ.DocumentName = docName;
        occ.DocumentContentType = contentType;
        occ.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var OccupantHistory = await _context.OccupantHistories.FindAsync(id);
        if (OccupantHistory != null)
        {
            _context.OccupantHistories.Remove(OccupantHistory);
            await _context.SaveChangesAsync();
        }
    }
}
