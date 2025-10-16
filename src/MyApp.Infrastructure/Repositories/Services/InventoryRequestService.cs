using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

public class InventoryRequestRepository : IInventoryRequestRepository
{
    private readonly AppDbContext _context;

    public InventoryRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(InventoryRequest Request)
    {
        await _context.InventoryRequests.AddAsync(Request);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var e = await _context.InventoryRequests.FindAsync(id);
        if (e != null) { _context.InventoryRequests.Remove(e); await _context.SaveChangesAsync(); }
    }

    public async Task<List<InventoryRequest>> GetAllAsync()
    {
        return await _context.InventoryRequests.Include(l => l.Inventory).ToListAsync();
    }

    public async Task<InventoryRequest?> GetByIdAsync(int id)
    {
        return await _context.InventoryRequests.Include(l => l.Inventory).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<InventoryRequest>> GetActiveRequestsAsync()
    {
        return await _context.InventoryRequests.Where(l => l.Status != "Returned" && l.Status != "Cancelled").Include(l => l.Inventory).ToListAsync();
    }

    public async Task UpdateAsync(InventoryRequest Request)
    {
        _context.InventoryRequests.Update(Request);
        await _context.SaveChangesAsync();
    }
}
