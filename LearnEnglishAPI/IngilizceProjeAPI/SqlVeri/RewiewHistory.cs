namespace IngilizceProjeAPI.SqlVeri
{
    public class ReviewHistory
    {
        public int Id { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsCorrect { get; set; }

        // Foreign Keys
        public int WordId { get; set; }
        public virtual Word Word { get; set; }

        public string UserId { get; set; }
        public virtual AppUser User { get; set; }
    }
}
