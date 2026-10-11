// IngilizceProje/Infrastructure/Services/QuizService.cs
using System;
using System.Collections.Generic;
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
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;

        public QuizService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MatchingQuizDto?> GenerateMatchingQuizAsync(string userId)
        {
            var userWords = await _context.Words
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (userWords.Count < 5)
            {
                throw new InvalidOperationException("Eşleştirme testi oluşturmak için en az 5 kelimenizin bulunması gerekmektedir.");
            }

            var random = new Random();
            var chosenWords = userWords
                .OrderBy(x => random.Next())
                .Take(5)
                .ToList();

            var pairs = chosenWords.Select(w => new MatchingPairDto
            {
                WordId = w.Id,
                EnglishWord = w.EnglishWord,
                TurkishMeaning = w.TurkishMeaning
            }).ToList();

            var shuffledMeanings = chosenWords
                .Select(w => w.TurkishMeaning)
                .OrderBy(x => random.Next())
                .ToList();

            return new MatchingQuizDto
            {
                Pairs = pairs,
                ShuffledTurkishMeanings = shuffledMeanings
            };
        }

        public async Task<TestDto?> GenerateMultipleChoiceTestAsync(string userId)
        {
            var userWords = await _context.Words
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (userWords.Count < 4)
            {
                throw new InvalidOperationException("Çoktan seçmeli test oluşturmak için en az 4 kelimenizin bulunması gerekmektedir.");
            }

            var random = new Random();
            // Select correct word
            var correctWord = userWords[random.Next(userWords.Count)];

            // Select 3 distractors
            var distractors = userWords
                .Where(w => w.Id != correctWord.Id)
                .OrderBy(x => random.Next())
                .Take(3)
                .ToList();

            // Prepare options list
            var options = new List<string> { correctWord.TurkishMeaning };
            options.AddRange(distractors.Select(d => d.TurkishMeaning));

            // Shuffle options
            options = options.OrderBy(x => random.Next()).ToList();

            return new TestDto
            {
                WordId = correctWord.Id,
                EnglishWord = correctWord.EnglishWord,
                ExampleSentence = correctWord.ExampleSentence,
                ExampleSentenceTranslation = correctWord.ExampleSentenceTranslation,
                Options = options
            };
        }

        public async Task<bool> SubmitQuizResultAsync(QuizResultDto resultDto, string userId)
        {
            var word = await _context.Words
                .FirstOrDefaultAsync(w => w.Id == resultDto.WordId && w.UserId == userId);

            if (word == null) return false;

            // Save QuizResult
            var quizResult = new QuizResult
            {
                WordId = word.Id,
                UserId = userId,
                IsCorrect = resultDto.IsCorrect,
                AnswerDate = DateTime.UtcNow
            };
            await _context.QuizResults.AddAsync(quizResult);

            // Update Word Statistics
            word.ReviewCount++;
            if (resultDto.IsCorrect)
            {
                word.CorrectAnswerCount++;
            }
            else
            {
                word.WrongAnswerCount++;
            }

            // Recalculate LearningStatus
            UpdateLearningStatus(word);
            word.UpdatedDate = DateTime.UtcNow;
            word.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        private void UpdateLearningStatus(Word word)
        {
            int totalAnswers = word.CorrectAnswerCount + word.WrongAnswerCount;
            if (totalAnswers == 0)
            {
                word.LearningStatus = LearningStatusEnum.NotReviewed;
            }
            else
            {
                double successRate = (double)word.CorrectAnswerCount / totalAnswers * 100;
                if (successRate < 30.0)
                {
                    word.LearningStatus = LearningStatusEnum.VeryImportant;
                }
                else if (successRate < 90.0)
                {
                    word.LearningStatus = LearningStatusEnum.NeedsReview;
                }
                else
                {
                    word.LearningStatus = LearningStatusEnum.Learned;
                }
            }
        }
    }
}
