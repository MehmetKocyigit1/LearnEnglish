using Microsoft.AspNetCore.Identity;

namespace IngilizceProjeAPI.SqlVeri
{
    public class AppUser : IdentityUser
    {
        // Navigation Properties (Bir kullanıcının birden çok verisi olabilir)
        public virtual ICollection<Word> Words { get; set; }
        public virtual ICollection<QuizResult> QuizResults { get; set; }
        public virtual ICollection<ReviewHistory> ReviewHistories { get; set; }
        public virtual ICollection<DailyGoal> DailyGoals { get; set; }
    }
}
