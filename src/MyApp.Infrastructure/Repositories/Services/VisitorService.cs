using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

public class VisitorRepository : IVisitorRepository
{
    private readonly AppDbContext _context;

    public VisitorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Visitor?> GetByIdAsync(int id) =>
        await _context.Visitors.FindAsync(id);

    public async Task<List<Visitor>> GetAllAsync()
    {
        return await _context.Visitors
            .Include(o => o.Occupant)
                .ThenInclude(e => e.Employee)
                .ThenInclude(f => f.Position)
            .Include(o => o.Occupant)
            .ThenInclude(e => e.Room)
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task AddAsync(Visitor Visitor)
    {
        await _context.Visitors.AddAsync(Visitor);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Visitor Visitor)
    {
        _context.Visitors.Update(Visitor);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDocumentAsync(int id, byte[] docData, string docName, string contentType)
    {
        var occ = await _context.Visitors.FindAsync(id);
        if (occ == null) return;

        occ.DocumentData = docData;
        occ.DocumentName = docName;
        occ.DocumentContentType = contentType;
        occ.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var Visitor = await _context.Visitors.FindAsync(id);
        if (Visitor != null)
        {
            _context.Visitors.Remove(Visitor);
            await _context.SaveChangesAsync();
        }
    }
}
