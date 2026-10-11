// IngilizceProje/Application/DTOs/StatsDtos.cs
using System;

namespace IngilizceProje.Application.DTOs
{
    public class StatisticsDto
    {
        public int TotalWords { get; set; }
        public int VeryImportantCount { get; set; }
        public int NeedsReviewCount { get; set; }
        public int LearnedCount { get; set; }
        public int NotReviewedCount { get; set; }
        public double SuccessRate { get; set; }
        public int DailyGoalTarget { get; set; }
        public int DailyWordsAddedToday { get; set; }
        public double DailyGoalProgress { get; set; } // Percentage e.g. 0 to 100
    }

    public class DailyGoalDto
    {
        public int TargetCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
