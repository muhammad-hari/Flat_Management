using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

public class InventoryHistoryRepository : IInventoryHistoryRepository
{
    private readonly AppDbContext _context;

    public InventoryHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(InventoryHistory Request)
    {
        await _context.InventoryHistories.AddAsync(Request);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var e = await _context.InventoryHistories.FindAsync(id);
        if (e != null) { _context.InventoryHistories.Remove(e); await _context.SaveChangesAsync(); }
    }

    public async Task<List<InventoryHistory>> GetAllAsync()
    {
        return await _context.InventoryHistories.Include(l => l.Inventory).ToListAsync();
    }

    public async Task<InventoryHistory?> GetByIdAsync(int id)
    {
        return await _context.InventoryHistories.Include(l => l.Inventory).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<InventoryHistory>> GetActiveRequestsAsync()
    {
        return await _context.InventoryHistories.Where(l => l.Status != "Returned" && l.Status != "Cancelled").Include(l => l.Inventory).ToListAsync();
    }

    public async Task UpdateAsync(InventoryHistory Request)
    {
        _context.InventoryHistories.Update(Request);
        await _context.SaveChangesAsync();
    }
}
