// IngilizceProje/Infrastructure/Services/IGeminiChatService.cs
using System.Threading.Tasks;
using IngilizceProje.Application.DTOs;

namespace IngilizceProje.Infrastructure.Services
{
    public interface IGeminiChatService
    {
        Task<AiChatResponseDto> GetChatReplyAsync(string userId, AiChatRequestDto requestDto);
    }
}
