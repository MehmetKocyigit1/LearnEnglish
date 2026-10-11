// IngilizceProje/Application/DTOs/AuthDtos.cs
using System;

namespace IngilizceProje.Application.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string EmailOrUserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class TokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
