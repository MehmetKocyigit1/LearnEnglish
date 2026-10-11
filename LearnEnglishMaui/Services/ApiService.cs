// IngilizceProje/MAUIClient/Services/ApiService.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using IngilizceProje.Application.DTOs;

namespace IngilizceProjeMAUI.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private static readonly string BaseUrl = DeviceInfo.Platform == DevicePlatform.Android 
            ? "https://localhost:7006"
            : "http://localhost:5031";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ApiService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public static async Task SetTokenAsync(string token, string userName, string email)
        {
            Preferences.Set("auth_token", token);
            Preferences.Set("user_name", userName);
            Preferences.Set("user_email", email);

            try
            {
                await SecureStorage.SetAsync("auth_token", token);
                await SecureStorage.SetAsync("user_name", userName);
                await SecureStorage.SetAsync("user_email", email);
            }
            catch
            {
                // Platform fallback to Preferences
            }
        }

        public static async Task<string> GetTokenAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(token)) return token;
            }
            catch
            {
                // Fallback
            }

            return Preferences.Get("auth_token", string.Empty);
        }

        public static async Task ClearTokenAsync()
        {
            Preferences.Remove("auth_token");
            Preferences.Remove("user_name");
            Preferences.Remove("user_email");

            try
            {
                SecureStorage.Remove("auth_token");
                SecureStorage.Remove("user_name");
                SecureStorage.Remove("user_email");
            }
            catch
            {
            }
            await Task.CompletedTask;
        }

        private async Task AddAuthHeaderAsync()
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var result = await RegisterWithDetailsAsync(dto);
            return result.Success;
        }

        public async Task<(bool Success, string Message)> RegisterWithDetailsAsync(RegisterDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/register", dto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Kayıt başarıyla tamamlandı.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    try
                    {
                        var jsonDoc = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
                        if (jsonDoc.TryGetProperty("message", out var msgProp))
                        {
                            return (false, msgProp.GetString() ?? "Kayıt başarısız.");
                        }
                    }
                    catch { }
                    return (false, "Bu kullanıcı adı veya e-posta zaten kullanımda, ya da şifreniz en az 6 karakter olmalıdır.");
                }

                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Kayıt başarısız oldu." : err);
            }
            catch (Exception ex)
            {
                return (false, $"Sunucuya bağlanılamadı: {ex.Message}");
            }
        }

        public async Task<bool> LoginAsync(LoginDto dto)
        {
            var result = await LoginWithDetailsAsync(dto);
            return result.Success;
        }

        public async Task<(bool Success, string Message)> LoginWithDetailsAsync(LoginDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", dto);
                if (response.IsSuccessStatusCode)
                {
                    var tokenDto = await response.Content.ReadFromJsonAsync<TokenDto>(_jsonOptions);
                    if (tokenDto != null && !string.IsNullOrEmpty(tokenDto.Token))
                    {
                        await SetTokenAsync(tokenDto.Token, tokenDto.UserName, tokenDto.Email);
                        return (true, string.Empty);
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return (false, "Kullanıcı adı/e-posta veya şifre hatalı.");
                }

                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Giriş başarısız. Lütfen bilgilerinizi kontrol edin." : err);
            }
            catch (Exception ex)
            {
                return (false, $"Sunucuya bağlanılamadı (port 5031): {ex.Message}");
            }
        }

        public async Task LogoutAsync()
        {
            await ClearTokenAsync();
        }

        public async Task<IEnumerable<WordDto>> GetWordsAsync()
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.GetAsync("/api/words");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<IEnumerable<WordDto>>(_jsonOptions) 
                           ?? Array.Empty<WordDto>();
                }
                return Array.Empty<WordDto>();
            }
            catch
            {
                return Array.Empty<WordDto>();
            }
        }

        public Task<IEnumerable<WordDto>> GetAllWordsAsync() => GetWordsAsync();

        public async Task<WordDto?> GetWordByIdAsync(int id)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.GetAsync($"/api/words/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<WordDto>(_jsonOptions);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateWordAsync(CreateWordDto dto)
        {
            var result = await CreateWordWithResultAsync(dto);
            return result.Success;
        }

        public async Task<(bool Success, WordDto? Word, string ErrorMessage)> CreateWordWithResultAsync(CreateWordDto dto)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.PostAsJsonAsync("/api/words", dto);
                if (response.IsSuccessStatusCode)
                {
                    var word = await response.Content.ReadFromJsonAsync<WordDto>(_jsonOptions);
                    return (true, word, string.Empty);
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, null, err);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<bool> DeleteWordByEnglishAsync(string englishWord)
        {
            try
            {
                var words = await GetAllWordsAsync();
                var match = words.FirstOrDefault(w => string.Equals(w.EnglishWord, englishWord.Trim(), StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return await DeleteWordAsync(match.Id);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateWordAsync(int id, UpdateWordDto dto)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.PutAsJsonAsync($"/api/words/{id}", dto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteWordAsync(int id)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.DeleteAsync($"/api/words/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleFavoriteAsync(int id)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.PutAsync($"/api/words/{id}/favorite", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<WordDto?> GetRandomWordForReviewAsync()
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.GetAsync("/api/words/random-review");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<WordDto>(_jsonOptions);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SubmitReviewResultAsync(int id, bool isCorrect)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.PostAsync($"/api/words/{id}/review?isCorrect={isCorrect}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<MatchingQuizDto?> GenerateMatchingQuizAsync()
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.GetAsync("/api/quizzes/matching");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<MatchingQuizDto>(_jsonOptions);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<TestDto?> GenerateMultipleChoiceTestAsync()
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.GetAsync("/api/quizzes/multiple-choice");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TestDto>(_jsonOptions);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SubmitQuizResultAsync(QuizResultDto dto)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.PostAsJsonAsync("/api/quizzes/submit-result", dto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<StatisticsDto?> GetUserStatisticsAsync()
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.GetAsync("/api/statistics/summary");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<StatisticsDto>(_jsonOptions);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DailyGoalDto?> GetDailyGoalAsync()
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.GetAsync("/api/statistics/daily-goal");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DailyGoalDto>(_jsonOptions);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SetDailyGoalAsync(int targetCount)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.PostAsync($"/api/statistics/daily-goal?targetCount={targetCount}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public string GetExportExcelUrl()
        {
            return $"{BaseUrl}/api/words/export/excel";
        }

        public string GetExportCsvUrl()
        {
            return $"{BaseUrl}/api/words/export/csv";
        }

        public async Task<byte[]?> DownloadExportFileAsync(string relativeUrl)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.GetAsync(relativeUrl);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<AiChatResponseDto?> SendAiChatMessageAsync(AiChatRequestDto dto)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.PostAsJsonAsync("/api/ai-chat/send", dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AiChatResponseDto>(_jsonOptions);
                }
                return new AiChatResponseDto { Reply = $"Hata oluştu: Sunucu hatası ({response.StatusCode})" };
            }
            catch (Exception ex)
            {
                return new AiChatResponseDto { Reply = $"Bağlantı hatası: {ex.Message}" };
            }
        }
    }
}
