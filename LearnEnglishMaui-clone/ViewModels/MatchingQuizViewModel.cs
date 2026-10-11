// IngilizceProje/MAUIClient/ViewModels/MatchingQuizViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngilizceProje.Application.DTOs;
using IngilizceProjeMAUI.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace IngilizceProjeMAUI.ViewModels
{
    public class MatchingItem : ObservableObject
    {
        public int WordId { get; set; }
        public string EnglishWord { get; set; } = string.Empty;
        public string CorrectMeaning { get; set; } = string.Empty;

        private string _selectedMeaning = string.Empty;
        public string SelectedMeaning
        {
            get => _selectedMeaning;
            set
            {
                if (SetProperty(ref _selectedMeaning, value))
                {
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private bool? _isCorrectMatch;
        public bool? IsCorrectMatch
        {
            get => _isCorrectMatch;
            set => SetProperty(ref _isCorrectMatch, value);
        }

        public bool IsSelected => !string.IsNullOrEmpty(SelectedMeaning);
    }

    public partial class MatchingQuizViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly Random _random = new();
        private List<TechWordModel>? _cachedDatasetWords;

        [ObservableProperty]
        private ObservableCollection<MatchingItem> _quizItems = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _hasChecked;

        [ObservableProperty]
        private string _scoreText = string.Empty;

        [ObservableProperty]
        private bool _canLoadQuiz;

        [ObservableProperty]
        private string _infoMessage = string.Empty;

        public MatchingQuizViewModel(ApiService apiService)
        {
            _apiService = apiService;
            CanLoadQuiz = true;
        }

        [RelayCommand]
        public async Task LoadQuizAsync()
        {
            IsBusy = true;
            HasChecked = false;
            ScoreText = string.Empty;
            QuizItems.Clear();
            InfoMessage = string.Empty;

            try
            {
                var quiz = await _apiService.GenerateMatchingQuizAsync();
                if (quiz != null && quiz.Pairs.Count >= 5)
                {
                    var items = quiz.Pairs.Select(p => new MatchingItem
                    {
                        WordId = p.WordId,
                        EnglishWord = p.EnglishWord,
                        CorrectMeaning = p.TurkishMeaning
                    }).ToList();

                    QuizItems = new ObservableCollection<MatchingItem>(items);
                    CanLoadQuiz = false;
                }
                else
                {
                    await GenerateHybridMatchingQuizAsync();
                }
            }
            catch
            {
                await GenerateHybridMatchingQuizAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GenerateHybridMatchingQuizAsync()
        {
            await EnsureDatasetLoadedAsync();

            try
            {
                var userWords = (await _apiService.GetAllWordsAsync())?.ToList() ?? new List<WordDto>();

                var chosenItems = new List<MatchingItem>();

                // Add available user words
                foreach (var uw in userWords.OrderBy(_ => _random.Next()).Take(5))
                {
                    chosenItems.Add(new MatchingItem
                    {
                        WordId = uw.Id,
                        EnglishWord = uw.EnglishWord,
                        CorrectMeaning = uw.TurkishMeaning
                    });
                }

                // If fewer than 5, supplement from tech dataset
                if (chosenItems.Count < 5 && _cachedDatasetWords != null)
                {
                    var needed = 5 - chosenItems.Count;
                    var techPool = _cachedDatasetWords
                        .Where(tw => !chosenItems.Any(c => string.Equals(c.EnglishWord, tw.English, StringComparison.OrdinalIgnoreCase)))
                        .OrderBy(_ => _random.Next())
                        .Take(needed);

                    foreach (var tw in techPool)
                    {
                        chosenItems.Add(new MatchingItem
                        {
                            WordId = -tw.Id,
                            EnglishWord = tw.English,
                            CorrectMeaning = tw.Turkish
                        });
                    }
                }

                if (chosenItems.Count >= 2)
                {
                    QuizItems = new ObservableCollection<MatchingItem>(chosenItems);
                    CanLoadQuiz = false;
                }
                else
                {
                    InfoMessage = "Quiz yüklenemedi.";
                }
            }
            catch (Exception ex)
            {
                InfoMessage = $"Hata: {ex.Message}";
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
        private async Task CheckAnswersAsync()
        {
            if (QuizItems.Any(item => !item.IsSelected))
            {
                await Shell.Current.DisplayAlert("Uyarı", "Lütfen tüm kelimelerin Türkçe karşılıklarını yazın.", "Tamam");
                return;
            }

            IsBusy = true;
            int correctCount = 0;

            try
            {
                foreach (var item in QuizItems)
                {
                    bool isCorrect = !string.IsNullOrEmpty(item.SelectedMeaning) && 
                                     item.SelectedMeaning.Trim().Equals(item.CorrectMeaning.Trim(), StringComparison.OrdinalIgnoreCase);
                    item.IsCorrectMatch = isCorrect;

                    if (isCorrect) correctCount++;

                    // Submit to backend if it is an existing user word
                    if (item.WordId > 0)
                    {
                        await _apiService.SubmitQuizResultAsync(new QuizResultDto
                        {
                            WordId = item.WordId,
                            IsCorrect = isCorrect
                        });
                    }
                }

                ScoreText = $"Sonuç: {QuizItems.Count} kelimeden {correctCount} tanesini doğru yazdınız! 🎉";
                HasChecked = true;
                CanLoadQuiz = true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Hata", $"Sonuçlar kontrol edilirken hata oluştu: {ex.Message}", "Tamam");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
