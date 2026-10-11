// IngilizceProje/Domain/Entities/ReviewHistory.cs
using System;

namespace IngilizceProje.Domain.Entities
{
    public class ReviewHistory
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public bool IsCorrect { get; set; }

        // Navigation Properties
        public virtual Word Word { get; set; } = null!;
        public virtual AppUser User { get; set; } = null!;
    }
}
