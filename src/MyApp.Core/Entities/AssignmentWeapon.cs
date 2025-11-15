using System;

namespace MyApp.Core.Entities
{
    public class AssignmentWeapon
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        public int WeaponId { get; set; }
        public Weapon Weapon { get; set; } = null!;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnedAt { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public string? ReturnedBy { get; set; }
        public string? Note { get; set; }
    }
}