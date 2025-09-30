using MyApp.Core.Entities;
using MyApp.Infrastructure.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using MyApp.Web.Helpers;


public class FileService : IFileService
{
    private readonly AppDbContext _context;
    private readonly UploadService _uploadService;

    public FileService(AppDbContext context, UploadService uploadService)
    {
        _context = context;
        _uploadService = uploadService;
    }

    public async Task<FileUpload> SaveFileAsync(IBrowserFile file, string ownerType, int ownerId)
    {
        // 1. Simpan file fisik ke PrivateUploads/<OwnerType>/
        var relativePath = await _uploadService.SavePrivateAsync(file, ownerType);

        // 2. Catat metadata ke DB
        var entity = new FileUpload
        {
            OwnerType = ownerType,
            OwnerId = ownerId,
            Path = relativePath,
            FileName = file.Name,
            ContentType = file.ContentType,
            CreatedAt = DateTime.UtcNow
        };

        _context.Files.Add(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<IEnumerable<FileUpload>> GetFilesAsync(string ownerType, int ownerId)
    {
        return await _context.Files
            .Where(f => f.OwnerType == ownerType && f.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task DeleteFileAsync(int fileId)
    {
        var file = await _context.Files.FindAsync(fileId);
        if (file == null) return;

        // Hapus file fisik juga
        var fullPath = Path.Combine(
            AppContext.BaseDirectory, "PrivateUploads", file.Path);
        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);

        _context.Files.Remove(file);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<FileUpload>> GetFilesByCategoryAsync(string ownerType)
    {
        return await _context.Files
            .Where(f => f.OwnerType == ownerType)
            .ToListAsync();
    }



}



