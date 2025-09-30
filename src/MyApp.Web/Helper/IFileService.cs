using MyApp.Core.Entities;
using Microsoft.AspNetCore.Components.Forms;

public interface IFileService
{
    Task<FileUpload> SaveFileAsync(IBrowserFile file, string ownerType, int ownerId);
    Task<IEnumerable<FileUpload>> GetFilesAsync(string ownerType, int ownerId);
    Task DeleteFileAsync(int fileId);
    Task<IEnumerable<FileUpload>> GetFilesByCategoryAsync(string ownerType);
}
