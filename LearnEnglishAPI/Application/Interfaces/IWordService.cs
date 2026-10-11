// IngilizceProje/Application/Interfaces/IWordService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using IngilizceProje.Application.DTOs;

namespace IngilizceProje.Application.Interfaces
{
    public interface IWordService
    {
        Task<IEnumerable<WordDto>> GetAllWordsAsync(string userId);
        Task<WordDto?> GetWordByIdAsync(int id, string userId);
        Task<WordDto> CreateWordAsync(CreateWordDto createWordDto, string userId);
        Task<WordDto?> UpdateWordAsync(UpdateWordDto updateWordDto, string userId);
        Task<bool> DeleteWordAsync(int id, string userId);
        Task<bool> ToggleFavoriteAsync(int id, string userId);
        Task<WordDto?> GetRandomWordForReviewAsync(string userId);
        Task<bool> SubmitReviewResultAsync(int wordId, bool isCorrect, string userId);
    }
}
