// IngilizceProje/Application/DTOs/QuizDtos.cs
using System.Collections.Generic;

namespace IngilizceProje.Application.DTOs
{
    public class MatchingPairDto
    {
        public int WordId { get; set; }
        public string EnglishWord { get; set; } = string.Empty;
        public string TurkishMeaning { get; set; } = string.Empty;
    }

    public class MatchingQuizDto
    {
        public List<MatchingPairDto> Pairs { get; set; } = new();
        public List<string> ShuffledTurkishMeanings { get; set; } = new();
    }

    public class TestDto
    {
        public int WordId { get; set; }
        public string EnglishWord { get; set; } = string.Empty;
        public string ExampleSentence { get; set; } = string.Empty;
        public string ExampleSentenceTranslation { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
    }

    public class AnswerTestDto
    {
        public int WordId { get; set; }
        public string SelectedAnswer { get; set; } = string.Empty;
    }

    public class QuizResultDto
    {
        public int WordId { get; set; }
        public bool IsCorrect { get; set; }
    }
}
