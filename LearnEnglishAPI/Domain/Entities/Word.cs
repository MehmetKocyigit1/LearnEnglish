// IngilizceProje/Domain/Entities/Word.cs
using System;
using System.Collections.Generic;
using IngilizceProje.Domain.Enums;

namespace IngilizceProje.Domain.Entities
{
    public class Word
    {
        public Word()
        {
            QuizResults = new HashSet<QuizResult>();
            ReviewHistories = new HashSet<ReviewHistory>();
        }

        public int Id { get; set; }
        public string EnglishWord { get; set; } = string.Empty;
        public string TurkishMeaning { get; set; } = string.Empty;
        public WordTypeEnum WordType { get; set; }
        public string ExampleSentence { get; set; } = string.Empty;
        public string ExampleSentenceTranslation { get; set; } = string.Empty;
        public LearningStatusEnum LearningStatus { get; set; } = LearningStatusEnum.NotReviewed;
        public bool IsFavorite { get; set; }
        public int ReviewCount { get; set; }
        public int CorrectAnswerCount { get; set; }
        public int WrongAnswerCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        public virtual AppUser User { get; set; } = null!;
        public virtual ICollection<QuizResult> QuizResults { get; set; }
        public virtual ICollection<ReviewHistory> ReviewHistories { get; set; }
    }
}
