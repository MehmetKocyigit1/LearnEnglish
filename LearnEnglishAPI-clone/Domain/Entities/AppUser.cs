// IngilizceProje/Domain/Entities/AppUser.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace IngilizceProje.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public AppUser()
        {
            Words = new HashSet<Word>();
            QuizResults = new HashSet<QuizResult>();
            ReviewHistories = new HashSet<ReviewHistory>();
            DailyGoals = new HashSet<DailyGoal>();
        }

        public virtual ICollection<Word> Words { get; set; }
        public virtual ICollection<QuizResult> QuizResults { get; set; }
        public virtual ICollection<ReviewHistory> ReviewHistories { get; set; }
        public virtual ICollection<DailyGoal> DailyGoals { get; set; }
    }
}
