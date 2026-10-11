// IngilizceProje/MAUIClient/ViewModels/QuizTurkishOptionItem.cs
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace IngilizceProjeMAUI.ViewModels
{
    public partial class QuizTurkishOptionItem : ObservableObject
    {
        [ObservableProperty]
        private string _text = string.Empty;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isMatched;

        [ObservableProperty]
        private string? _matchedEnglishWord;

        [ObservableProperty]
        private bool? _isCorrectMatch;

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
}
