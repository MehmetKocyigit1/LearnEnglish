// IngilizceProje/Application/Interfaces/IStatisticsService.cs
using System.Threading.Tasks;
using IngilizceProje.Application.DTOs;

namespace IngilizceProje.Application.Interfaces
{
    public interface IStatisticsService
    {
        Task<StatisticsDto> GetUserStatisticsAsync(string userId);
        Task<bool> SetDailyGoalAsync(int targetCount, string userId);
        Task<DailyGoalDto?> GetDailyGoalAsync(string userId);
    }
}
