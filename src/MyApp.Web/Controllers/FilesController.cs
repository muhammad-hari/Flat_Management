using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Web.Controllers
{
    [Route("secure-files")]
    // [Authorize] // atau custom policy
    public class FilesController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public FilesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("{*path}")]
        public IActionResult GetPrivateFile(string path)
        {
            var fullPath = Path.Combine(_env.ContentRootPath, "PrivateUploads", path);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var mime = "application/octet-stream";
            return PhysicalFile(fullPath, mime);
        }
    }
}
