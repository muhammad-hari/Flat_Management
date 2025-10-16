using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(int id) =>
        await _context.Rooms.FindAsync(id);

    public async Task<List<Room>> GetAllAsync() =>
        await _context.Rooms.ToListAsync();

    public async Task AddAsync(Room Room)
    {
        await _context.Rooms.AddAsync(Room);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        // 1. Cek apakah entitas yang memiliki ID yang sama sudah dilacak.
        var existingEntry = _context.ChangeTracker.Entries<Room>()
                                    .FirstOrDefault(e => e.Entity.Id == room.Id);

        if (existingEntry != null)
        {
            // Jika sudah ada (konflik), lepas pelacakannya (Detach)
            _context.Entry(existingEntry.Entity).State = EntityState.Detached;
        }

        // 2. Lampirkan entitas 'room' yang dimodifikasi ke DbContext.
        // 3. Set statusnya menjadi Modified (diubah).
        _context.Entry(room).State = EntityState.Modified;

        // 4. Simpan perubahan.
        await _context.SaveChangesAsync();
    }

    // public async Task DeleteAsync(int id)
    // {
    //     var Room = await _context.Rooms.FindAsync(id);
    //     if (Room != null)
    //     {
    //         _context.Rooms.Remove(Room);
    //         await _context.SaveChangesAsync();
    //     }
    // }

    public async Task<bool> DeleteAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return false; // Room tidak ditemukan
        }

        try
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true; // Berhasil dihapus
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is MySqlConnector.MySqlException mySqlEx)
        {
            // Kode kesalahan MySQL untuk batasan foreign key biasanya adalah 1451
            // atau 1452 (tergantung versi/skenario). Kita cek pesan di dalamnya saja.
            if (mySqlEx.Message.Contains("Cannot delete or update a parent row: a foreign key constraint fails"))
            {
                // Logika bisnis: Room tidak bisa dihapus karena masih ada Occupant.
                return false;
                // ATAU, jika Anda ingin pesan yang lebih spesifik di service:
                // throw new InvalidOperationException("Room cannot be deleted because it still has occupants.", ex);
            }

            // Melemparkan exception lain yang tidak terkait Foreign Key
            throw;
        }
        catch (Exception)
        {
            // Handle exception umum lainnya
            throw;
        }
    }

    public async Task<List<Room>> GetAllWithRelationsAsync()
    {
        return await _context.Rooms
            .Include(r => r.Category)   // relasi ke RoomCategory
            .Include(r => r.Status)     // relasi ke RoomStatus
            .Include(r => r.Condition)  // relasi ke RoomCondition
            .AsNoTracking()
            .ToListAsync();
    }


}
