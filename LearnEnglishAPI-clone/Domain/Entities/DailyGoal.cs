// IngilizceProje/Domain/Entities/DailyGoal.cs
using System;

namespace IngilizceProje.Domain.Entities
{
    public class DailyGoal
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int TargetCount { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation Properties
        public virtual AppUser User { get; set; } = null!;
    }
}
