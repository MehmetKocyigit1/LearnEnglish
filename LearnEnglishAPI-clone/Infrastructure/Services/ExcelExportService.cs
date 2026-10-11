// IngilizceProje/Infrastructure/Services/ExcelExportService.cs
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using IngilizceProje.Application.Interfaces;
using IngilizceProje.Infrastructure.Data;

namespace IngilizceProje.Infrastructure.Services
{
    public class ExcelExportService : IExcelExportService
    {
        private readonly AppDbContext _context;

        public ExcelExportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportToExcelAsync(string userId)
        {
            var words = await _context.Words
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.EnglishWord)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Kelimelerim");

                // Headers
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "İngilizce Kelime";
                worksheet.Cell(1, 3).Value = "Türkçe Anlamı";
                worksheet.Cell(1, 4).Value = "Kelime Türü";
                worksheet.Cell(1, 5).Value = "Örnek Cümle";
                worksheet.Cell(1, 6).Value = "Örnek Cümle Çevirisi";
                worksheet.Cell(1, 7).Value = "Öğrenme Durumu";
                worksheet.Cell(1, 8).Value = "Favori Mi?";
                worksheet.Cell(1, 9).Value = "Toplam İnceleme";
                worksheet.Cell(1, 10).Value = "Doğru Cevap";
                worksheet.Cell(1, 11).Value = "Yanlış Cevap";
                worksheet.Cell(1, 12).Value = "Ekleme Tarihi";

                // Header Styling
                var headerRange = worksheet.Range("A1:L1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
                headerRange.Style.Font.FontColor = XLColor.White;

                // Data Rows
                int row = 2;
                foreach (var word in words)
                {
                    worksheet.Cell(row, 1).Value = word.Id;
                    worksheet.Cell(row, 2).Value = word.EnglishWord;
                    worksheet.Cell(row, 3).Value = word.TurkishMeaning;
                    worksheet.Cell(row, 4).Value = word.WordType.ToString();
                    worksheet.Cell(row, 5).Value = word.ExampleSentence;
                    worksheet.Cell(row, 6).Value = word.ExampleSentenceTranslation;
                    worksheet.Cell(row, 7).Value = word.LearningStatus.ToString();
                    worksheet.Cell(row, 8).Value = word.IsFavorite ? "Evet" : "Hayır";
                    worksheet.Cell(row, 9).Value = word.ReviewCount;
                    worksheet.Cell(row, 10).Value = word.CorrectAnswerCount;
                    worksheet.Cell(row, 11).Value = word.WrongAnswerCount;
                    worksheet.Cell(row, 12).Value = word.CreatedDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<byte[]> ExportToCsvAsync(string userId)
        {
            var words = await _context.Words
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.EnglishWord)
                .ToListAsync();

            var csvBuilder = new StringBuilder();
            
            // BOM for UTF-8 compatibility with Excel
            csvBuilder.Append('\uFEFF');

            // Headers
            csvBuilder.AppendLine("ID;İngilizce Kelime;Türkçe Anlamı;Kelime Türü;Örnek Cümle;Örnek Cümle Çevirisi;Öğrenme Durumu;Favori Mi?;Toplam İnceleme;Doğru Cevap;Yanlış Cevap;Ekleme Tarihi");

            foreach (var word in words)
            {
                csvBuilder.AppendLine(string.Join(";", 
                    word.Id,
                    EscapeCsv(word.EnglishWord),
                    EscapeCsv(word.TurkishMeaning),
                    word.WordType,
                    EscapeCsv(word.ExampleSentence),
                    EscapeCsv(word.ExampleSentenceTranslation),
                    word.LearningStatus,
                    word.IsFavorite ? "Evet" : "Hayır",
                    word.ReviewCount,
                    word.CorrectAnswerCount,
                    word.WrongAnswerCount,
                    word.CreatedDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                ));
            }

            return Encoding.UTF8.GetBytes(csvBuilder.ToString());
        }

        private string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            
            // Replace semicolons and newlines to prevent CSV breakdown
            var cleanText = text.Replace(";", ",").Replace("\r", " ").Replace("\n", " ");
            if (cleanText.Contains("\""))
            {
                cleanText = cleanText.Replace("\"", "\"\"");
            }
            return $"\"{cleanText}\"";
        }
    }
}
