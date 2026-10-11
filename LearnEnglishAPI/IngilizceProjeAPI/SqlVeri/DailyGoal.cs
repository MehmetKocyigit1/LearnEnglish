namespace IngilizceProjeAPI.SqlVeri
{
    public class DailyGoal
    {
        public int Id { get; set; }
        public int TargetCount { get; set; }
        public DateTime CreatedDate { get; set; }

        // Foreign Key
        public string UserId { get; set; }
        public virtual AppUser User { get; set; }
    }
}
