// IngilizceProje/Application/DTOs/AiChatDtos.cs
using System;

namespace IngilizceProje.Application.DTOs
{
    public class AiChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
    }

    public class AiChatResponseDto
    {
        public string Reply { get; set; } = string.Empty;
    }
}
