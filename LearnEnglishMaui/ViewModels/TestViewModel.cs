// IngilizceProje/MAUIClient/ViewModels/TestViewModel.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngilizceProje.Application.DTOs;
using IngilizceProjeMAUI.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace IngilizceProjeMAUI.ViewModels
{
    public partial class TestViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly Random _random = new();
        private List<TechWordModel>? _cachedDatasetWords;

        [ObservableProperty]
        private TestDto? _currentTest;

        [ObservableProperty]
        private bool _isAnswered;

        [ObservableProperty]
        private string _selectedOption = string.Empty;

        [ObservableProperty]
        private string _correctOption = string.Empty;

        [ObservableProperty]
        private bool _isCorrect;

        [ObservableProperty]
        private string _feedbackText = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _hasTest;

        [ObservableProperty]
        private string _infoMessage = string.Empty;

        [ObservableProperty]
        private string _optionA = string.Empty;
        [ObservableProperty]
        private string _optionB = string.Empty;
        [ObservableProperty]
        private string _optionC = string.Empty;
        [ObservableProperty]
        private string _optionD = string.Empty;

        // Dynamic Color properties for UI binding
        public Color OptionAColor => GetOptionColor(OptionA);
        public Color OptionBColor => GetOptionColor(OptionB);
        public Color OptionCColor => GetOptionColor(OptionC);
        public Color OptionDColor => GetOptionColor(OptionD);

        public Color OptionATextColor => GetOptionTextColor(OptionA);
        public Color OptionBTextColor => GetOptionTextColor(OptionB);
        public Color OptionCTextColor => GetOptionTextColor(OptionC);
        public Color OptionDTextColor => GetOptionTextColor(OptionD);

        public TestViewModel(ApiService apiService)
        {
            _apiService = apiService;
            HasTest = false;
        }

        [RelayCommand]
        public async Task LoadTestAsync()
        {
            IsBusy = true;
            IsAnswered = false;
            SelectedOption = string.Empty;
            CorrectOption = string.Empty;
            FeedbackText = string.Empty;
            CurrentTest = null;
            HasTest = false;
            InfoMessage = string.Empty;

            RefreshOptionColors();

            try
            {
                // 1. Try to load multiple choice test from the backend API
                var test = await _apiService.GenerateMultipleChoiceTestAsync();
                if (test != null && test.Options.Count >= 4)
                {
                    CurrentTest = test;
                    OptionA = test.Options[0];
                    OptionB = test.Options[1];
                    OptionC = test.Options[2];
                    OptionD = test.Options[3];

                    // Fetch correct answer for verification
                    var wordDetails = await _apiService.GetWordByIdAsync(test.WordId);
                    if (wordDetails != null)
                    {
                        CorrectOption = wordDetails.TurkishMeaning;
                    }

                    HasTest = true;
                }
                else
                {
                    // 2. Hybrid fallback: If backend doesn't have 4 words yet, generate using user words & CS/AI dataset
                    await GenerateHybridTestAsync();
                }
            }
            catch
            {
                // If API throws or is offline, generate seamlessly from local/dataset words
                await GenerateHybridTestAsync();
            }
            finally
            {
                IsBusy = false;
                RefreshOptionColors();
            }
        }

        private async Task GenerateHybridTestAsync()
        {
            await EnsureDatasetLoadedAsync();

            try
            {
                var userWords = (await _apiService.GetAllWordsAsync())?.ToList();

                string targetEnglish;
                string targetTurkish;
                string targetSentence;
                int targetWordId;

                // If user has personal words, prioritize testing them!
                if (userWords != null && userWords.Count > 0)
                {
                    var chosen = userWords[_random.Next(userWords.Count)];
                    targetEnglish = chosen.EnglishWord;
                    targetTurkish = chosen.TurkishMeaning;
                    targetSentence = chosen.ExampleSentence;
                    targetWordId = chosen.Id;
                }
                else if (_cachedDatasetWords != null && _cachedDatasetWords.Count > 0)
                {
                    // If no user words yet, test from the 1500+ CS/AI vocabulary
                    var chosen = _cachedDatasetWords[_random.Next(_cachedDatasetWords.Count)];
                    targetEnglish = chosen.English;
                    targetTurkish = chosen.Turkish;
                    targetSentence = chosen.Sentence;
                    targetWordId = -chosen.Id;
                }
                else
                {
                    InfoMessage = "Test sorusu oluşturulamadı.";
                    HasTest = false;
                    return;
                }

                // Gather 3 distinct distractors
                var distractors = new List<string>();
                var candidates = new List<string>();

                if (userWords != null)
                {
                    candidates.AddRange(userWords.Select(w => w.TurkishMeaning));
                }
                if (_cachedDatasetWords != null)
                {
                    candidates.AddRange(_cachedDatasetWords.Select(w => w.Turkish));
                }

                candidates = candidates
                    .Where(c => !string.IsNullOrWhiteSpace(c) && !string.Equals(c.Trim(), targetTurkish.Trim(), StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .OrderBy(_ => _random.Next())
                    .Take(3)
                    .ToList();

                var options = new List<string> { targetTurkish };
                options.AddRange(candidates);

                // If fewer than 4 distractors found, pad with fallback tech terms
                while (options.Count < 4)
                {
                    options.Add($"Seçenek {options.Count + 1}");
                }

                // Shuffle options
                options = options.OrderBy(_ => _random.Next()).ToList();

                CurrentTest = new TestDto
                {
                    WordId = targetWordId,
                    EnglishWord = targetEnglish,
                    ExampleSentence = targetSentence,
                    ExampleSentenceTranslation = string.Empty,
                    Options = options
                };

                OptionA = options[0];
                OptionB = options[1];
                OptionC = options[2];
                OptionD = options[3];
                CorrectOption = targetTurkish;
                HasTest = true;
            }
            catch (Exception ex)
            {
                InfoMessage = $"Test oluşturulamadı: {ex.Message}";
                HasTest = false;
            }
        }

        private async Task EnsureDatasetLoadedAsync()
        {
            if (_cachedDatasetWords != null && _cachedDatasetWords.Count > 0) return;

            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("tech_words_dataset.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _cachedDatasetWords = JsonSerializer.Deserialize<List<TechWordModel>>(json, options);
            }
            catch
            {
                _cachedDatasetWords = new List<TechWordModel>();
            }
        }

        [RelayCommand]
        private async Task SelectOptionAsync(string option)
        {
            if (IsAnswered || CurrentTest == null) return;

            SelectedOption = option;
            IsAnswered = true;
            IsBusy = true;

            RefreshOptionColors();

            try
            {
                if (string.IsNullOrEmpty(CorrectOption) && CurrentTest.WordId > 0)
                {
                    var wordDetails = await _apiService.GetWordByIdAsync(CurrentTest.WordId);
                    if (wordDetails != null)
                    {
                        CorrectOption = wordDetails.TurkishMeaning;
                    }
                }

                IsCorrect = string.Equals(option.Trim(), CorrectOption.Trim(), StringComparison.OrdinalIgnoreCase);

                if (IsCorrect)
                {
                    FeedbackText = "Doğru Cevap! Tebrikler. 🎉";
                }
                else
                {
                    FeedbackText = $"Yanlış Cevap. Doğrusu: '{CorrectOption}' olmalıydı.";
                }

                // Submit result to backend if it corresponds to an existing user word
                if (CurrentTest.WordId > 0)
                {
                    await _apiService.SubmitQuizResultAsync(new QuizResultDto
                    {
                        WordId = CurrentTest.WordId,
                        IsCorrect = IsCorrect
                    });
                }
            }
            catch (Exception ex)
            {
                FeedbackText = $"Sonuç kaydedilemedi: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                RefreshOptionColors();
            }
        }

        private Color GetOptionColor(string option)
        {
            var isLight = Application.Current?.RequestedTheme == AppTheme.Light;
            var defaultColor = isLight ? Color.FromArgb("#F1F5F9") : Color.FromArgb("#1E293B");

            if (string.IsNullOrEmpty(option)) return defaultColor;
            if (!IsAnswered) return defaultColor;

            if (string.Equals(option.Trim(), CorrectOption.Trim(), StringComparison.OrdinalIgnoreCase))
                return Color.FromArgb("#10B981"); // Premium Emerald Green

            if (string.Equals(option.Trim(), SelectedOption.Trim(), StringComparison.OrdinalIgnoreCase))
                return Color.FromArgb("#EF4444"); // Premium Red

            return defaultColor;
        }

        private Color GetOptionTextColor(string option)
        {
            var isLight = Application.Current?.RequestedTheme == AppTheme.Light;
            var defaultText = isLight ? Color.FromArgb("#0F172A") : Color.FromArgb("#F8FAFC");

            if (string.IsNullOrEmpty(option)) return defaultText;
            if (!IsAnswered) return defaultText;

            if (string.Equals(option.Trim(), CorrectOption.Trim(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(option.Trim(), SelectedOption.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Colors.White;
            }

            return defaultText;
        }

        private void RefreshOptionColors()
        {
            OnPropertyChanged(nameof(OptionAColor));
            OnPropertyChanged(nameof(OptionBColor));
            OnPropertyChanged(nameof(OptionCColor));
            OnPropertyChanged(nameof(OptionDColor));

            OnPropertyChanged(nameof(OptionATextColor));
            OnPropertyChanged(nameof(OptionBTextColor));
            OnPropertyChanged(nameof(OptionCTextColor));
            OnPropertyChanged(nameof(OptionDTextColor));
        }
    }
}
