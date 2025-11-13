using System;

namespace MyApp.Core.Entities
{
    public class RestoreHistory
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public int? FileSize { get; set; }
        public int RestoredBy { get; set; }
        public string? UserName { get; set; }
        public string Status { get; set; } = "Pending"; // Success, Failed
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? DurationSeconds { get; set; }
        public string? ErrorMessage { get; set; }
    }
}