// IngilizceProje/MAUIClient/ViewModels/QuizTurkishOptionItem.cs
using CommunityToolkit.Mvvm.ComponentModel;

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
        private string _matchBorderColor = "#E2E8F0";

        [ObservableProperty]
        private string _matchBgColor = "#FFFFFF";

        [ObservableProperty]
        private string _matchBadgeBgColor = "#EEF2FF";

        [ObservableProperty]
        private string _matchTextColor = "#0F172A";

        [ObservableProperty]
        private string _pairBadgeText = string.Empty;

        public void ResetColors()
        {
            PairIndex = -1;
            MatchBorderColor = "#E2E8F0";
            MatchBgColor = "#FFFFFF";
            MatchBadgeBgColor = "#EEF2FF";
            MatchTextColor = "#0F172A";
            PairBadgeText = string.Empty;
        }

        public void ApplyPairColor(int index, string borderHex, string bgHex, string badgeBgHex, string textHex)
        {
            PairIndex = index;
            MatchBorderColor = borderHex;
            MatchBgColor = bgHex;
            MatchBadgeBgColor = badgeBgHex;
            MatchTextColor = textHex;
            PairBadgeText = $"#{index + 1}";
        }
    }
}
