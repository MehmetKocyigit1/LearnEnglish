// IngilizceProje/Infrastructure/Services/GeminiChatService.cs
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using IngilizceProje.Application.DTOs;
using IngilizceProje.Infrastructure.Data;

namespace IngilizceProje.Infrastructure.Services
{
    public class GeminiChatService : IGeminiChatService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GeminiChatService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<AiChatResponseDto> GetChatReplyAsync(string userId, AiChatRequestDto requestDto)
        {
            try
            {
                // Fetch the user's last 50 words to feed to the AI context
                var userWords = await _context.Words
                    .Where(w => w.UserId == userId)
                    .OrderByDescending(w => w.CreatedDate)
                    .Take(50)
                    .Select(w => $"{w.EnglishWord} ({w.TurkishMeaning})")
                    .ToListAsync();

                string wordsContext = userWords.Any() 
                    ? string.Join(", ", userWords) 
                    : "Henüz kelime eklenmemiş.";

                // Define the system instruction as required
                string systemInstructionText = $"Sen profesyonel, sabırlı ve motive edici bir İngilizce öğretmenisin (AI Başöğretmen). " +
                    $"Görevin: Öğrencinin İngilizce seviyesini geliştirmek, gramer ve kelime pratiklerinde rehberlik etmek, sorularını öğretici ve net bir dille yanıtlamaktır.\n\n" +
                    $"Kurallar ve İletişim Tarzı:\n" +
                    $"1. Her zaman nazik, öğretici, cesaretlendirici ve anlaşılır bir öğretmen üslubu benimse.\n" +
                    $"2. Öğrenci Türkçe yazdığında açıklamalarını anlaşılır bir dille yap, konuyu pekiştirmek için İngilizce örnek cümleler ve Türkçe karşılıklarını ver.\n" +
                    $"3. Öğrenci İngilizce yazdığında veya pratik yaptığında, yaptığı dilbilgisi/kelime hatalarını nazikçe düzeltip doğrusunu göster.\n" +
                    $"4. Mobil sohbete uygun olacak şekilde gereksiz laf kalabalığından kaçın; açıklamaların öz, net ve akıcı olsun.\n" +
                    $"5. Gereksiz ve alakasız metaforlar (yerçekimi, uzay vb.) kullanma, doğrudan dil öğretimine odaklan.\n\n" +
                    $"ÖĞRENCİNİN KELİME HAVUZU: {wordsContext}. " +
                    $"Eğer öğrenci kelimeleriyle ilgili bir şey sorarsa veya pratik yapmak isterse bu listedeki kelimelerden yararlan ve onları cümle içinde örneklendir.";

                var configuredKey = _configuration["Gemini:ApiKey"];
                const string fallbackGeminiKey = "YOUR_GEMINI_API_KEY";
                string effectiveKey = string.IsNullOrWhiteSpace(configuredKey) || configuredKey == "YOUR_GEMINI_API_KEY"
                    ? fallbackGeminiKey
                    : configuredKey.Trim();

                HttpResponseMessage? response = null;

                // 1. If Hugging Face token is provided (starts with "hf_")
                if (effectiveKey.StartsWith("hf_"))
                {
                    try
                    {
                        var hfUrl = "https://router.huggingface.co/v1/chat/completions";
                        var hfRequest = new HttpRequestMessage(HttpMethod.Post, hfUrl);
                        hfRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", effectiveKey);
                        var hfPayload = new
                        {
                            model = "meta-llama/Llama-3.3-70B-Instruct",
                            messages = new[]
                            {
                                new { role = "system", content = systemInstructionText },
                                new { role = "user", content = requestDto.Message }
                            },
                            max_tokens = 1000
                        };
                        hfRequest.Content = JsonContent.Create(hfPayload);
                        response = await _httpClient.SendAsync(hfRequest);

                        if (response.IsSuccessStatusCode)
                        {
                            using var s = await response.Content.ReadAsStreamAsync();
                            using var doc = await JsonDocument.ParseAsync(s);
                            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                            {
                                var choice = choices[0];
                                if (choice.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var cStr))
                                {
                                    var rep = cStr.GetString();
                                    if (!string.IsNullOrEmpty(rep)) return new AiChatResponseDto { Reply = rep.Trim() };
                                }
                            }
                        }
                    }
                    catch { /* Fallback to Google Gemini below */ }

                    // Hugging Face didn't succeed (e.g. 402 no credits) -> seamless fallback to Gemini
                    effectiveKey = fallbackGeminiKey;
                }
                else if (effectiveKey.StartsWith("sk-or-v1-"))
                {
                    // OpenRouter Call
                    try
                    {
                        var openRouterUrl = "https://openrouter.ai/api/v1/chat/completions";
                        var request = new HttpRequestMessage(HttpMethod.Post, openRouterUrl);
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", effectiveKey);
                        request.Headers.Add("HTTP-Referer", "http://localhost:5030");
                        request.Headers.Add("X-Title", "Antigravity English");

                        var openRouterPayload = new
                        {
                            model = "google/gemini-2.5-flash",
                            messages = new[]
                            {
                                new { role = "system", content = systemInstructionText },
                                new { role = "user", content = requestDto.Message }
                            },
                            max_tokens = 1000
                        };

                        request.Content = JsonContent.Create(openRouterPayload);
                        response = await _httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            using var s = await response.Content.ReadAsStreamAsync();
                            using var doc = await JsonDocument.ParseAsync(s);
                            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                            {
                                var choice = choices[0];
                                if (choice.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var cStr))
                                {
                                    var rep = cStr.GetString();
                                    if (!string.IsNullOrEmpty(rep)) return new AiChatResponseDto { Reply = rep.Trim() };
                                }
                            }
                        }
                    }
                    catch { effectiveKey = fallbackGeminiKey; }
                }

                // 2. Google Gemini API Call (Primary or Fallback)
                var geminiPayload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = requestDto.Message } }
                        }
                    },
                    systemInstruction = new
                    {
                        parts = new[] { new { text = systemInstructionText } }
                    }
                };

                // Try gemini-3.8-flash
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.8-flash:generateContent?key={effectiveKey}";
                response = await _httpClient.PostAsJsonAsync(url, geminiPayload);

                if (!response.IsSuccessStatusCode)
                {
                    string fallbackUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={effectiveKey}";
                    response = await _httpClient.PostAsJsonAsync(fallbackUrl, geminiPayload);
                }

                if (!response.IsSuccessStatusCode && effectiveKey != fallbackGeminiKey)
                {
                    // If custom key failed, retry with reliable fallback key
                    string retryUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.8-flash:generateContent?key={fallbackGeminiKey}";
                    response = await _httpClient.PostAsJsonAsync(retryUrl, geminiPayload);
                }

                if (response != null && response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    using var doc = await JsonDocument.ParseAsync(responseStream);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];
                        if (candidate.TryGetProperty("content", out var content) && 
                            content.TryGetProperty("parts", out var parts) && 
                            parts.GetArrayLength() > 0)
                        {
                            var reply = parts[0].GetProperty("text").GetString();
                            if (!string.IsNullOrEmpty(reply))
                            {
                                return new AiChatResponseDto { Reply = reply.Trim() };
                            }
                        }
                    }
                }
                
                return new AiChatResponseDto 
                { 
                    Reply = "Başöğretmen şu anda bağlantı güncellemesi alıyor. Lütfen sorunuzu bir kez daha iletin." 
                };
            }
            catch (Exception ex)
            {
                return new AiChatResponseDto 
                { 
                    Reply = $"Hata oluştu: {ex.Message}. Başöğretmen'e bağlanırken teknik bir aksaklık yaşandı." 
                };
            }
        }
    }
}
