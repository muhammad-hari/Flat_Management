using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;
using MyApp.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUserNameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserName == username && u.IsActive);
    }


    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.FindAsync(id);

    public async Task<List<User>> GetAllAsync() =>
        await _context.Users.ToListAsync();

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        try
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existingUser == null)
                throw new Exception("User not found.");

            // Detach entity lama
            _context.Entry(existingUser).State = EntityState.Detached;

            // Attach entity baru lalu tandai sebagai Modified
            _context.Attach(user);
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("The user record was modified or deleted by another process.");
        }
    }


    public async Task DeleteAsync(int id)
{
    try
    {
        // Coba cari user yang sudah dilacak dulu
        var trackedUser = _context.ChangeTracker.Entries<User>()
                                  .FirstOrDefault(e => e.Entity.Id == id)?.Entity;

        if (trackedUser != null)
        {
            // Kalau sudah dilacak, langsung hapus dari tracking yang sama
            _context.Users.Remove(trackedUser);
        }
        else
        {
            // Kalau belum dilacak, ambil dari DB tanpa tracking
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new Exception("User not found or already deleted.");

            // Tambahkan ke context baru sebagai entity yang akan dihapus
            _context.Entry(user).State = EntityState.Deleted;
        }

        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        throw new Exception("User already deleted or modified by another process.");
    }
}


}
