// IngilizceProje/Application/Interfaces/IExcelExportService.cs
using System.Threading.Tasks;

namespace IngilizceProje.Application.Interfaces
{
    public interface IExcelExportService
    {
        Task<byte[]> ExportToExcelAsync(string userId);
        Task<byte[]> ExportToCsvAsync(string userId);
    }
}
