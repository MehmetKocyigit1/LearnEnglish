// IngilizceProje/Domain/Entities/QuizResult.cs
using System;

namespace IngilizceProje.Domain.Entities
{
    public class QuizResult
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public DateTime AnswerDate { get; set; }

        // Navigation Properties
        public virtual Word Word { get; set; } = null!;
        public virtual AppUser User { get; set; } = null!;
    }
}
