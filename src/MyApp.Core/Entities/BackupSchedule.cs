using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.Entities
{
    public class BackupSchedule
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Frequency { get; set; } = "Daily"; // Daily, Weekly, Monthly, Custom
        
        public TimeOnly? ScheduledTime { get; set; } // Jam berapa backup dijalankan
        
        public int? DayOfWeek { get; set; } // 0-6 (Sunday-Saturday) untuk Weekly
        
        public int? DayOfMonth { get; set; } // 1-31 untuk Monthly
        
        [MaxLength(500)]
        public string? CustomCronExpression { get; set; } // Untuk Custom schedule
        
        public bool IsEnabled { get; set; } = true;
        
        [Required]
        [MaxLength(500)]
        public string BackupPath { get; set; } = string.Empty; // Path untuk menyimpan backup
        
        public bool IncludeSchema { get; set; } = true;
        
        public bool IncludeData { get; set; } = true;
        
        public int RetentionDays { get; set; } = 30; // Berapa lama backup disimpan
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        public int CreatedBy { get; set; }
        
        public int? UpdatedBy { get; set; }
        
        // Navigation
        public ICollection<BackupHistory> BackupHistories { get; set; } = new List<BackupHistory>();
    }
}