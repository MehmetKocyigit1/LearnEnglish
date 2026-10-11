// IngilizceProje/Infrastructure/Services/StatisticsService.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IngilizceProje.Application.DTOs;
using IngilizceProje.Application.Interfaces;
using IngilizceProje.Domain.Entities;
using IngilizceProje.Domain.Enums;
using IngilizceProje.Infrastructure.Data;

namespace IngilizceProje.Infrastructure.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StatisticsDto> GetUserStatisticsAsync(string userId)
        {
            var words = await _context.Words
                .Where(w => w.UserId == userId)
                .ToListAsync();

            int totalWords = words.Count;
            int veryImportantCount = words.Count(w => w.LearningStatus == LearningStatusEnum.VeryImportant);
            int needsReviewCount = words.Count(w => w.LearningStatus == LearningStatusEnum.NeedsReview);
            int learnedCount = words.Count(w => w.LearningStatus == LearningStatusEnum.Learned);
            int notReviewedCount = words.Count(w => w.LearningStatus == LearningStatusEnum.NotReviewed);

            int totalCorrect = words.Sum(w => w.CorrectAnswerCount);
            int totalWrong = words.Sum(w => w.WrongAnswerCount);
            int totalAnswers = totalCorrect + totalWrong;
            double successRate = totalAnswers > 0 ? (double)totalCorrect / totalAnswers * 100 : 0;

            // Get daily goal target
            var latestGoal = await _context.DailyGoals
                .Where(dg => dg.UserId == userId)
                .OrderByDescending(dg => dg.CreatedDate)
                .FirstOrDefaultAsync();

            int dailyGoalTarget = latestGoal?.TargetCount ?? 10; // Default goal is 10 words if not set

            // Get words added today (UTC or local, let's use UTC date comparisons but check Today)
            var todayUtc = DateTime.UtcNow.Date;
            int dailyWordsAddedToday = await _context.Words
                .CountAsync(w => w.UserId == userId && w.CreatedDate >= todayUtc);

            double dailyGoalProgress = dailyGoalTarget > 0 
                ? Math.Min(((double)dailyWordsAddedToday / dailyGoalTarget * 100), 100) 
                : 100;

            return new StatisticsDto
            {
                TotalWords = totalWords,
                VeryImportantCount = veryImportantCount,
                NeedsReviewCount = needsReviewCount,
                LearnedCount = learnedCount,
                NotReviewedCount = notReviewedCount,
                SuccessRate = Math.Round(successRate, 1),
                DailyGoalTarget = dailyGoalTarget,
                DailyWordsAddedToday = dailyWordsAddedToday,
                DailyGoalProgress = Math.Round(dailyGoalProgress, 1)
            };
        }

        public async Task<bool> SetDailyGoalAsync(int targetCount, string userId)
        {
            if (targetCount <= 0)
            {
                throw new ArgumentException("Günlük hedef sıfırdan büyük olmalıdır.");
            }

            var todayUtc = DateTime.UtcNow.Date;

            // Check if user already set a goal today
            var existingGoalToday = await _context.DailyGoals
                .FirstOrDefaultAsync(dg => dg.UserId == userId && dg.CreatedDate >= todayUtc);

            if (existingGoalToday != null)
            {
                existingGoalToday.TargetCount = targetCount;
                _context.DailyGoals.Update(existingGoalToday);
            }
            else
            {
                var dailyGoal = new DailyGoal
                {
                    UserId = userId,
                    TargetCount = targetCount,
                    CreatedDate = DateTime.UtcNow
                };
                await _context.DailyGoals.AddAsync(dailyGoal);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DailyGoalDto?> GetDailyGoalAsync(string userId)
        {
            var latestGoal = await _context.DailyGoals
                .Where(dg => dg.UserId == userId)
                .OrderByDescending(dg => dg.CreatedDate)
                .FirstOrDefaultAsync();

            if (latestGoal == null) return null;

            return new DailyGoalDto
            {
                TargetCount = latestGoal.TargetCount,
                CreatedDate = latestGoal.CreatedDate
            };
        }
    }
}
