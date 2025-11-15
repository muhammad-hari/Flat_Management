using System;

namespace MyApp.Core.Entities
{
    public class AssignmentAlsus
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        public int AlsusId { get; set; }
        public Alsus Alsus { get; set; } = null!;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnedAt { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public string? ReturnedBy { get; set; }
        public string? Note { get; set; }
    }
}