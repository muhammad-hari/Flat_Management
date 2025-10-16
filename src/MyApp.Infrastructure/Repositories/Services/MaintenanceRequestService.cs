using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Infrastructure.Data
{
    public class MaintenanceRequestRepository : IMaintenanceRequestRepository
    {
        private readonly AppDbContext _context;

        public MaintenanceRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MaintenanceRequest?> GetByIdAsync(int id) =>
            await _context.MaintenanceRequests.FindAsync(id);

        public async Task<List<MaintenanceRequest>> GetAllAsync()
        {
            return await _context.MaintenanceRequests
                // WAJIB: Eager Load Building dan Room!
                .Include(r => r.Building)
                .Include(r => r.Room)
                .ToListAsync();
        }
        public async Task AddAsync(MaintenanceRequest MaintenanceRequest)
        {
            await _context.MaintenanceRequests.AddAsync(MaintenanceRequest);
            await _context.SaveChangesAsync();
        }


        // MyApp.Infrastructure.Data.MaintenanceRequestRepository (Contoh)

        public async Task UpdateAsync(MaintenanceRequest entity)
        {
            // 1. Cek apakah entitas sudah dilacak oleh DbContext
            var existingEntry = _context.ChangeTracker.Entries<MaintenanceRequest>()
                .FirstOrDefault(e => e.Entity.Id == entity.Id);

            if (existingEntry != null)
            {
                // Jika sudah dilacak, lepaskan entitas yang bertentangan
                existingEntry.State = EntityState.Detached;
            }

            // 2. Lampirkan entitas baru (yang berasal dari Blazor) dengan status Modified
            _context.Entry(entity).State = EntityState.Modified;

            // Opsional: Untuk mencegah pembaruan relasi navigasi (Building/Room)
            // yang tidak berubah dan tidak dilacak:
            _context.Entry(entity).Property(r => r.BuildingId).IsModified = true;
            _context.Entry(entity).Property(r => r.RoomId).IsModified = true;

            await _context.SaveChangesAsync();
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var MaintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (MaintenanceRequest == null)
            {
                return false; // Room tidak ditemukan
            }

            try
            {
                _context.MaintenanceRequests.Remove(MaintenanceRequest);
                await _context.SaveChangesAsync();
                return true; // Berhasil dihapus
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is MySqlConnector.MySqlException mySqlEx)
            {

                if (mySqlEx.Message.Contains("Cannot delete or update a parent row: a foreign key constraint fails"))
                {
                    return false;

                }

                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
