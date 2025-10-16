using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Infrastructure.Data
{
    public class VendorRepository : IVendorRepository
    {
        private readonly AppDbContext _context;

        public VendorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Vendor?> GetByIdAsync(int id) =>
            await _context.Vendors.FindAsync(id);

        public async Task<List<Vendor>> GetAllAsync() =>
            await _context.Vendors.ToListAsync();

        public async Task AddAsync(Vendor Vendor)
        {
            await _context.Vendors.AddAsync(Vendor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vendor Vendor)
        {
            var existing = await _context.Vendors.FindAsync(Vendor.Id);
            if (existing != null)
            {
                existing.Name = Vendor.Name;
                existing.Address = Vendor.Address;
                existing.BusinnesType = Vendor.BusinnesType;
                existing.Email = Vendor.Email;
                existing.Director = Vendor.Director;
                existing.Phone = Vendor.Phone;
                existing.SecondPhone = Vendor.SecondPhone;
                existing.Details = Vendor.Details;
                await _context.SaveChangesAsync();
            }
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
            {
                return false; // Room tidak ditemukan
            }

            try
            {
                _context.Vendors.Remove(vendor);
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
