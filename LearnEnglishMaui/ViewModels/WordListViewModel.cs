// IngilizceProje/MAUIClient/ViewModels/WordListViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngilizceProje.Application.DTOs;
using IngilizceProje.Domain.Enums;
using IngilizceProjeMAUI.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace IngilizceProjeMAUI.ViewModels
{
    public partial class UserWordItem : ObservableObject
    {
        public int Id { get; set; }

        [ObservableProperty]
        private string _englishWord = string.Empty;

        [ObservableProperty]
        private string _turkishMeaning = string.Empty;

        [ObservableProperty]
        private WordTypeEnum _wordType = WordTypeEnum.Noun;

        [ObservableProperty]
        private string _aiExplanation = string.Empty;

        [ObservableProperty]
        private string _difficulty = "Kolay";

        [ObservableProperty]
        private int _difficultyLevel = 1;

        [ObservableProperty]
        private string _exampleSentence = string.Empty;

        [ObservableProperty]
        private string _exampleSentenceTranslation = string.Empty;

        [ObservableProperty]
        private LearningStatusEnum _learningStatus = LearningStatusEnum.NotReviewed;

        [ObservableProperty]
        private bool _isFavorite;

        [ObservableProperty]
        private bool _isAiLoading;

        public bool HasAiExplanation => !string.IsNullOrWhiteSpace(AiExplanation);

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

    public partial class UserTestQuestion : ObservableObject
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
    }

    public partial class UserQuizMatchingItem : ObservableObject
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

    public partial class WordListViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly GeminiAiService _geminiAiService;
        private readonly PronunciationService _pronunciationService;
        private readonly Random _random = new();

        private List<UserWordItem> _allUserWords = new();

        // Active Navigation Sub-Tab: "Words", "Test", "Quiz", "Flashcard"
        [ObservableProperty]
        private string _activeSubTab = "Words";

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private int _totalWordsCount;

        // --- Words Tab State ---
        [ObservableProperty]
        private ObservableCollection<UserWordItem> _filteredWords = new();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private string _selectedDifficultyFilter = "Tümü"; // "Tümü", "Kolay", "Orta", "Zor"

        // New Word Quick Add
        [ObservableProperty]
        private bool _isAddingWordDialogVisible;

        [ObservableProperty]
        private string _newWordEnglish = string.Empty;

        [ObservableProperty]
        private string _newWordTurkish = string.Empty;

        [ObservableProperty]
        private bool _isAiGeneratingForNewWord;

        // Bulk AI update
        [ObservableProperty]
        private bool _isBulkAiUpdating;

        [ObservableProperty]
        private string _bulkAiUpdateStatus = string.Empty;

        // --- Test Tab State ---
        [ObservableProperty]
        private UserTestQuestion? _currentQuestion;

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
        private ObservableCollection<UserQuizMatchingItem> _quizItems = new();

        [ObservableProperty]
        private ObservableCollection<QuizTurkishOptionItem> _turkishOptions = new();

        private UserQuizMatchingItem? _selectedEnglishItem;
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
        private UserWordItem? _currentFlashcard;

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

        public WordListViewModel(ApiService apiService, GeminiAiService geminiAiService, PronunciationService pronunciationService)
        {
            _apiService = apiService;
            _geminiAiService = geminiAiService;
            _pronunciationService = pronunciationService;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task SpeakWordAsync(string? text) => _pronunciationService.SpeakAsync(text);

        public void StopPronunciation() => _pronunciationService.Stop();

        partial void OnCurrentFlashcardChanged(UserWordItem? value) => StopPronunciation();

        partial void OnSearchQueryChanged(string value)
        {
            ApplyFilter();
        }

        partial void OnSelectedDifficultyFilterChanged(string value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        public void SwitchSubTab(string tabName)
        {
            if (string.IsNullOrWhiteSpace(tabName)) return;

            StopPronunciation();
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
            SelectedDifficultyFilter = diff;
        }

        [RelayCommand]
        public async Task LoadWordsAsync()
        {
            IsBusy = true;
            try
            {
                var wordList = await _apiService.GetWordsAsync();
                var items = new List<UserWordItem>();

                foreach (var w in wordList)
                {
                    // Compute difficulty heuristic if not stored
                    int diffLevel = w.EnglishWord.Length switch
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

                    string aiExpl = string.Empty;
                    // Check if explanation was embedded in ExampleSentenceTranslation
                    if (!string.IsNullOrWhiteSpace(w.ExampleSentenceTranslation) && w.ExampleSentenceTranslation.Contains("💡 AI:"))
                    {
                        var parts = w.ExampleSentenceTranslation.Split("💡 AI:", 2);
                        w.ExampleSentenceTranslation = parts[0].Trim();
                        aiExpl = parts[1].Trim();

                        // Discard old generic fallback templates
                        if (aiExpl.Contains("temel kavramlardan biridir", StringComparison.OrdinalIgnoreCase) ||
                            aiExpl.Contains("temel ifadelerdendir", StringComparison.OrdinalIgnoreCase))
                        {
                            aiExpl = string.Empty;
                        }
                    }
                    else
                    {
                        aiExpl = string.Empty;
                    }

                    items.Add(new UserWordItem
                    {
                        Id = w.Id,
                        EnglishWord = w.EnglishWord,
                        TurkishMeaning = w.TurkishMeaning,
                        WordType = w.WordType,
                        ExampleSentence = w.ExampleSentence,
                        ExampleSentenceTranslation = w.ExampleSentenceTranslation,
                        AiExplanation = aiExpl,
                        Difficulty = diffStr,
                        DifficultyLevel = diffLevel,
                        LearningStatus = w.LearningStatus,
                        IsFavorite = w.IsFavorite
                    });
                }

                _allUserWords = items;
                TotalWordsCount = _allUserWords.Count;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Hata", $"Kelimeler yüklenemedi: {ex.Message}", "Tamam");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilter()
        {
            if (_allUserWords == null) return;

            var query = _allUserWords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var s = SearchQuery.ToLower().Trim();
                query = query.Where(w => w.EnglishWord.ToLower().Contains(s) ||
                                         w.TurkishMeaning.ToLower().Contains(s) ||
                                         w.AiExplanation.ToLower().Contains(s));
            }

            if (SelectedDifficultyFilter != "Tümü")
            {
                query = query.Where(w => w.Difficulty.Equals(SelectedDifficultyFilter, StringComparison.OrdinalIgnoreCase));
            }

            // Default Sort: KOLAYDAN ZORA (Easy to Hard: Level 1 -> 2 -> 3, then alphabetical)
            var sorted = query
                .OrderBy(w => w.DifficultyLevel)
                .ThenBy(w => w.EnglishWord)
                .ToList();

            FilteredWords = new ObservableCollection<UserWordItem>(sorted);
        }

        // --- AI EXPLANATION RE-GENERATE FOR EXISTING WORD ---
        [RelayCommand]
        public async Task GenerateAiExplanationForWordAsync(UserWordItem item)
        {
            if (item == null || item.IsAiLoading) return;

            item.IsAiLoading = true;
            try
            {
                var analysis = await _geminiAiService.AnalyzeAndExplainWordAsync(item.EnglishWord);
                if (analysis != null && !string.IsNullOrWhiteSpace(analysis.Explanation))
                {
                    item.AiExplanation = analysis.Explanation;
                    item.Difficulty = analysis.Difficulty;
                    item.DifficultyLevel = analysis.DifficultyLevel;
                    item.WordType = analysis.GetWordTypeEnum();

                    // If example sentence is empty or was the old generic template, replace it
                    if (string.IsNullOrWhiteSpace(item.ExampleSentence) ||
                        item.ExampleSentence.Contains("Understanding how to use", StringComparison.OrdinalIgnoreCase))
                    {
                        item.ExampleSentence = analysis.ExampleSentence;
                        item.ExampleSentenceTranslation = analysis.ExampleSentenceTranslation;
                    }

                    // If Turkish meaning was missing or old fallback template ("... karşılığı"), update it
                    if (string.IsNullOrWhiteSpace(item.TurkishMeaning) ||
                        item.TurkishMeaning.EndsWith("karşılığı", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(item.TurkishMeaning.Trim(), item.EnglishWord.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(analysis.Turkish))
                        {
                            item.TurkishMeaning = analysis.Turkish;
                        }
                    }

                    // Clean translation text to avoid duplicate "💡 AI:" markers
                    var cleanTranslation = item.ExampleSentenceTranslation ?? string.Empty;
                    if (cleanTranslation.Contains("💡 AI:"))
                    {
                        cleanTranslation = cleanTranslation.Split("💡 AI:", 2)[0].Trim();
                    }

                    // Update on backend
                    var updateDto = new UpdateWordDto
                    {
                        Id = item.Id,
                        EnglishWord = item.EnglishWord,
                        TurkishMeaning = item.TurkishMeaning,
                        WordType = item.WordType,
                        ExampleSentence = item.ExampleSentence,
                        ExampleSentenceTranslation = $"{cleanTranslation} 💡 AI: {item.AiExplanation}",
                        IsFavorite = item.IsFavorite,
                        LearningStatus = item.LearningStatus
                    };
                    await _apiService.UpdateWordAsync(item.Id, updateDto);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("AI Hatası", $"Yapay zeka analiz üretemedi: {ex.Message}", "Tamam");
            }
            finally
            {
                item.IsAiLoading = false;
            }
        }

        // --- BULK REFRESH ALL OUTDATED AI EXPLANATIONS ---
        [RelayCommand]
        public async Task RefreshAllOutdatedExplanationsAsync()
        {
            if (_allUserWords == null || _allUserWords.Count == 0 || IsBulkAiUpdating) return;

            // Find words with old generic templates or missing explanations
            var outdatedWords = _allUserWords.Where(w =>
                string.IsNullOrWhiteSpace(w.AiExplanation) ||
                w.AiExplanation.Contains("temel kavramlardan biridir", StringComparison.OrdinalIgnoreCase) ||
                w.AiExplanation.Contains("temel ifadelerdendir", StringComparison.OrdinalIgnoreCase) ||
                w.AiExplanation.Contains("Çevrimdışı Mod", StringComparison.OrdinalIgnoreCase) ||
                !w.AiExplanation.Contains("Nasıl Kullanılır", StringComparison.OrdinalIgnoreCase)
            ).ToList();

            if (outdatedWords.Count == 0)
            {
                await Shell.Current.DisplayAlert("Harika!", "Listedeki tüm kelimelerinizin yapay zekâ kullanım analizleri ve anlamları zaten güncel!", "Tamam");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "AI Açıklamalarını Yenile",
                $"{outdatedWords.Count} adet kelimenin açıklamaları yapay zekâ ile detaylandırılacak (kelimelerin nasıl ve başka hangi anlamlarda kullanıldıkları tek tek analiz edilecek).\n\nDevam etmek istiyor musunuz?",
                "Evet, Hepsini Yenile",
                "İptal");

            if (!confirm) return;

            IsBulkAiUpdating = true;
            int updatedCount = 0;
            try
            {
                for (int i = 0; i < outdatedWords.Count; i++)
                {
                    var word = outdatedWords[i];
                    BulkAiUpdateStatus = $"AI Analiz Ediyor ({i + 1}/{outdatedWords.Count}): '{word.EnglishWord}'...";
                    await GenerateAiExplanationForWordAsync(word);
                    updatedCount++;
                    await Task.Delay(250); // Small interval to respect API limits
                }

                ApplyFilter();
                await Shell.Current.DisplayAlert("Tamamlandı 🎉", $"{updatedCount} adet kelimenin detaylı yapay zekâ açıklaması başarıyla yenilendi!", "Tamam");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Kısmi Hata", $"Güncelleme sırasında hata: {ex.Message}", "Tamam");
            }
            finally
            {
                IsBulkAiUpdating = false;
                BulkAiUpdateStatus = string.Empty;
            }
        }

        // --- QUICK WORD ADD WITH AI AUTO-EXPLANATION ---
        [RelayCommand]
        public void OpenAddWordDialog()
        {
            NewWordEnglish = string.Empty;
            NewWordTurkish = string.Empty;
            IsAddingWordDialogVisible = true;
        }

        [RelayCommand]
        public void CloseAddWordDialog()
        {
            IsAddingWordDialogVisible = false;
        }

        [RelayCommand]
        public async Task SaveWordWithAiAsync()
        {
            if (string.IsNullOrWhiteSpace(NewWordEnglish))
            {
                await Shell.Current.DisplayAlert("Uyarı", "Lütfen bir İngilizce kelime girin.", "Tamam");
                return;
            }

            IsAiGeneratingForNewWord = true;
            try
            {
                // Call Gemini with English word and user's meaning to validate spelling & enrich meaning
                var aiResult = await _geminiAiService.AnalyzeAndExplainWordAsync(NewWordEnglish.Trim(), NewWordTurkish);

                // Apply AI corrected spelling if available
                string finalEnglish = !string.IsNullOrWhiteSpace(aiResult.CorrectedEnglish) 
                    ? aiResult.CorrectedEnglish.Trim() 
                    : NewWordEnglish.Trim();

                // Apply AI enriched/most common meaning
                string finalTurkish = !string.IsNullOrWhiteSpace(aiResult.EnrichedTurkishMeaning)
                    ? aiResult.EnrichedTurkishMeaning.Trim()
                    : (!string.IsNullOrWhiteSpace(NewWordTurkish) ? NewWordTurkish.Trim() : aiResult.Turkish);

                var dto = new CreateWordDto
                {
                    EnglishWord = finalEnglish,
                    TurkishMeaning = finalTurkish,
                    WordType = aiResult.GetWordTypeEnum(),
                    ExampleSentence = aiResult.ExampleSentence,
                    ExampleSentenceTranslation = $"{aiResult.ExampleSentenceTranslation} 💡 AI: {aiResult.Explanation}"
                };

                var res = await _apiService.CreateWordWithResultAsync(dto);
                if (res.Success && res.Word != null)
                {
                    var newItem = new UserWordItem
                    {
                        Id = res.Word.Id,
                        EnglishWord = res.Word.EnglishWord,
                        TurkishMeaning = res.Word.TurkishMeaning,
                        WordType = res.Word.WordType,
                        ExampleSentence = aiResult.ExampleSentence,
                        ExampleSentenceTranslation = aiResult.ExampleSentenceTranslation,
                        AiExplanation = aiResult.Explanation,
                        Difficulty = aiResult.Difficulty,
                        DifficultyLevel = aiResult.DifficultyLevel,
                        LearningStatus = LearningStatusEnum.NotReviewed,
                        IsFavorite = false
                    };

                    _allUserWords.Insert(0, newItem);
                    TotalWordsCount = _allUserWords.Count;
                    ApplyFilter();

                    IsAddingWordDialogVisible = false;
                    NewWordEnglish = string.Empty;
                    NewWordTurkish = string.Empty;

                    // Meaningful pedagogical feedback if AI made corrections or added the most common meaning
                    if (!string.IsNullOrWhiteSpace(aiResult.ValidationFeedback))
                    {
                        await Shell.Current.DisplayAlert("Yapay Zekâ İnceledi & Zenginleştirdi ✨", 
                            $"{aiResult.ValidationFeedback}\n\nKelime '{newItem.EnglishWord}' - '{newItem.TurkishMeaning}' olarak listenize eklendi!", "Harika");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Harika! 🎉", 
                            $"'{newItem.EnglishWord}' kelimesi yapay zekâ analizi ve örnek cümlesiyle listenize eklendi!", "Tamam");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Hata", $"Kelime kaydedilemedi: {res.ErrorMessage}", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Hata", $"İşlem sırasında hata oluştu: {ex.Message}", "Tamam");
            }
            finally
            {
                IsAiGeneratingForNewWord = false;
            }
        }

        // --- ACTIONS: FAVORITE & DELETE ---
        [RelayCommand]
        public async Task ToggleFavoriteAsync(UserWordItem item)
        {
            if (item == null) return;
            item.IsFavorite = !item.IsFavorite;
            await _apiService.ToggleFavoriteAsync(item.Id);
        }

        [RelayCommand]
        public async Task DeleteWordAsync(UserWordItem item)
        {
            if (item == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Kelimeyi Sil",
                $"'{item.EnglishWord}' kelimesini silmek istediğinize emin misiniz?",
                "Evet, Sil",
                "İptal");

            if (!confirm) return;

            bool success = await _apiService.DeleteWordAsync(item.Id);
            if (success)
            {
                _allUserWords.Remove(item);
                TotalWordsCount = _allUserWords.Count;
                ApplyFilter();
            }
        }

        // --- USER WORDS TEST ENGINE ---
        private List<UserTestQuestion> _userQuestionsPool = new();

        [RelayCommand]
        public void StartNewTest()
        {
            if (_allUserWords == null || _allUserWords.Count < 4)
            {
                return;
            }

            _userQuestionsPool.Clear();
            CurrentQuestionNumber = 1;
            TotalQuestions = Math.Min(10, _allUserWords.Count);
            CorrectCount = 0;
            WrongCount = 0;
            IsTestCompleted = false;
            TestProgress = 1.0 / TotalQuestions;

            // Sort target words easy to hard
            var pool = _allUserWords.OrderBy(w => w.DifficultyLevel).ThenBy(_ => _random.Next()).Take(TotalQuestions).ToList();

            int qIdx = 1;
            foreach (var target in pool)
            {
                bool askForTerm = qIdx % 2 == 0;
                var wrongPool = _allUserWords.Where(w => w.Id != target.Id).OrderBy(_ => _random.Next()).Take(3).ToList();

                var options = new List<string>();
                if (askForTerm)
                {
                    options.Add(target.EnglishWord);
                    options.AddRange(wrongPool.Select(w => w.EnglishWord));
                }
                else
                {
                    options.Add(target.TurkishMeaning);
                    options.AddRange(wrongPool.Select(w => w.TurkishMeaning));
                }

                var shuffled = options.OrderBy(_ => _random.Next()).ToList();
                int correctIdx = askForTerm ? shuffled.IndexOf(target.EnglishWord) : shuffled.IndexOf(target.TurkishMeaning);

                string qText = askForTerm
                    ? $"\"{target.TurkishMeaning}\" anlamına gelen kelime hangisidir?"
                    : $"'{target.EnglishWord}' kelimesinin doğru Türkçe anlamı nedir?";

                _userQuestionsPool.Add(new UserTestQuestion
                {
                    QuestionIndex = qIdx,
                    Term = target.EnglishWord,
                    QuestionText = qText,
                    Options = shuffled,
                    CorrectOptionIndex = correctIdx,
                    Explanation = $"💡 {target.EnglishWord}: {target.TurkishMeaning}. {target.AiExplanation}",
                    ExampleSentence = target.ExampleSentence,
                    Difficulty = target.Difficulty
                });
                qIdx++;
            }

            LoadCurrentQuestion();
        }

        private void LoadCurrentQuestion()
        {
            if (CurrentQuestionNumber <= _userQuestionsPool.Count)
            {
                CurrentQuestion = _userQuestionsPool[CurrentQuestionNumber - 1];
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

            if (CurrentQuestion.IsCorrect) CorrectCount++;
            else WrongCount++;

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
            SetOptionColor(correctIdx, Color.FromArgb("#DCFCE7"), Color.FromArgb("#16A34A"));
            if (selectedIdx != correctIdx)
            {
                SetOptionColor(selectedIdx, Color.FromArgb("#FEE2E2"), Color.FromArgb("#DC2626"));
            }
        }

        private void SetOptionColor(int index, Color bg, Color border)
        {
            switch (index)
            {
                case 0: OptionAColor = bg; OptionABorderColor = border; break;
                case 1: OptionBColor = bg; OptionBBorderColor = border; break;
                case 2: OptionCColor = bg; OptionCBorderColor = border; break;
                case 3: OptionDColor = bg; OptionDBorderColor = border; break;
            }
        }

        [RelayCommand]
        public void NextQuestion()
        {
            if (CurrentQuestionNumber < TotalQuestions)
            {
                CurrentQuestionNumber++;
                LoadCurrentQuestion();
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
            TestResultMessage = ScorePercentage >= 80 
                ? "🌟 Harika! Kişisel kelime havuzuna çok iyi hakimsin!"
                : "📚 Güzel deneme! Kelime kartlarını tekrar ederek eksiklerini tamamlayabilirsin.";
        }

        // --- USER WORDS QUIZ MATCHING ENGINE (DUOLINGO STYLE TAP-TO-MATCH) ---
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
        public void SelectEnglishWord(UserQuizMatchingItem? item)
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

        private void PairEnglishAndTurkish(UserQuizMatchingItem englishItem, QuizTurkishOptionItem turkishOption)
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

            // Pair them with IDENTICAL 5-distinct colors, badges and glowing 3.5px border
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
            if (_allUserWords == null || _allUserWords.Count < 5) return;

            IsQuizChecked = false;
            QuizFeedbackMessage = string.Empty;
            _selectedEnglishItem = null;
            _selectedTurkishOption = null;

            var chosen = _allUserWords.OrderBy(_ => _random.Next()).Take(5).ToList();
            var items = chosen.Select(w => new UserQuizMatchingItem
            {
                Id = w.Id,
                English = w.EnglishWord,
                CorrectMeaning = w.TurkishMeaning,
                Explanation = w.AiExplanation,
                SelectedMeaning = string.Empty,
                IsActiveSelection = false,
                IsCorrectMatch = null
            }).ToList();

            foreach (var item in items) item.ResetColors();

            QuizItems = new ObservableCollection<UserQuizMatchingItem>(items);
            AvailableMeanings = chosen.Select(w => w.TurkishMeaning).OrderBy(_ => _random.Next()).ToList();

            var turkishList = chosen.Select(w => new QuizTurkishOptionItem
            {
                Text = w.TurkishMeaning,
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

            int correct = 0;
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
                    correct++;
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
            IsQuizSuccess = (correct == QuizItems.Count);
            QuizFeedbackMessage = IsQuizSuccess 
                ? $"🎉 Mükemmel! Tüm {QuizItems.Count} eşleştirmeyi doğru yaptın!"
                : $"Skor: {correct} / {QuizItems.Count} doğru. Kırmızı eşleştirmeleri tekrar gözden geçir.";
        }

        // --- USER WORDS FLASHCARD ENGINE ---
        private List<UserWordItem> _flashcardList = new();

        [RelayCommand]
        public void StartFlashcards()
        {
            if (_allUserWords == null || _allUserWords.Count == 0) return;

            _flashcardList = _allUserWords.OrderBy(w => w.DifficultyLevel).ToList();
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
    }
}
