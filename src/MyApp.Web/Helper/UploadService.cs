using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Components.Forms;

namespace MyApp.Web.Helpers
{
    public class UploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _privateRoot;

        public UploadService(IWebHostEnvironment env)
        {
            _env = env;
            _privateRoot = Path.Combine(_env.ContentRootPath, "PrivateUploads");
            if (!Directory.Exists(_privateRoot))
                Directory.CreateDirectory(_privateRoot);
        }

        public async Task<string> SavePrivateAsync(IBrowserFile file, string subFolder = "")
        {
            var targetPath = string.IsNullOrWhiteSpace(subFolder)
                ? _privateRoot
                : Path.Combine(_privateRoot, subFolder);

            Directory.CreateDirectory(targetPath);

            var unique = $"{Guid.NewGuid():N}{Path.GetExtension(file.Name)}";
            var full = Path.Combine(targetPath, unique);

            await using var stream = new FileStream(full, FileMode.Create);
            await file.OpenReadStream(20_000_000).CopyToAsync(stream);

            // kembalikan relative path agar kita bisa simpan di DB
            return Path.Combine(subFolder, unique).Replace("\\", "/");
        }
    }
}
