// IngilizceProje/Infrastructure/Services/WordService.cs
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
    public class WordService : IWordService
    {
        private readonly AppDbContext _context;

        public WordService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WordDto>> GetAllWordsAsync(string userId)
        {
            var words = await _context.Words
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();

            return words.Select(MapToDto);
        }

        public async Task<WordDto?> GetWordByIdAsync(int id, string userId)
        {
            var word = await _context.Words
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            return word == null ? null : MapToDto(word);
        }

        public async Task<WordDto> CreateWordAsync(CreateWordDto createWordDto, string userId)
        {
            var trimmedWord = createWordDto.EnglishWord.Trim();
            var exists = await _context.Words.AnyAsync(w => 
                w.UserId == userId && 
                w.EnglishWord.ToLower() == trimmedWord.ToLower());

            if (exists)
            {
                throw new InvalidOperationException($"'{trimmedWord}' kelimesi zaten listenizde mevcut.");
            }

            var word = new Word
            {
                EnglishWord = trimmedWord,
                TurkishMeaning = createWordDto.TurkishMeaning.Trim(),
                WordType = createWordDto.WordType,
                ExampleSentence = createWordDto.ExampleSentence.Trim(),
                ExampleSentenceTranslation = createWordDto.ExampleSentenceTranslation.Trim(),
                LearningStatus = LearningStatusEnum.NotReviewed,
                IsFavorite = false,
                ReviewCount = 0,
                CorrectAnswerCount = 0,
                WrongAnswerCount = 0,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId,
                UserId = userId
            };

            await _context.Words.AddAsync(word);
            await _context.SaveChangesAsync();

            return MapToDto(word);
        }

        public async Task<WordDto?> UpdateWordAsync(UpdateWordDto updateWordDto, string userId)
        {
            var word = await _context.Words
                .FirstOrDefaultAsync(w => w.Id == updateWordDto.Id && w.UserId == userId);

            if (word == null) return null;

            // Check duplicate EnglishWord if it is changed
            var trimmedWord = updateWordDto.EnglishWord.Trim();
            if (word.EnglishWord.ToLower() != trimmedWord.ToLower())
            {
                var exists = await _context.Words.AnyAsync(w => 
                    w.UserId == userId && 
                    w.EnglishWord.ToLower() == trimmedWord.ToLower());

                if (exists)
                {
                    throw new InvalidOperationException($"'{trimmedWord}' kelimesi zaten listenizde mevcut.");
                }
            }

            word.EnglishWord = trimmedWord;
            word.TurkishMeaning = updateWordDto.TurkishMeaning.Trim();
            word.WordType = updateWordDto.WordType;
            word.ExampleSentence = updateWordDto.ExampleSentence.Trim();
            word.ExampleSentenceTranslation = updateWordDto.ExampleSentenceTranslation.Trim();
            word.IsFavorite = updateWordDto.IsFavorite;
            word.LearningStatus = updateWordDto.LearningStatus;
            word.UpdatedDate = DateTime.UtcNow;
            word.UpdatedBy = userId;

            // Re-evaluate learning status in case counts are already populated and they manually changed it, 
            // or if they force change the status we respect it.
            // Let's keep the user's manual choice or recompute. If they explicitly change it in update, we save it.

            _context.Words.Update(word);
            await _context.SaveChangesAsync();

            return MapToDto(word);
        }

        public async Task<bool> DeleteWordAsync(int id, string userId)
        {
            var word = await _context.Words
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (word == null) return false;

            _context.Words.Remove(word);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleFavoriteAsync(int id, string userId)
        {
            var word = await _context.Words
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (word == null) return false;

            word.IsFavorite = !word.IsFavorite;
            word.UpdatedDate = DateTime.UtcNow;
            word.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return word.IsFavorite;
        }

        public async Task<WordDto?> GetRandomWordForReviewAsync(string userId)
        {
            // Flashcard: Get a random word for review.
            // We can prioritize words with LearningStatus != Learned, or just a completely random word.
            // Let's select words that are not reviewed or needs review first. If none, select any.
            var query = _context.Words.Where(w => w.UserId == userId);
            
            var unlearnedWords = await query
                .Where(w => w.LearningStatus != LearningStatusEnum.Learned)
                .ToListAsync();

            var listToSelectFrom = unlearnedWords.Any() ? unlearnedWords : await query.ToListAsync();
            if (!listToSelectFrom.Any()) return null;

            var random = new Random();
            var randomWord = listToSelectFrom[random.Next(listToSelectFrom.Count)];

            return MapToDto(randomWord);
        }

        public async Task<bool> SubmitReviewResultAsync(int wordId, bool isCorrect, string userId)
        {
            var word = await _context.Words
                .FirstOrDefaultAsync(w => w.Id == wordId && w.UserId == userId);

            if (word == null) return false;

            // Increment count
            word.ReviewCount++;
            if (isCorrect)
            {
                word.CorrectAnswerCount++;
            }
            else
            {
                word.WrongAnswerCount++;
            }

            // Update status using the business logic algorithm
            UpdateLearningStatus(word);
            word.UpdatedDate = DateTime.UtcNow;
            word.UpdatedBy = userId;

            // Save to ReviewHistory
            var history = new ReviewHistory
            {
                WordId = word.Id,
                UserId = userId,
                ReviewDate = DateTime.UtcNow,
                IsCorrect = isCorrect
            };

            await _context.ReviewHistories.AddAsync(history);
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

        private WordDto MapToDto(Word word)
        {
            return new WordDto
            {
                Id = word.Id,
                EnglishWord = word.EnglishWord,
                TurkishMeaning = word.TurkishMeaning,
                WordType = word.WordType,
                ExampleSentence = word.ExampleSentence,
                ExampleSentenceTranslation = word.ExampleSentenceTranslation,
                LearningStatus = word.LearningStatus,
                IsFavorite = word.IsFavorite,
                ReviewCount = word.ReviewCount,
                CorrectAnswerCount = word.CorrectAnswerCount,
                WrongAnswerCount = word.WrongAnswerCount,
                CreatedDate = word.CreatedDate,
                UpdatedDate = word.UpdatedDate,
                UserId = word.UserId
            };
        }
    }
}
