// IngilizceProje/Application/Interfaces/IQuizService.cs
using System.Threading.Tasks;
using IngilizceProje.Application.DTOs;

namespace IngilizceProje.Application.Interfaces
{
    public interface IQuizService
    {
        Task<MatchingQuizDto?> GenerateMatchingQuizAsync(string userId);
        Task<TestDto?> GenerateMultipleChoiceTestAsync(string userId);
        Task<bool> SubmitQuizResultAsync(QuizResultDto resultDto, string userId);
    }
}
