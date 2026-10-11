// IngilizceProje/Application/DTOs/WordDtos.cs
using System;
using IngilizceProje.Domain.Enums;

namespace IngilizceProje.Application.DTOs
{
    public class WordDto
    {
        public int Id { get; set; }
        public string EnglishWord { get; set; } = string.Empty;
        public string TurkishMeaning { get; set; } = string.Empty;
        public WordTypeEnum WordType { get; set; }
        public string ExampleSentence { get; set; } = string.Empty;
        public string ExampleSentenceTranslation { get; set; } = string.Empty;
        public LearningStatusEnum LearningStatus { get; set; }
        public bool IsFavorite { get; set; }
        public int ReviewCount { get; set; }
        public int CorrectAnswerCount { get; set; }
        public int WrongAnswerCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class CreateWordDto
    {
        public string EnglishWord { get; set; } = string.Empty;
        public string TurkishMeaning { get; set; } = string.Empty;
        public WordTypeEnum WordType { get; set; } = WordTypeEnum.Noun;
        public string ExampleSentence { get; set; } = string.Empty;
        public string ExampleSentenceTranslation { get; set; } = string.Empty;
    }

    public class UpdateWordDto
    {
        public int Id { get; set; }
        public string EnglishWord { get; set; } = string.Empty;
        public string TurkishMeaning { get; set; } = string.Empty;
        public WordTypeEnum WordType { get; set; }
        public string ExampleSentence { get; set; } = string.Empty;
        public string ExampleSentenceTranslation { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public LearningStatusEnum LearningStatus { get; set; }
    }
}
