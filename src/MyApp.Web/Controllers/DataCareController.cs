using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Interfaces;
using System.Security.Claims;

namespace MyApp.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataCareController : ControllerBase
    {
        private readonly IDataCareService _backupService;
        private readonly ILogger<DataCareController> _logger;

        public DataCareController(
            IDataCareService backupService, 
            ILogger<DataCareController> logger)
        {
            _backupService = backupService;
            _logger = logger;
        }

        /// <summary>
        /// Restore database from backup
        /// </summary>
        [HttpPost("restore/{historyId}")]
        public async Task<IActionResult> RestoreBackup(int historyId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userId == 0)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                _logger.LogWarning($"🔄 Database restore initiated by user {User.Identity?.Name} for backup {historyId}");

                var result = await _backupService.RestoreBackupAsync(historyId, userId);
                
                if (!result)
                {
                    return BadRequest(new { message = "Restore failed. Check logs for details." });
                }

                _logger.LogInformation($"✅ Database restored successfully from backup {historyId}");
                
                return Ok(new 
                { 
                    message = "Database restored successfully",
                    backupId = historyId,
                    restoredBy = User.Identity?.Name,
                    restoredAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error restoring backup {historyId}");
                return StatusCode(500, new { message = $"Restore failed: {ex.Message}" });
            }
        }

        /// <summary>
        /// Validate backup file before restore
        /// </summary>
        [HttpGet("restore/validate/{historyId}")]
        public async Task<IActionResult> ValidateBackup(int historyId)
        {
            try
            {
                var history = await _backupService.GetBackupHistoryByIdAsync(historyId);
                
                if (history == null)
                {
                    return NotFound(new { message = "Backup not found" });
                }

                if (!System.IO.File.Exists(history.FilePath))
                {
                    return BadRequest(new 
                    { 
                        valid = false, 
                        message = "Backup file does not exist",
                        filePath = history.FilePath
                    });
                }

                if (history.Status != "Success")
                {
                    return BadRequest(new 
                    { 
                        valid = false, 
                        message = $"Backup status is '{history.Status}'. Only successful backups can be restored."
                    });
                }

                var fileInfo = new FileInfo(history.FilePath);
                
                return Ok(new
                {
                    valid = true,
                    message = "Backup is valid for restore",
                    fileName = history.FileName,
                    fileSize = history.FileSize,
                    backupDate = history.StartedAt,
                    backupType = history.BackupType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error validating backup {historyId}");
                return StatusCode(500, new { message = "Error validating backup" });
            }
        }

        /// <summary>
        /// Delete backup file
        /// </summary>
        [HttpDelete("backup/{historyId}")]
        public async Task<IActionResult> DeleteBackup(int historyId)
        {
            try
            {
                var result = await _backupService.DeleteBackupFileAsync(historyId);
                
                if (!result)
                {
                    return NotFound(new { message = "Backup not found" });
                }

                _logger.LogInformation($"🗑️ Backup deleted: {historyId} by user {User.Identity?.Name}");
                
                return Ok(new { message = "Backup deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error deleting backup {historyId}");
                return StatusCode(500, new { message = "Error deleting backup" });
            }
        }

        /// <summary>
        /// Restore database from uploaded backup file
        /// </summary>
        [HttpPost("restore/upload")]
        public async Task<IActionResult> RestoreFromUpload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized(new { message = "User not authenticated" });

            // Save uploaded file to temp folder
            var tempFolder = Path.Combine(Path.GetTempPath(), "MyAppRestore");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            var fileName = Path.GetFileName(file.FileName);
            var tempFilePath = Path.Combine(tempFolder, $"{Guid.NewGuid()}_{fileName}");

            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Call restore logic
                var backupService = HttpContext.RequestServices.GetRequiredService<IDataCareService>();
                var result = await backupService.RestoreBackupFromFileAsync(tempFilePath, userId);

                // Clean up temp file
                if (System.IO.File.Exists(tempFilePath))
                    System.IO.File.Delete(tempFilePath);

                if (!result)
                    return BadRequest(new { message = "Restore failed. Check logs for details." });

                return Ok(new { message = "Database restored successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error restoring from uploaded file");
                return StatusCode(500, new { message = $"Restore failed: {ex.Message}" });
            }
        }
    }
}