// IngilizceProje/MAUIClient/Services/GeminiAiService.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace IngilizceProjeMAUI.Services
{
    public class AiWordAnalysisResult
    {
        public string Turkish { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Kolay";
        public int DifficultyLevel { get; set; } = 1;
        public string ExampleSentence { get; set; } = string.Empty;
        public string ExampleSentenceTranslation { get; set; } = string.Empty;
        public string WordType { get; set; } = "Noun";

        // AI Validation & Enrichment Features
        public bool IsSpellingCorrected { get; set; } = false;
        public string CorrectedEnglish { get; set; } = string.Empty;
        public string PrimaryCommonMeaning { get; set; } = string.Empty;
        public string EnrichedTurkishMeaning { get; set; } = string.Empty;
        public string ValidationFeedback { get; set; } = string.Empty;

        public IngilizceProje.Domain.Enums.WordTypeEnum GetWordTypeEnum()
        {
            var wt = WordType?.Trim().ToLowerInvariant() ?? "";
            if (wt.Contains("verb") || wt.Contains("fiil")) return IngilizceProje.Domain.Enums.WordTypeEnum.Verb;
            if (wt.Contains("adj") || wt.Contains("sıfat") || wt.Contains("sifat")) return IngilizceProje.Domain.Enums.WordTypeEnum.Adjective;
            if (wt.Contains("adv") || wt.Contains("zarf")) return IngilizceProje.Domain.Enums.WordTypeEnum.Adverb;
            if (wt.Contains("prep") || wt.Contains("edat")) return IngilizceProje.Domain.Enums.WordTypeEnum.Preposition;
            if (wt.Contains("phras") || wt.Contains("deyim") || wt.Contains("kalıp")) return IngilizceProje.Domain.Enums.WordTypeEnum.Verb;
            return IngilizceProje.Domain.Enums.WordTypeEnum.Noun;
        }
    }

    public class GeminiAiService
    {
        private readonly HttpClient _httpClient = new();
        private const string DefaultApiKey = "YOUR_GEMINI_API_KEY";

        public static string GetApiKey()
        {
            var key = Preferences.Get("gemini_api_key", string.Empty);
            if (!string.IsNullOrWhiteSpace(key)) return key;
            return DefaultApiKey;
        }

        public static void SetApiKey(string key)
        {
            Preferences.Set("gemini_api_key", key);
        }

        public async Task<AiWordAnalysisResult> AnalyzeAndExplainWordAsync(string englishWord, string? userTurkishMeaning = null)
        {
            if (string.IsNullOrWhiteSpace(englishWord))
            {
                return new AiWordAnalysisResult();
            }

            var cleanWord = englishWord.Trim();
            var userMeaning = string.IsNullOrWhiteSpace(userTurkishMeaning) ? string.Empty : userTurkishMeaning.Trim();
            var apiKey = GetApiKey();

            var meaningClause = string.IsNullOrWhiteSpace(userMeaning)
                ? "Öğrenci herhangi bir Türkçe anlam belirtmedi; anlamı senin tamamen en yaygın ve doğru şekilde belirlemeni bekliyor."
                : $"Öğrencinin girdiği Türkçe anlam: \"{userMeaning}\"";

            var prompt = $@"Sen uzman, titiz ve pedagojik bir İngilizce başöğretmenisin.
Öğrenci kişisel kelime havuzuna şu kelimeyi ekliyor: ""{cleanWord}"".
{meaningClause}

GÖREVLERİN (KESİNLİKLE UYULMASI GEREKEN KURALLAR):
1) YAZIM DOĞRULUĞU DENETİMİ (Spell Check):
- ""{cleanWord}"" kelimesinde bir yazım yanlışı, harf eksiği/fazlası veya typo var mı? (Örn: 'accomodation' -> 'accommodation', 'embarassed' -> 'embarrassed', 'definately' -> 'definitely', 'aple' -> 'apple').
- Yanlış yazılmışsa: 'isSpellingCorrected': true yap ve 'correctedEnglish' alanına DOĞRU yazımını yaz.
- Doğru yazılmışsa: 'isSpellingCorrected': false yap ve 'correctedEnglish': ""{cleanWord}"" yaz.

2) EN YAYGIN ANLAMI BELİRLEME VE ZENGİNLEŞTİRME HAKKI:
- Kelimenin günlük İngilizcede EN ÇOK KULLANILAN, birincil (primary/most common) temel Türkçe anlamını belirle ('primaryCommonMeaning').
- Eğer öğrenci bir Türkçe anlam girdiyse:
  * Öğrencinin anlamı doğru olabilir ancak kelimenin ÇOK DAHA YAYGIN / TEMEL bir birincil anlamı varsa (Örn: 'run' için sadece 'çalıştırmak' demişse en yaygın anlamı 'koşmak'tır; 'bank' için sadece 'nehir kıyısı' demişse en yaygın anlamı 'banka'dır; 'bark' için 'ağaç kabuğu' demişse en yaygın anlamı 'havlamak'tır), yapay zekâ olarak EN YAYGIN anlamı başa ekleme hakkın var! Anlamı birleştirip zenginleştir (Örn: ""koşmak; çalıştırmak, yönetmek"").
  * Eğer öğrencinin girdiği anlam hatalı veya alakasızsa, doğrusunu ve en yaygın anlamını yaz.
  * Bu nihai zenginleştirilmiş anlamı 'enrichedTurkishMeaning' alanına yaz.
- Eğer öğrenci Türkçe anlam girmediyse:
  * En yaygın ve temel Türkçe anlamları 'enrichedTurkishMeaning' ve 'turkish' alanına yaz.

3) PEDAGOJİK GERİ BİLDİRİM (validationFeedback):
- Eğer kelimenin yazımını düzelttiysen ve/veya öğrencinin girdiği anlama ek olarak daha yaygın temel anlamını eklediysen, öğrenciye bunu kibar, öğretici ve kısa bir dille açıkla.
- Örnek: ""Yazım 'accommodation' olarak düzeltildi ve en yaygın anlamı olan 'konaklama' başa eklendi."" veya ""Kelimenin en çok kullanılan 'koşmak' anlamı listenize eklendi: koşmak; çalıştırmak.""
- Değişiklik veya ekleme gerekmediyse boş string bırak ("""").

4) AÇIKLAMA (explanation) ALANI İÇİN KESİN KURALLAR:
1) NASIL KULLANILIR?: Kelimenin türü, edat/kalıp kullanımları (phrasal verbs/collocations), cümledeki konumu.
2) BAŞKA HANGİ ANLAMLARDA KULLANILIR?: Farklı bağlamlardaki, mecaz ya da günlük dildeki diğer anlamları.
Açıklamayı çok net, anlaşılır ve 2-4 cümlelik zengin bir rehber şeklinde yaz.

JSON Formatı (SADECE JSON DÖNDÜR):
{{
  ""isSpellingCorrected"": false,
  ""correctedEnglish"": ""kelimenin_dogru_hali"",
  ""primaryCommonMeaning"": ""en_yaygin_temel_anlam"",
  ""enrichedTurkishMeaning"": ""nihai_zenginlestirilmis_anlam"",
  ""validationFeedback"": ""ogrenciye_geribildirim_veya_bos"",
  ""turkish"": ""en_yaygin_turkce_anlam"",
  ""wordType"": ""Noun"" veya ""Verb"" veya ""Adjective"" veya ""Adverb"" veya ""PhrasalVerb"",
  ""explanation"": ""1) Nasıl Kullanılır: ... 2) Diğer Anlamları: ..."",
  ""difficulty"": ""Kolay"" veya ""Orta"" veya ""Zor"",
  ""difficultyLevel"": 1,
  ""exampleSentence"": ""Kelimenin kullanımını en iyi gösteren doğal İngilizce örnek cümle."",
  ""exampleSentenceTranslation"": ""Örnek cümlenin akıcı Türkçe çevirisi.""
}}
JSON dışında hiçbir selamlama, markdown işareti veya ek metin yazma.";

            // Candidate models in order of preference
            string[] models = new[] { "gemini-3.8-flash", "gemini-flash-latest", "gemini-2.5-flash-lite" };

            foreach (var model in models)
            {
                try
                {
                    string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                    var payload = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                role = "user",
                                parts = new[] { new { text = prompt } }
                            }
                        }
                    };

                    var response = await _httpClient.PostAsJsonAsync(url, payload);
                    if (response.IsSuccessStatusCode)
                    {
                        using var stream = await response.Content.ReadAsStreamAsync();
                        using var doc = await JsonDocument.ParseAsync(stream);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                        {
                            var candidate = candidates[0];
                            if (candidate.TryGetProperty("content", out var content) &&
                                content.TryGetProperty("parts", out var parts) &&
                                parts.GetArrayLength() > 0)
                            {
                                var text = parts[0].GetProperty("text").GetString();
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    var result = ParseAiResponseJson(text, cleanWord);
                                    if (result != null && !string.IsNullOrWhiteSpace(result.Explanation))
                                    {
                                        return result;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"[GeminiAiService] Model {model} failed with status {response.StatusCode}: {err}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[GeminiAiService] Model {model} exception: {ex.Message}");
                }
            }

            return GenerateFallbackAnalysis(cleanWord);
        }

        private static AiWordAnalysisResult ParseAiResponseJson(string rawText, string originalWord)
        {
            try
            {
                var clean = rawText.Trim();
                if (clean.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                {
                    clean = clean.Substring(7);
                }
                else if (clean.StartsWith("```"))
                {
                    clean = clean.Substring(3);
                }

                if (clean.EndsWith("```"))
                {
                    clean = clean.Substring(0, clean.Length - 3);
                }
                clean = clean.Trim();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<AiWordAnalysisResult>(clean, options);
                if (result != null)
                {
                    if (string.IsNullOrWhiteSpace(result.CorrectedEnglish))
                    {
                        result.CorrectedEnglish = originalWord;
                    }

                    if (string.IsNullOrWhiteSpace(result.EnrichedTurkishMeaning))
                    {
                        result.EnrichedTurkishMeaning = !string.IsNullOrWhiteSpace(result.Turkish) 
                            ? result.Turkish 
                            : result.PrimaryCommonMeaning;
                    }

                    if (string.IsNullOrWhiteSpace(result.Turkish))
                    {
                        result.Turkish = result.EnrichedTurkishMeaning;
                    }

                    if (result.DifficultyLevel <= 0)
                    {
                        result.DifficultyLevel = result.Difficulty?.ToLower() switch
                        {
                            "kolay" => 1,
                            "zor" => 3,
                            _ => 2
                        };
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GeminiAiService] JSON parse error: {ex.Message}");
            }

            return GenerateFallbackAnalysis(originalWord);
        }

        private static AiWordAnalysisResult GenerateFallbackAnalysis(string englishWord)
        {
            int diffLevel = englishWord.Length switch
            {
                <= 6 => 1,
                >= 12 => 3,
                _ => 2
            };

            string diffStr = diffLevel switch
            {
                1 => "Kolay",
                2 => "Orta",
                _ => "Zor"
            };

            return new AiWordAnalysisResult
            {
                CorrectedEnglish = englishWord,
                EnrichedTurkishMeaning = englishWord,
                PrimaryCommonMeaning = englishWord,
                IsSpellingCorrected = false,
                Turkish = $"{englishWord}",
                Explanation = $"⚠️ Çevrimdışı Mod: Yapay zekâ analizine ulaşılamadı. İnternet bağlantınızı kontrol edip '✨ Yenile' butonuna basarak '{englishWord}' kelimesinin nasıl ve başka hangi anlamlarda kullanıldığını getirebilirsiniz.",
                Difficulty = diffStr,
                DifficultyLevel = diffLevel,
                ExampleSentence = $"Practice using '{englishWord}' in different contexts to master its meanings.",
                ExampleSentenceTranslation = $"Farklı anlamlarına hakim olmak için '{englishWord}' kelimesini farklı bağlamlarda kullanma alıştırması yapın."
            };
        }
    }
}
