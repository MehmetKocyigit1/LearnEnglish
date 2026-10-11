// IngilizceProje/Infrastructure/Services/AuthService.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using IngilizceProje.Application.DTOs;
using IngilizceProje.Application.Interfaces;
using IngilizceProje.Domain.Entities;

namespace IngilizceProje.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<TokenDto?> LoginAsync(LoginDto loginDto)
        {
            if (loginDto == null) return null;

            var identifier = loginDto.EmailOrUserName?.Trim() ?? string.Empty;
            var password = loginDto.Password?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
                return null;

            // Search by UserName or Email, or fallback synthetic email
            var user = await _userManager.FindByNameAsync(identifier) 
                       ?? await _userManager.FindByEmailAsync(identifier);

            if (user == null && !identifier.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync($"{identifier}@local.app");
            }

            if (user == null) return null;

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid) return null;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var jwtKey = _configuration["Jwt:Key"] ?? "IngilizceProjeSuperSecretSecurityKey123!!!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryDays = double.TryParse(_configuration["Jwt:ExpiryDays"], out var days) ? days : 30;
            var expiration = DateTime.UtcNow.AddDays(expiryDays);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "IngilizceProjeAPI",
                audience: _configuration["Jwt:Audience"] ?? "IngilizceProjeMAUI",
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new TokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty
            };
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto)
        {
            if (registerDto == null)
                return (false, "Kayıt bilgileri boş olamaz.");

            var userName = registerDto.UserName?.Trim() ?? string.Empty;
            var rawEmail = registerDto.Email?.Trim() ?? string.Empty;
            var password = registerDto.Password?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userName))
                return (false, "Lütfen bir kullanıcı adı girin.");

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Lütfen bir şifre girin.");

            if (password.Length < 3)
                return (false, "Şifreniz en az 3 karakter olmalıdır.");

            // Otomatik e-posta tamamlama (kullanıcı sadece rumuz girmişse veya @ yoksa)
            string email = rawEmail;
            if (string.IsNullOrWhiteSpace(email))
            {
                email = $"{userName.ToLowerInvariant()}@local.app";
            }
            else if (!email.Contains('@'))
            {
                email = $"{email}@local.app";
            }

            var existingUserByName = await _userManager.FindByNameAsync(userName);
            if (existingUserByName != null)
            {
                return (false, $"'{userName}' kullanıcı adı zaten kullanımda. Lütfen farklı bir kullanıcı adı seçin.");
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(email);
            if (existingUserByEmail != null)
            {
                return (false, $"'{email}' e-posta adresi zaten kayıtlı. Lütfen giriş yapın veya farklı bir e-posta kullanın.");
            }

            var user = new AppUser
            {
                Email = email,
                UserName = userName
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errorMessages = new System.Collections.Generic.List<string>();
                foreach (var error in result.Errors)
                {
                    string msg = error.Code switch
                    {
                        "DuplicateUserName" => $"'{userName}' kullanıcı adı zaten kullanımda.",
                        "DuplicateEmail" => $"'{email}' e-posta adresi zaten kayıtlı.",
                        "InvalidUserName" => "Kullanıcı adı geçersiz karakterler içeriyor.",
                        "InvalidEmail" => "Geçerli bir e-posta adresi giriniz (örnek: ornek@mail.com).",
                        "PasswordTooShort" => "Şifreniz çok kısa (en az 3 karakter olmalı).",
                        "PasswordRequiresDigit" => "Şifre en az bir rakam içermelidir.",
                        "PasswordRequiresUpper" => "Şifre en az bir büyük harf içermelidir.",
                        "PasswordRequiresLower" => "Şifre en az bir küçük harf içermelidir.",
                        "PasswordRequiresNonAlphanumeric" => "Şifre en az bir özel karakter içermelidir.",
                        _ => error.Description
                    };
                    errorMessages.Add(msg);
                }
                return (false, string.Join(" ", errorMessages));
            }

            return (true, "Kayıt başarıyla tamamlandı.");
        }
    }
}
// IngilizceProje/Infrastructure/Services/AuthService.cs
