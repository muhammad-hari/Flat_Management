using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;
using QRCoder;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System.Text;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByIdAsync(int id) =>
        await _context.Inventories
            .Include(o => o.InventoryType)
            .Include(o => o.Repository)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<List<Inventory>> GetAllAsync()
    {
        return await _context.Inventories
            .Include(o => o.InventoryType)
            .Include(o => o.Repository)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Inventory inventory)
    {
        // auto generate barcode saat insert
        inventory.GeneratedBarcodeValue = GenerateBarcode(inventory.Code ?? inventory.Name ?? string.Empty, inventory.BarcodeToGenerate);
        inventory.CreatedAt = DateTime.UtcNow;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _context.Inventories.AddAsync(inventory);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Inventory inventory)
    {
        // regenerate barcode jika Code berubah
        inventory.GeneratedBarcodeValue = GenerateBarcode(inventory.Code ?? inventory.Name ?? string.Empty, inventory.BarcodeToGenerate);
        inventory.UpdatedAt = DateTime.UtcNow;

        // Avoid attaching a second instance with same key if a tracked instance already exists in the ChangeTracker.
        var tracked = await _context.Inventories.FindAsync(inventory.Id);
        if (tracked != null)
        {
            // update scalar properties on the tracked entity
            tracked.Name = inventory.Name;
            tracked.Code = inventory.Code;
            tracked.InventoryTypeId = inventory.InventoryTypeId;
            tracked.RepositoryId = inventory.RepositoryId;
            tracked.Description = inventory.Description;
            tracked.BarcodeToGenerate = inventory.BarcodeToGenerate;
            tracked.GeneratedBarcodeValue = inventory.GeneratedBarcodeValue;
            tracked.IsAvailable = inventory.IsAvailable;
            tracked.PhotoData = inventory.PhotoData;
            tracked.DocumentName = inventory.DocumentName;
            tracked.DocumentContentType = inventory.DocumentContentType;
            tracked.DocumentData = inventory.DocumentData;
            tracked.UpdatedAt = inventory.UpdatedAt;
        }
        else
        {
            // not being tracked yet - attach as modified
            _context.Inventories.Update(inventory);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory != null)
        {
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Generate barcode or QRCode and return as Base64 string (data URI).
    /// </summary>
    private string GenerateBarcode(string value, BarcodeType type)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        if (type == BarcodeType.QRCode)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            var bytes = qrCode.GetGraphic(20);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }
        else
        {
            var writer = new ZXing.BarcodeWriterSvg
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions { Height = 80, Width = 250, Margin = 2 }
            };

            var svg = writer.Write(value);
            return $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(svg.Content))}";
        }
    }
}
