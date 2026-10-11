using System;
using IngilizceProjeAPI.SqlVeri;

namespace IngilizceProjeAPI.SqlVeri

{
    public class QuizResult
    {
        public int Id { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime AnswerDate { get; set; }

        // Foreign Keys
        public int WordId { get; set; }
        public virtual Word Word { get; set; }

        public string UserId { get; set; }
        public virtual AppUser User { get; set; }
    }
}