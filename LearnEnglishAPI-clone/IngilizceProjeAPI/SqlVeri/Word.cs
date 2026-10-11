using System;
using System.Collections.Generic;
using IngilizceProjeAPI.Enums;

namespace IngilizceProjeAPI.SqlVeri
{
    public class Word
    {
        public int Id { get; set; }
        public string EnglishWord { get; set; }
        public string TurkishMeaning { get; set; }
        public WordTypeEnum WordType { get; set; }
        public string ExampleSentence { get; set; }
        public string ExampleSentenceTranslation { get; set; }
        public LearningStatusEnum LearningStatus { get; set; }
        public bool IsFavorite { get; set; }
        public int ReviewCount { get; set; }
        public int CorrectAnswerCount { get; set; }
        public int WrongAnswerCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        // Foreign Key (Hangi kullanıcıya ait?)
        public string UserId { get; set; }
        public virtual AppUser User { get; set; }

        // Navigation Properties (Bir kelimenin birden çok test sonucu olabilir)
        public virtual ICollection<QuizResult> QuizResults { get; set; }
        public virtual ICollection<ReviewHistory> ReviewHistories { get; set; }
    }
}
