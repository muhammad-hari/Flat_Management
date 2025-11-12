using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.Entities
{
    public class BackupHistory
    {
        public int Id { get; set; }
        
        public int? BackupScheduleId { get; set; } // Null jika manual backup
        
        [Required]
        [MaxLength(100)]
        public string BackupType { get; set; } = "Scheduled"; // Scheduled, Manual
        
        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;
        
        public long FileSize { get; set; } // Size in bytes
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed
        
        public DateTime StartedAt { get; set; } = DateTime.Now;
        
        public DateTime? CompletedAt { get; set; }
        
        public int DurationSeconds { get; set; }
        
        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }
        
        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation
        public BackupSchedule? BackupSchedule { get; set; }
    }
}