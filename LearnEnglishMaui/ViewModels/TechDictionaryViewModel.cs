// IngilizceProje/MAUIClient/ViewModels/TechDictionaryViewModel.cs
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
using IngilizceProje.Domain.Enums;
using IngilizceProjeMAUI.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace IngilizceProjeMAUI.ViewModels
{
    public class TechWordModel
    {
        public int Id { get; set; }
        public string English { get; set; } = string.Empty;
        public string Turkish { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string MainCategory { get; set; } = "Software";
        public string Difficulty { get; set; } = "Kolay";
        public int DifficultyLevel { get; set; } = 1;
        public string WordType { get; set; } = "Noun";
        public string Sentence { get; set; } = string.Empty;
        public string SentenceTurkish { get; set; } = string.Empty;
    }

    public partial class TechWordItem : ObservableObject
    {
        public int Id { get; set; }
        public string English { get; set; } = string.Empty;
        public string Turkish { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string MainCategory { get; set; } = "Software";
        public string Difficulty { get; set; } = "Kolay";
        public int DifficultyLevel { get; set; } = 1;
        public string WordType { get; set; } = "Noun";
        public string Sentence { get; set; } = string.Empty;
        public string SentenceTurkish { get; set; } = string.Empty;
        public int UserWordId { get; set; }

        [ObservableProperty]
        private bool _isAdded;

        [ObservableProperty]
        private bool _isAdding;

        public string DifficultyBadgeText => Difficulty switch
        {
            "Kolay" => "🟢 Kolay",
            "Orta" => "🟡 Orta",
            _ => "🔴 Zor"
        };

        public string DifficultyColor => DifficultyLevel switch
        {
            1 => "#10B981",
            2 => "#F59E0B",
            _ => "#EF4444"
        };

        public string DifficultyBadgeBg => DifficultyLevel switch
        {
            1 => "#ECFDF5",
            2 => "#FFFBEB",
            _ => "#FEF2F2"
        };
    }

    public class CategoryInfoModel
    {
        public string Key { get; set; } = string.Empty; // "AI", "Software", "Hardware"
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "💡";
        public string GradientStart { get; set; } = "#4F46E5";
        public string GradientEnd { get; set; } = "#7C3AED";
        public int WordCount { get; set; }
        public int EasyCount { get; set; }
        public int MediumCount { get; set; }
        public int HardCount { get; set; }
        public string Tagline { get; set; } = string.Empty;
    }

    public partial class CategoryTestQuestion : ObservableObject
    {
        public int QuestionIndex { get; set; }
        public string Term { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectOptionIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public string ExampleSentence { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Kolay";

        [ObservableProperty]
        private int _selectedOptionIndex = -1;

        [ObservableProperty]
        private bool _isAnswered;

        [ObservableProperty]
        private bool _isCorrect;

        // Dynamic helper colors for Option A, B, C, D
        public Color GetOptionBg(int index)
        {
            if (!IsAnswered) return Color.FromArgb("#F8FAFC");
            if (index == CorrectOptionIndex) return Color.FromArgb("#DCFCE7"); // Green
            if (index == SelectedOptionIndex && !IsCorrect) return Color.FromArgb("#FEE2E2"); // Red
            return Color.FromArgb("#F8FAFC");
        }

        public Color GetOptionBorder(int index)
        {
            if (!IsAnswered) return Color.FromArgb("#CBD5E1");
            if (index == CorrectOptionIndex) return Color.FromArgb("#16A34A"); // Green border
            if (index == SelectedOptionIndex && !IsCorrect) return Color.FromArgb("#DC2626"); // Red border
            return Color.FromArgb("#CBD5E1");
        }
    }

    public partial class CategoryQuizItem : ObservableObject
    {
        public int Id { get; set; }
        public string English { get; set; } = string.Empty;
        public string CorrectMeaning { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;

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

        [ObservableProperty]
        private bool? _isCorrectMatch;

        [ObservableProperty]
        private bool _isActiveSelection;

        public bool IsSelected => !string.IsNullOrEmpty(SelectedMeaning);

        // Dynamic Pair Coloring for visual pairing
        [ObservableProperty]
        private int _pairIndex = -1;

        [ObservableProperty]
        private Brush _matchBorderBrush = new SolidColorBrush(Color.FromArgb("#CBD5E1"));

        [ObservableProperty]
        private Color _matchBorderColor = Color.FromArgb("#CBD5E1");

        [ObservableProperty]
        private Color _matchBgColor = Colors.White;

        [ObservableProperty]
        private Color _matchBadgeBgColor = Color.FromArgb("#EEF2FF");

        [ObservableProperty]
        private Color _matchTextColor = Color.FromArgb("#0F172A");

        [ObservableProperty]
        private string _pairBadgeText = string.Empty;

        [ObservableProperty]
        private double _borderThickness = 1.5;

        public void ResetColors()
        {
            PairIndex = -1;
            MatchBorderColor = Color.FromArgb("#CBD5E1");
            MatchBorderBrush = new SolidColorBrush(Color.FromArgb("#CBD5E1"));
            MatchBgColor = Colors.White;
            MatchBadgeBgColor = Color.FromArgb("#EEF2FF");
            MatchTextColor = Color.FromArgb("#0F172A");
            PairBadgeText = string.Empty;
            BorderThickness = 1.5;
        }

        public void ApplyActiveSelection(Color border, Color bg)
        {
            MatchBorderColor = border;
            MatchBorderBrush = new SolidColorBrush(border);
            MatchBgColor = bg;
            BorderThickness = 3.5;
        }

        public void ApplyPairColor(int index, Color border, Color bg, Color badgeBg, Color text)
        {
            PairIndex = index;
            MatchBorderColor = border;
            MatchBorderBrush = new SolidColorBrush(border);
            MatchBgColor = bg;
            MatchBadgeBgColor = badgeBg;
            MatchTextColor = text;
            PairBadgeText = $"#{index + 1}";
            BorderThickness = 3.5;
        }
    }

    public partial class TechDictionaryViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly PronunciationService _pronunciationService;
        private readonly Random _random = new();
        private List<TechWordItem> _allWords = new();

        // View Navigation State
        [ObservableProperty]
        private bool _isCategorySelected;

        [ObservableProperty]
        private CategoryInfoModel? _selectedCategoryInfo;

        [ObservableProperty]
        private string _activeSubTab = "Words"; // "Words", "Test", "Quiz", "Flashcard"

        // Categories Hub
        [ObservableProperty]
        private ObservableCollection<CategoryInfoModel> _categoryCards = new();

        [ObservableProperty]
        private int _totalWordsCount;

        // --- Words Tab State ---
        [ObservableProperty]
        private ObservableCollection<TechWordItem> _filteredWords = new();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private string _selectedDifficulty = "Tümü"; // "Tümü", "Kolay", "Orta", "Zor"

        [ObservableProperty]
        private int _categoryWordsCount;

        [ObservableProperty]
        private bool _isLoading;

        // --- Test Tab State ---
        [ObservableProperty]
        private CategoryTestQuestion? _currentQuestion;

        [ObservableProperty]
        private int _currentQuestionNumber = 1;

        [ObservableProperty]
        private int _totalQuestions = 10;

        [ObservableProperty]
        private double _testProgress = 0.1;

        [ObservableProperty]
        private int _correctCount = 0;

        [ObservableProperty]
        private int _wrongCount = 0;

        [ObservableProperty]
        private bool _isTestCompleted = false;

        [ObservableProperty]
        private int _scorePercentage = 0;

        [ObservableProperty]
        private string _testResultMessage = string.Empty;

        // Dynamic colors for 4 options in current test
        [ObservableProperty]
        private Color _optionAColor = Color.FromArgb("#F8FAFC");
        [ObservableProperty]
        private Color _optionBColor = Color.FromArgb("#F8FAFC");
        [ObservableProperty]
        private Color _optionCColor = Color.FromArgb("#F8FAFC");
        [ObservableProperty]
        private Color _optionDColor = Color.FromArgb("#F8FAFC");

        [ObservableProperty]
        private Color _optionABorderColor = Color.FromArgb("#CBD5E1");
        [ObservableProperty]
        private Color _optionBBorderColor = Color.FromArgb("#CBD5E1");
        [ObservableProperty]
        private Color _optionCBorderColor = Color.FromArgb("#CBD5E1");
        [ObservableProperty]
        private Color _optionDBorderColor = Color.FromArgb("#CBD5E1");

        // --- Quiz Tab State ---
        [ObservableProperty]
        private ObservableCollection<CategoryQuizItem> _quizItems = new();

        [ObservableProperty]
        private ObservableCollection<QuizTurkishOptionItem> _turkishOptions = new();

        private CategoryQuizItem? _selectedEnglishItem;
        private QuizTurkishOptionItem? _selectedTurkishOption;

        [ObservableProperty]
        private List<string> _availableMeanings = new();

        [ObservableProperty]
        private bool _isQuizChecked;

        [ObservableProperty]
        private string _quizFeedbackMessage = string.Empty;

        [ObservableProperty]
        private bool _isQuizSuccess;

        // --- Flashcard Tab State ---
        [ObservableProperty]
        private TechWordItem? _currentFlashcard;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FlashcardCounterText))]
        private int _flashcardIndex = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FlashcardCounterText))]
        private int _totalFlashcards = 0;

        [ObservableProperty]
        private bool _isCardFlipped = false;

        public string FlashcardCounterText => TotalFlashcards > 0
            ? $"Kart {FlashcardIndex + 1} / {TotalFlashcards}"
            : "Kart 0 / 0";

        public TechDictionaryViewModel(ApiService apiService, PronunciationService pronunciationService)
        {
            _apiService = apiService;
            _pronunciationService = pronunciationService;
            MainThread.BeginInvokeOnMainThread(async () => await LoadDatasetAsync());
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task SpeakWordAsync(string? text) => _pronunciationService.SpeakAsync(text);

        public void StopPronunciation() => _pronunciationService.Stop();

        partial void OnCurrentFlashcardChanged(TechWordItem? value) => StopPronunciation();

        partial void OnActiveSubTabChanged(string value) => StopPronunciation();

        partial void OnIsCategorySelectedChanged(bool value) => StopPronunciation();

        partial void OnSearchQueryChanged(string value)
        {
            ApplyWordsFilter();
        }

        partial void OnSelectedDifficultyChanged(string value)
        {
            ApplyWordsFilter();
        }

        [RelayCommand]
        public async Task LoadDatasetAsync()
        {
            if (_allWords.Count > 0)
            {
                await SyncUserWordsAsync();
                return;
            }

            IsLoading = true;
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("tech_words_dataset.json");
                using var reader = new StreamReader(stream);
                var jsonContent = await reader.ReadToEndAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var rawList = JsonSerializer.Deserialize<List<TechWordModel>>(jsonContent, options);

                if (rawList != null)
                {
                    _allWords = rawList.Select(w => new TechWordItem
                    {
                        Id = w.Id,
                        English = w.English,
                        Turkish = w.Turkish,
                        Explanation = w.Explanation,
                        Category = w.Category,
                        MainCategory = string.IsNullOrWhiteSpace(w.MainCategory) ? "Software" : w.MainCategory,
                        Difficulty = string.IsNullOrWhiteSpace(w.Difficulty) ? "Kolay" : w.Difficulty,
                        DifficultyLevel = w.DifficultyLevel > 0 ? w.DifficultyLevel : 1,
                        WordType = w.WordType,
                        Sentence = w.Sentence,
                        SentenceTurkish = w.SentenceTurkish,
                        IsAdded = false
                    }).ToList();

                    TotalWordsCount = _allWords.Count;
                    BuildCategoryCards();
                    await SyncUserWordsAsync();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Hata", $"Sözlük veri seti yüklenemedi: {ex.Message}", "Tamam");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BuildCategoryCards()
        {
            var aiWords = _allWords.Where(w => w.MainCategory == "AI").ToList();
            var swWords = _allWords.Where(w => w.MainCategory == "Software").ToList();
            var hwWords = _allWords.Where(w => w.MainCategory == "Hardware").ToList();

            var cards = new List<CategoryInfoModel>
            {
                new CategoryInfoModel
                {
                    Key = "AI",
                    Title = "Yapay Zekâ (AI)",
                    Subtitle = "Machine Learning, LLM, Derin Öğrenme & Prompt",
                    Description = "Yapay zekâ ve veri biliminde en çok kullanılan kavramlar, mimariler ve pratik kullanım senaryoları.",
                    Icon = "🤖",
                    GradientStart = "#6366F1",
                    GradientEnd = "#8B5CF6",
                    WordCount = aiWords.Count,
                    EasyCount = aiWords.Count(w => w.DifficultyLevel == 1),
                    MediumCount = aiWords.Count(w => w.DifficultyLevel == 2),
                    HardCount = aiWords.Count(w => w.DifficultyLevel == 3),
                    Tagline = "460+ Uzman Seviye Yapay Zekâ Terimi"
                },
                new CategoryInfoModel
                {
                    Key = "Software",
                    Title = "Yazılım (Software)",
                    Subtitle = "Algoritmalar, Web, API, Mimari & Veritabanı",
                    Description = "Modern yazılım geliştirme, mimari prensipler, nesne yönelimli programlama ve sistem tasarımı.",
                    Icon = "💻",
                    GradientStart = "#0EA5E9",
                    GradientEnd = "#2563EB",
                    WordCount = swWords.Count,
                    EasyCount = swWords.Count(w => w.DifficultyLevel == 1),
                    MediumCount = swWords.Count(w => w.DifficultyLevel == 2),
                    HardCount = swWords.Count(w => w.DifficultyLevel == 3),
                    Tagline = "1.000+ Kapsamlı Yazılım Mühendisliği Terimi"
                },
                new CategoryInfoModel
                {
                    Key = "Hardware",
                    Title = "Donanım (Hardware)",
                    Subtitle = "İşlemciler, Çipler, Bellek & Sistem Mimarisi",
                    Description = "CPU, GPU, ön bellek, anakart, çipler, veri yolları ve bilgisayarın fiziksel çalışma prensipleri.",
                    Icon = "🖥️",
                    GradientStart = "#10B981",
                    GradientEnd = "#059669",
                    WordCount = hwWords.Count,
                    EasyCount = hwWords.Count(w => w.DifficultyLevel == 1),
                    MediumCount = hwWords.Count(w => w.DifficultyLevel == 2),
                    HardCount = hwWords.Count(w => w.DifficultyLevel == 3),
                    Tagline = "Donanım ve Sistem Mühendisliği Terimleri"
                }
            };

            CategoryCards = new ObservableCollection<CategoryInfoModel>(cards);
        }

        // --- Category Selection ---
        [RelayCommand]
        public void SelectCategory(string categoryKey)
        {
            if (string.IsNullOrWhiteSpace(categoryKey)) return;

            var info = CategoryCards.FirstOrDefault(c => c.Key.Equals(categoryKey, StringComparison.OrdinalIgnoreCase));
            if (info == null) return;

            SelectedCategoryInfo = info;
            IsCategorySelected = true;
            ActiveSubTab = "Words";
            SelectedDifficulty = "Tümü";
            SearchQuery = string.Empty;

            ApplyWordsFilter();
        }

        [RelayCommand]
        public void BackToCategories()
        {
            IsCategorySelected = false;
            SelectedCategoryInfo = null;
        }

        [RelayCommand]
        public void SwitchSubTab(string tabName)
        {
            if (string.IsNullOrWhiteSpace(tabName)) return;

            ActiveSubTab = tabName;

            if (tabName == "Test")
            {
                StartNewTest();
            }
            else if (tabName == "Quiz")
            {
                StartNewQuiz();
            }
            else if (tabName == "Flashcard")
            {
                StartFlashcards();
            }
        }

        [RelayCommand]
        public void SetDifficultyFilter(string diff)
        {
            SelectedDifficulty = diff;
        }

        // --- Filter Words in Selected Category (Easy to Hard Sorting) ---
        private void ApplyWordsFilter()
        {
            if (SelectedCategoryInfo == null || _allWords == null || _allWords.Count == 0) return;

            var key = SelectedCategoryInfo.Key;
            var query = _allWords.Where(w => w.MainCategory.Equals(key, StringComparison.OrdinalIgnoreCase));

            // Difficulty filter
            if (SelectedDifficulty != "Tümü")
            {
                query = query.Where(w => w.Difficulty.Equals(SelectedDifficulty, StringComparison.OrdinalIgnoreCase));
            }

            // Search query
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var s = NormalizeText(SearchQuery);
                query = query.Where(w => NormalizeText(w.English).Contains(s) ||
                                         NormalizeText(w.Turkish).Contains(s) ||
                                         NormalizeText(w.Explanation).Contains(s) ||
                                         NormalizeText(w.Sentence).Contains(s));
            }

            // Default Sort: KOLAYDAN ZORA (Easy to Hard: Level 1 -> 2 -> 3, then Alphabetical)
            var sortedList = query
                .OrderBy(w => w.DifficultyLevel)
                .ThenBy(w => w.English)
                .ToList();

            CategoryWordsCount = sortedList.Count;
            // Take 300 to maintain smooth 60fps scrolling
            FilteredWords = new ObservableCollection<TechWordItem>(sortedList.Take(300));
        }

        private static string NormalizeText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return input.Trim().ToLowerInvariant()
                .Replace("ı", "i")
                .Replace("İ", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c")
                .Replace("â", "a")
                .Replace(" ", "");
        }

        // --- CATEGORY TEST (DENEME) ENGINE ---
        private List<CategoryTestQuestion> _testQuestionsPool = new();

        [RelayCommand]
        public void StartNewTest()
        {
            if (SelectedCategoryInfo == null) return;

            var catKey = SelectedCategoryInfo.Key;
            var pool = _allWords.Where(w => w.MainCategory.Equals(catKey, StringComparison.OrdinalIgnoreCase)).ToList();

            if (pool.Count < 4) return;

            // Generate 10 diverse questions sorted from easy to hard
            _testQuestionsPool.Clear();
            CurrentQuestionNumber = 1;
            TotalQuestions = Math.Min(10, pool.Count);
            CorrectCount = 0;
            WrongCount = 0;
            IsTestCompleted = false;
            TestProgress = 1.0 / TotalQuestions;

            // Pick candidate words sorted from easy (first 3) to medium (next 4) to hard (last 3)
            var easyWords = pool.Where(w => w.DifficultyLevel == 1).OrderBy(_ => _random.Next()).ToList();
            var medWords = pool.Where(w => w.DifficultyLevel == 2).OrderBy(_ => _random.Next()).ToList();
            var hardWords = pool.Where(w => w.DifficultyLevel == 3).OrderBy(_ => _random.Next()).ToList();

            var selectedTargetWords = new List<TechWordItem>();
            selectedTargetWords.AddRange(easyWords.Take(3));
            selectedTargetWords.AddRange(medWords.Take(4));
            selectedTargetWords.AddRange(hardWords.Take(3));

            // Fill if remaining
            if (selectedTargetWords.Count < TotalQuestions)
            {
                var remaining = pool.Except(selectedTargetWords).OrderBy(_ => _random.Next()).Take(TotalQuestions - selectedTargetWords.Count);
                selectedTargetWords.AddRange(remaining);
            }

            // Ensure sorted from easy to hard in the test!
            selectedTargetWords = selectedTargetWords.OrderBy(w => w.DifficultyLevel).ToList();

            int qIdx = 1;
            foreach (var target in selectedTargetWords)
            {
                // Question format variation
                bool askForTerm = qIdx % 2 == 0; // Alternating questions: Meaning vs Term

                var wrongCandidates = pool.Where(w => w.Id != target.Id).OrderBy(_ => _random.Next()).Take(3).ToList();
                var options = new List<string>();

                if (askForTerm)
                {
                    options.Add(target.English);
                    options.AddRange(wrongCandidates.Select(w => w.English));
                }
                else
                {
                    options.Add($"{target.Turkish} - {target.Explanation}");
                    options.AddRange(wrongCandidates.Select(w => $"{w.Turkish} - {w.Explanation}"));
                }

                // Shuffle options
                var shuffledOptions = options.OrderBy(_ => _random.Next()).ToList();
                int correctIdx = askForTerm ? 
                    shuffledOptions.IndexOf(target.English) : 
                    shuffledOptions.IndexOf($"{target.Turkish} - {target.Explanation}");

                string qText = askForTerm
                    ? $"Tanım: \"{target.Turkish} ({target.Explanation})\"\nBu tanım aşağıdaki {SelectedCategoryInfo.Title} terimlerinden hangisine aittir?"
                    : $"'{target.English}' teriminin doğru Türkçe karşılığı ve bilişimdeki anlamı nedir?";

                _testQuestionsPool.Add(new CategoryTestQuestion
                {
                    QuestionIndex = qIdx,
                    Term = target.English,
                    QuestionText = qText,
                    Options = shuffledOptions,
                    CorrectOptionIndex = correctIdx,
                    Explanation = $"💡 {target.English}: {target.Turkish}. {target.Explanation}",
                    ExampleSentence = target.Sentence,
                    Difficulty = target.Difficulty
                });
                qIdx++;
            }

            LoadCurrentTestQuestion();
        }

        private void LoadCurrentTestQuestion()
        {
            if (CurrentQuestionNumber <= _testQuestionsPool.Count)
            {
                CurrentQuestion = _testQuestionsPool[CurrentQuestionNumber - 1];
                CurrentQuestion.SelectedOptionIndex = -1;
                CurrentQuestion.IsAnswered = false;
                TestProgress = (double)CurrentQuestionNumber / TotalQuestions;
                ResetOptionColors();
            }
            else
            {
                FinishTest();
            }
        }

        [RelayCommand]
        public void AnswerOption(int optionIndex)
        {
            if (CurrentQuestion == null || CurrentQuestion.IsAnswered) return;

            CurrentQuestion.SelectedOptionIndex = optionIndex;
            CurrentQuestion.IsAnswered = true;
            CurrentQuestion.IsCorrect = (optionIndex == CurrentQuestion.CorrectOptionIndex);

            if (CurrentQuestion.IsCorrect)
            {
                CorrectCount++;
            }
            else
            {
                WrongCount++;
            }

            UpdateOptionColors(optionIndex, CurrentQuestion.CorrectOptionIndex);
        }

        private void ResetOptionColors()
        {
            OptionAColor = Color.FromArgb("#F8FAFC");
            OptionBColor = Color.FromArgb("#F8FAFC");
            OptionCColor = Color.FromArgb("#F8FAFC");
            OptionDColor = Color.FromArgb("#F8FAFC");

            OptionABorderColor = Color.FromArgb("#CBD5E1");
            OptionBBorderColor = Color.FromArgb("#CBD5E1");
            OptionCBorderColor = Color.FromArgb("#CBD5E1");
            OptionDBorderColor = Color.FromArgb("#CBD5E1");
        }

        private void UpdateOptionColors(int selectedIdx, int correctIdx)
        {
            ResetOptionColors();

            // Set Correct Option to Green
            SetOptionColor(correctIdx, Color.FromArgb("#DCFCE7"), Color.FromArgb("#16A34A"));

            // If selected wrong, set to Red
            if (selectedIdx != correctIdx)
            {
                SetOptionColor(selectedIdx, Color.FromArgb("#FEE2E2"), Color.FromArgb("#DC2626"));
            }
        }

        private void SetOptionColor(int index, Color bg, Color border)
        {
            switch (index)
            {
                case 0:
                    OptionAColor = bg;
                    OptionABorderColor = border;
                    break;
                case 1:
                    OptionBColor = bg;
                    OptionBBorderColor = border;
                    break;
                case 2:
                    OptionCColor = bg;
                    OptionCBorderColor = border;
                    break;
                case 3:
                    OptionDColor = bg;
                    OptionDBorderColor = border;
                    break;
            }
        }

        [RelayCommand]
        public void NextQuestion()
        {
            if (CurrentQuestionNumber < TotalQuestions)
            {
                CurrentQuestionNumber++;
                LoadCurrentTestQuestion();
            }
            else
            {
                FinishTest();
            }
        }

        private void FinishTest()
        {
            IsTestCompleted = true;
            ScorePercentage = TotalQuestions > 0 ? (int)Math.Round((double)CorrectCount / TotalQuestions * 100) : 0;

            if (ScorePercentage >= 80)
            {
                TestResultMessage = $"🌟 Harika Başarı! {SelectedCategoryInfo?.Title} alanında yüksek bir hakimiyet sergiledin!";
            }
            else if (ScorePercentage >= 50)
            {
                TestResultMessage = $"👍 Güzel Performans! {SelectedCategoryInfo?.Title} terimlerini pekiştirmek için kelime kartlarını tekrar inceleyebilirsin.";
            }
            else
            {
                TestResultMessage = $"📚 Pratik Yapmaya Devam Et! {SelectedCategoryInfo?.Title} kategorisindeki kelimeleri kolaydan zora inceleyip tekrar dene.";
            }
        }

        // --- CATEGORY QUIZ MATCHING ENGINE (DUOLINGO STYLE TAP-TO-MATCH) ---
        private static readonly (Color Border, Color Bg, Color BadgeBg, Color Text)[] MatchingPairColorPalette = new[]
        {
            // Çift 1: Electric Indigo / Sapphire Blue
            (Color.FromArgb("#2563EB"), Color.FromArgb("#EFF6FF"), Color.FromArgb("#DBEAFE"), Color.FromArgb("#1D4ED8")),
            // Çift 2: Emerald Green
            (Color.FromArgb("#059669"), Color.FromArgb("#ECFDF5"), Color.FromArgb("#D1FAE5"), Color.FromArgb("#065F46")),
            // Çift 3: Vibrant Amber / Orange
            (Color.FromArgb("#D97706"), Color.FromArgb("#FFFBEB"), Color.FromArgb("#FDE68A"), Color.FromArgb("#78350F")),
            // Çift 4: Royal Violet / Purple
            (Color.FromArgb("#7C3AED"), Color.FromArgb("#F5F3FF"), Color.FromArgb("#DDD6FE"), Color.FromArgb("#4C1D95")),
            // Çift 5: Vivid Rose / Pink
            (Color.FromArgb("#DB2777"), Color.FromArgb("#FDF2F8"), Color.FromArgb("#FBCFE8"), Color.FromArgb("#831843"))
        };

        [RelayCommand]
        public void SelectEnglishWord(CategoryQuizItem? item)
        {
            if (IsQuizChecked || item == null) return;

            if (_selectedTurkishOption != null)
            {
                PairEnglishAndTurkish(item, _selectedTurkishOption);
                return;
            }

            foreach (var q in QuizItems)
            {
                q.IsActiveSelection = false;
                if (!q.IsSelected)
                {
                    q.ResetColors();
                }
            }

            if (_selectedEnglishItem == item)
            {
                _selectedEnglishItem = null;
                if (!item.IsSelected) item.ResetColors();
            }
            else
            {
                _selectedEnglishItem = item;
                item.IsActiveSelection = true;
                if (!item.IsSelected)
                {
                    int itemIdx = QuizItems.IndexOf(item);
                    if (itemIdx < 0 || itemIdx >= MatchingPairColorPalette.Length) itemIdx = 0;
                    var color = MatchingPairColorPalette[itemIdx];
                    item.ApplyActiveSelection(color.Border, color.Bg);
                }
            }
        }

        [RelayCommand]
        public void SelectTurkishOption(QuizTurkishOptionItem? option)
        {
            if (IsQuizChecked || option == null) return;

            if (_selectedEnglishItem != null)
            {
                PairEnglishAndTurkish(_selectedEnglishItem, option);
                return;
            }

            foreach (var t in TurkishOptions)
            {
                t.IsSelected = false;
                if (!t.IsMatched)
                {
                    t.ResetColors();
                }
            }

            if (_selectedTurkishOption == option)
            {
                _selectedTurkishOption = null;
                if (!option.IsMatched) option.ResetColors();
            }
            else
            {
                _selectedTurkishOption = option;
                option.IsSelected = true;
                if (!option.IsMatched)
                {
                    int optIdx = TurkishOptions.IndexOf(option);
                    if (optIdx < 0 || optIdx >= MatchingPairColorPalette.Length) optIdx = 0;
                    var color = MatchingPairColorPalette[optIdx];
                    option.ApplyActiveSelection(color.Border, color.Bg);
                }
            }
        }

        private void PairEnglishAndTurkish(CategoryQuizItem englishItem, QuizTurkishOptionItem turkishOption)
        {
            // If this Turkish option was previously paired with another English word, unpair that word
            var existingEnglish = QuizItems.FirstOrDefault(q => q.SelectedMeaning == turkishOption.Text);
            if (existingEnglish != null && existingEnglish != englishItem)
            {
                existingEnglish.SelectedMeaning = string.Empty;
                existingEnglish.ResetColors();
            }

            // If this English word was previously paired with another Turkish option, unpair that option
            var existingTurkish = TurkishOptions.FirstOrDefault(t => t.MatchedEnglishWord == englishItem.English);
            if (existingTurkish != null && existingTurkish != turkishOption)
            {
                existingTurkish.IsMatched = false;
                existingTurkish.MatchedEnglishWord = null;
                existingTurkish.ResetColors();
            }

            // Designated 5-color slot matching: English item's position (0..4) defines the pair color
            int assignedPairIdx = QuizItems.IndexOf(englishItem);
            if (assignedPairIdx < 0 || assignedPairIdx >= MatchingPairColorPalette.Length)
            {
                assignedPairIdx = 0;
            }

            var pairColor = MatchingPairColorPalette[assignedPairIdx];

            // Pair them with IDENTICAL 5-distinct colors, badges and glowing 3px border
            englishItem.SelectedMeaning = turkishOption.Text;
            englishItem.IsActiveSelection = false;
            englishItem.ApplyPairColor(assignedPairIdx, pairColor.Border, pairColor.Bg, pairColor.BadgeBg, pairColor.Text);

            turkishOption.IsMatched = true;
            turkishOption.MatchedEnglishWord = englishItem.English;
            turkishOption.IsSelected = false;
            turkishOption.ApplyPairColor(assignedPairIdx, pairColor.Border, pairColor.Bg, pairColor.BadgeBg, pairColor.Text);

            // Reset selection highlights
            _selectedEnglishItem = null;
            _selectedTurkishOption = null;
        }

        [RelayCommand]
        public void StartNewQuiz()
        {
            if (SelectedCategoryInfo == null) return;

            var catKey = SelectedCategoryInfo.Key;
            var pool = _allWords.Where(w => w.MainCategory.Equals(catKey, StringComparison.OrdinalIgnoreCase)).ToList();

            if (pool.Count < 5) return;

            IsQuizChecked = false;
            QuizFeedbackMessage = string.Empty;
            _selectedEnglishItem = null;
            _selectedTurkishOption = null;

            var chosen = pool.OrderBy(_ => _random.Next()).Take(5).ToList();

            var items = chosen.Select(w => new CategoryQuizItem
            {
                Id = w.Id,
                English = w.English,
                CorrectMeaning = w.Turkish,
                Explanation = w.Explanation,
                SelectedMeaning = string.Empty,
                IsActiveSelection = false,
                IsCorrectMatch = null
            }).ToList();

            foreach (var item in items) item.ResetColors();

            QuizItems = new ObservableCollection<CategoryQuizItem>(items);
            AvailableMeanings = chosen.Select(w => w.Turkish).OrderBy(_ => _random.Next()).ToList();

            var turkishList = chosen.Select(w => new QuizTurkishOptionItem
            {
                Text = w.Turkish,
                IsSelected = false,
                IsMatched = false,
                MatchedEnglishWord = null,
                IsCorrectMatch = null
            }).OrderBy(_ => _random.Next()).ToList();

            foreach (var t in turkishList) t.ResetColors();

            TurkishOptions = new ObservableCollection<QuizTurkishOptionItem>(turkishList);
        }

        [RelayCommand]
        public void CheckQuizMatching()
        {
            if (QuizItems == null || QuizItems.Count == 0) return;

            var successBorder = Color.FromArgb("#10B981");
            var successBg = Color.FromArgb("#ECFDF5");
            var successBadge = Color.FromArgb("#A7F3D0");
            var successText = Color.FromArgb("#065F46");

            var errorBorder = Color.FromArgb("#EF4444");
            var errorBg = Color.FromArgb("#FEF2F2");
            var errorBadge = Color.FromArgb("#FECACA");
            var errorText = Color.FromArgb("#991B1B");

            int correctMatches = 0;
            foreach (var item in QuizItems)
            {
                var matchingTurkish = TurkishOptions.FirstOrDefault(t => t.Text == item.SelectedMeaning);

                if (!string.IsNullOrEmpty(item.SelectedMeaning) &&
                    string.Equals(item.SelectedMeaning.Trim(), item.CorrectMeaning?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    item.IsCorrectMatch = true;
                    item.ApplyPairColor(item.PairIndex >= 0 ? item.PairIndex : 0, successBorder, successBg, successBadge, successText);

                    if (matchingTurkish != null)
                    {
                        matchingTurkish.IsCorrectMatch = true;
                        matchingTurkish.ApplyPairColor(matchingTurkish.PairIndex >= 0 ? matchingTurkish.PairIndex : 0, successBorder, successBg, successBadge, successText);
                    }
                    correctMatches++;
                }
                else
                {
                    item.IsCorrectMatch = false;
                    item.ApplyPairColor(item.PairIndex >= 0 ? item.PairIndex : 0, errorBorder, errorBg, errorBadge, errorText);

                    if (matchingTurkish != null)
                    {
                        matchingTurkish.IsCorrectMatch = false;
                        matchingTurkish.ApplyPairColor(matchingTurkish.PairIndex >= 0 ? matchingTurkish.PairIndex : 0, errorBorder, errorBg, errorBadge, errorText);
                    }
                }
            }

            foreach (var t in TurkishOptions.Where(t => !t.IsMatched))
            {
                t.IsCorrectMatch = false;
                t.ApplyActiveSelection(errorBorder, errorBg);
            }

            IsQuizChecked = true;
            IsQuizSuccess = (correctMatches == QuizItems.Count);

            if (IsQuizSuccess)
            {
                QuizFeedbackMessage = $"🎉 Tebrikler! Tüm {QuizItems.Count} eşleştirmeyi doğru yaptın!";
            }
            else
            {
                QuizFeedbackMessage = $"Skor: {correctMatches} / {QuizItems.Count} doğru. Kırmızı işaretli eşleştirmeleri kontrol et.";
            }
        }

        // --- CATEGORY FLASHCARD ENGINE ---
        private List<TechWordItem> _flashcardList = new();

        [RelayCommand]
        public void StartFlashcards()
        {
            if (SelectedCategoryInfo == null) return;

            var catKey = SelectedCategoryInfo.Key;
            _flashcardList = _allWords
                .Where(w => w.MainCategory.Equals(catKey, StringComparison.OrdinalIgnoreCase))
                .OrderBy(w => w.DifficultyLevel) // Easy to hard!
                .ToList();

            TotalFlashcards = _flashcardList.Count;
            FlashcardIndex = 0;
            IsCardFlipped = false;

            if (_flashcardList.Count > 0)
            {
                CurrentFlashcard = _flashcardList[0];
            }
        }

        [RelayCommand]
        public void FlipFlashcard()
        {
            IsCardFlipped = !IsCardFlipped;
        }

        [RelayCommand]
        public void NextFlashcard()
        {
            if (_flashcardList.Count == 0) return;
            if (FlashcardIndex < _flashcardList.Count - 1)
            {
                FlashcardIndex++;
                CurrentFlashcard = _flashcardList[FlashcardIndex];
                IsCardFlipped = false;
            }
        }

        [RelayCommand]
        public void PreviousFlashcard()
        {
            if (_flashcardList.Count == 0) return;
            if (FlashcardIndex > 0)
            {
                FlashcardIndex--;
                CurrentFlashcard = _flashcardList[FlashcardIndex];
                IsCardFlipped = false;
            }
        }

        // --- USER WORDS SYNC & TOGGLE ---
        public async Task SyncUserWordsAsync()
        {
            try
            {
                var userWords = await _apiService.GetAllWordsAsync();
                if (userWords != null)
                {
                    var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (var w in userWords)
                    {
                        var key = w.EnglishWord.Trim();
                        if (!map.ContainsKey(key))
                        {
                            map[key] = w.Id;
                        }
                    }

                    foreach (var item in _allWords)
                    {
                        if (map.TryGetValue(item.English.Trim(), out int uid))
                        {
                            item.IsAdded = true;
                            item.UserWordId = uid;
                        }
                        else
                        {
                            item.IsAdded = false;
                            item.UserWordId = 0;
                        }
                    }
                }
            }
            catch
            {
                // Silently ignore sync failure if offline
            }
        }

        [RelayCommand]
        public async Task ToggleWordAsync(TechWordItem item)
        {
            if (item == null || item.IsAdding) return;

            item.IsAdding = true;
            try
            {
                if (item.IsAdded)
                {
                    bool confirmed = await Shell.Current.DisplayAlert(
                        "Kelimeyi Çıkar",
                        $"'{item.English}' kelimesini listenizden ve test havuzunuzdan çıkarmak istiyor musunuz?",
                        "Evet, Çıkar",
                        "İptal");

                    if (!confirmed) return;

                    bool deleted = false;
                    if (item.UserWordId > 0)
                    {
                        deleted = await _apiService.DeleteWordAsync(item.UserWordId);
                    }
                    if (!deleted)
                    {
                        deleted = await _apiService.DeleteWordByEnglishAsync(item.English);
                    }

                    item.IsAdded = false;
                    item.UserWordId = 0;
                    await Shell.Current.DisplayAlert("Çıkarıldı", $"'{item.English}' kelimelerim listenizden çıkarıldı.", "Tamam");
                }
                else
                {
                    var wordTypeEnum = WordTypeEnum.Noun;
                    if (item.WordType.Contains("Verb", StringComparison.OrdinalIgnoreCase))
                        wordTypeEnum = WordTypeEnum.Verb;
                    else if (item.WordType.Contains("Adj", StringComparison.OrdinalIgnoreCase))
                        wordTypeEnum = WordTypeEnum.Adjective;

                    var dto = new CreateWordDto
                    {
                        EnglishWord = item.English.Trim(),
                        TurkishMeaning = item.Turkish.Trim(),
                        WordType = wordTypeEnum,
                        ExampleSentence = item.Sentence?.Trim() ?? string.Empty,
                        ExampleSentenceTranslation = item.SentenceTurkish?.Trim() ?? string.Empty
                    };

                    var result = await _apiService.CreateWordWithResultAsync(dto);
                    if (result.Success)
                    {
                        item.IsAdded = true;
                        item.UserWordId = result.Word?.Id ?? 0;
                        await Shell.Current.DisplayAlert("Eklendi 🎉", $"'{item.English}' kişisel kelimeler listenize eklendi! Hemen 'Tekrar' ve 'Test' sekmelerinde de pratik yapabilirsiniz.", "Tamam");
                    }
                    else
                    {
                        item.IsAdded = true;
                        await Shell.Current.DisplayAlert("Bilgi", $"'{item.English}' zaten kelime listenizde mevcut.", "Tamam");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Hata", $"İşlem gerçekleştirilemedi: {ex.Message}", "Tamam");
            }
            finally
            {
                item.IsAdding = false;
            }
        }
    }
}
