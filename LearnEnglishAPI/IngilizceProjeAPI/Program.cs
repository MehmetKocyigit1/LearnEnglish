// IngilizceProje/API/Program.cs
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using IngilizceProje.Application.DTOs;
using IngilizceProje.Application.Interfaces;
using IngilizceProje.Domain.Entities;
using IngilizceProje.Domain.Enums;
using IngilizceProje.Infrastructure.Data;
using IngilizceProje.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Default connection string not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("IngilizceProje.Infrastructure")));

// Add Identity Services
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = false;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ abcçdefgğhıijklmnoöprsştuüvyzABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZ";
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "IngilizceProjeSuperSecretSecurityKey123!!!";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "IngilizceProjeAPI",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "IngilizceProjeMAUI",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IGeminiChatService, GeminiChatService>();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ingilizce Kelime Ogrenme API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Auto-apply EF Core Migrations and Seed Demo User on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        int retries = 15;
        while (retries > 0)
        {
            try
            {
                logger.LogInformation("Veritabanı migration uygulanıyor...");
                db.Database.Migrate();
                logger.LogInformation("Veritabanı migration başarıyla tamamlandı.");
                break;
            }
            catch (Exception ex)
            {
                retries--;
                logger.LogWarning("Veritabanına bağlanılamadı, yeniden deneniyor (kalan deneme: {Retries}). Hata: {Message}", retries, ex.Message);
                if (retries == 0) throw;
                Thread.Sleep(3000);
            }
        }

        // Seed demo user
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var demoUser = userManager.FindByNameAsync("demo").GetAwaiter().GetResult();
        if (demoUser == null)
        {
            demoUser = new AppUser
            {
                UserName = "demo",
                Email = "demo@example.com"
            };
            var createResult = userManager.CreateAsync(demoUser, "Demo123!").GetAwaiter().GetResult();
            if (createResult.Succeeded)
            {
                logger.LogInformation("Demo kullanıcı oluşturuldu (Kullanıcı: demo, Şifre: Demo123!).");

                // Seed some initial words for demo user
                if (!db.Words.Any(w => w.UserId == demoUser.Id))
                {
                    db.Words.AddRange(new List<Word>
                    {
                        new() {
                            EnglishWord = "serendipity",
                            TurkishMeaning = "tesadüfi güzellik / şans eseri keşif",
                            WordType = WordTypeEnum.Noun,
                            ExampleSentence = "Finding this cozy bookstore was pure serendipity.",
                            ExampleSentenceTranslation = "Bu şirin kitapçıyı bulmak tam bir tesadüfi güzellikti.",
                            LearningStatus = LearningStatusEnum.NotReviewed,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = demoUser.Id,
                            UserId = demoUser.Id
                        },
                        new() {
                            EnglishWord = "resilient",
                            TurkishMeaning = "dirençli / çabuk toparlanan",
                            WordType = WordTypeEnum.Adjective,
                            ExampleSentence = "She remained resilient despite facing many challenges.",
                            ExampleSentenceTranslation = "Birçok zorlukla karşılaşmasına rağmen dirençli kaldı.",
                            LearningStatus = LearningStatusEnum.NeedsReview,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = demoUser.Id,
                            UserId = demoUser.Id
                        },
                        new() {
                            EnglishWord = "comprehend",
                            TurkishMeaning = "kavramak / anlamak",
                            WordType = WordTypeEnum.Verb,
                            ExampleSentence = "It took me a while to comprehend the grammar rule.",
                            ExampleSentenceTranslation = "Dilbilgisi kuralını kavramam biraz zaman aldı.",
                            LearningStatus = LearningStatusEnum.Learned,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = demoUser.Id,
                            UserId = demoUser.Id
                        },
                        new() {
                            EnglishWord = "meticulous",
                            TurkishMeaning = "titiz / kılı kırk yaran",
                            WordType = WordTypeEnum.Adjective,
                            ExampleSentence = "He takes meticulous notes while reading English books.",
                            ExampleSentenceTranslation = "İngilizce kitap okurken çok titiz notlar alır.",
                            LearningStatus = LearningStatusEnum.VeryImportant,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = demoUser.Id,
                            UserId = demoUser.Id
                        },
                        new() {
                            EnglishWord = "fluent",
                            TurkishMeaning = "akıcı (dil)",
                            WordType = WordTypeEnum.Adjective,
                            ExampleSentence = "Practice every day to become fluent in English.",
                            ExampleSentenceTranslation = "İngilizcede akıcı olmak için her gün pratik yap.",
                            LearningStatus = LearningStatusEnum.NotReviewed,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = demoUser.Id,
                            UserId = demoUser.Id
                        }
                    });
                    db.SaveChanges();
                    logger.LogInformation("Örnek başlangıç kelimeleri eklendi.");
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Veritabanı başlatılırken hata oluştu.");
    }
}

// Enable Swagger UI always (for testing and integration)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ingilizce API v1");
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Helper to get UserId from Claims
string GetUserId(ClaimsPrincipal user)
{
    return user.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? throw new UnauthorizedAccessException("Giriş yapılmış kullanıcı kimliği bulunamadı.");
}

#region Auth Endpoints
app.MapPost("/api/auth/register", async (RegisterDto dto, IAuthService authService) =>
{
    var (success, message) = await authService.RegisterAsync(dto);
    return success ? Results.Ok(new { Message = message })
                   : Results.BadRequest(new { Message = message });
})
.WithName("Register");

app.MapPost("/api/auth/login", async (LoginDto dto, IAuthService authService) =>
{
    var token = await authService.LoginAsync(dto);
    return token != null ? Results.Ok(token)
                         : Results.Unauthorized();
})
.WithName("Login");

app.MapPost("/api/auth/demo-login", async (IAuthService authService) =>
{
    var token = await authService.LoginAsync(new LoginDto
    {
        EmailOrUserName = "demo",
        Password = "Demo123!"
    });
    return token != null ? Results.Ok(token) : Results.Unauthorized();
})
.WithName("DemoLogin");
#endregion

#region Word Endpoints
app.MapGet("/api/words", async (ClaimsPrincipal user, IWordService wordService) =>
{
    try
    {
        var words = await wordService.GetAllWordsAsync(GetUserId(user));
        return Results.Ok(words);
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();

app.MapGet("/api/words/{id:int}", async (int id, ClaimsPrincipal user, IWordService wordService) =>
{
    try
    {
        var word = await wordService.GetWordByIdAsync(id, GetUserId(user));
        return word != null ? Results.Ok(word) : Results.NotFound();
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();

app.MapPost("/api/words", async (CreateWordDto dto, ClaimsPrincipal user, IWordService wordService) =>
{
    try
    {
        var created = await wordService.CreateWordAsync(dto, GetUserId(user));
        return Results.Created($"/api/words/{created.Id}", created);
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { Message = ex.Message }); }
})
.RequireAuthorization();

app.MapPut("/api/words/{id:int}", async (int id, UpdateWordDto dto, ClaimsPrincipal user, IWordService wordService) =>
{
    try
    {
        if (id != dto.Id) return Results.BadRequest(new { Message = "Id eşleşmiyor." });
        var updated = await wordService.UpdateWordAsync(dto, GetUserId(user));
        return updated != null ? Results.Ok(updated) : Results.NotFound();
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { Message = ex.Message }); }
})
.RequireAuthorization();

app.MapDelete("/api/words/{id:int}", async (int id, ClaimsPrincipal user, IWordService wordService) =>
{
    try
    {
        var success = await wordService.DeleteWordAsync(id, GetUserId(user));
        return success ? Results.Ok(new { Message = "Kelime başarıyla silindi." }) : Results.NotFound();
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();

app.MapPut("/api/words/{id:int}/favorite", async (int id, ClaimsPrincipal user, IWordService wordService) =>
{
    try
    {
        var isFavorite = await wordService.ToggleFavoriteAsync(id, GetUserId(user));
        return Results.Ok(new { IsFavorite = isFavorite });
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();

app.MapGet("/api/words/random-review", async (ClaimsPrincipal user, IWordService wordService) =>
{
    try
    {
        var word = await wordService.GetRandomWordForReviewAsync(GetUserId(user));
        return word != null ? Results.Ok(word) : Results.NotFound(new { Message = "Tekrar edilecek kelime bulunamadı. Lütfen kelime ekleyin." });
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();

app.MapPost("/api/words/{id:int}/review", async (int id, bool isCorrect, ClaimsPrincipal user, IWordService wordService) =>
{
    try
    {
        var success = await wordService.SubmitReviewResultAsync(id, isCorrect, GetUserId(user));
        return success ? Results.Ok(new { Message = "Tekrar sonucu başarıyla kaydedildi." }) : Results.NotFound();
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();

app.MapGet("/api/words/export/excel", async (ClaimsPrincipal user, IExcelExportService exportService) =>
{
    try
    {
        var fileBytes = await exportService.ExportToExcelAsync(GetUserId(user));
        return Results.File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "kelimelerim.xlsx");
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();

app.MapGet("/api/words/export/csv", async (ClaimsPrincipal user, IExcelExportService exportService) =>
{
    try
    {
        var fileBytes = await exportService.ExportToCsvAsync(GetUserId(user));
        return Results.File(fileBytes, "text/csv", "kelimelerim.csv");
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();
#endregion

#region Quiz Endpoints
app.MapGet("/api/quizzes/matching", async (ClaimsPrincipal user, IQuizService quizService) =>
{
    try
    {
        var quiz = await quizService.GenerateMatchingQuizAsync(GetUserId(user));
        return quiz != null ? Results.Ok(quiz) : Results.NotFound();
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { Message = ex.Message }); }
})
.RequireAuthorization();

app.MapGet("/api/quizzes/multiple-choice", async (ClaimsPrincipal user, IQuizService quizService) =>
{
    try
    {
        var test = await quizService.GenerateMultipleChoiceTestAsync(GetUserId(user));
        return test != null ? Results.Ok(test) : Results.NotFound();
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { Message = ex.Message }); }
})
.RequireAuthorization();

app.MapPost("/api/quizzes/submit-result", async (QuizResultDto dto, ClaimsPrincipal user, IQuizService quizService) =>
{
    try
    {
        var success = await quizService.SubmitQuizResultAsync(dto, GetUserId(user));
        return success ? Results.Ok(new { Message = "Test sonucu başarıyla kaydedildi." }) : Results.NotFound();
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();
#endregion

#region Statistics Endpoints
app.MapGet("/api/statistics/summary", async (ClaimsPrincipal user, IStatisticsService statsService) =>
{
    try
    {
        var stats = await statsService.GetUserStatisticsAsync(GetUserId(user));
        return Results.Ok(stats);
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();

app.MapGet("/api/statistics/daily-goal", async (ClaimsPrincipal user, IStatisticsService statsService) =>
{
    try
    {
        var goal = await statsService.GetDailyGoalAsync(GetUserId(user));
        return goal != null ? Results.Ok(goal) : Results.Ok(new { TargetCount = 10 }); // Default 10
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
})
.RequireAuthorization();

app.MapPost("/api/statistics/daily-goal", async (int targetCount, ClaimsPrincipal user, IStatisticsService statsService) =>
{
    try
    {
        var success = await statsService.SetDailyGoalAsync(targetCount, GetUserId(user));
        return success ? Results.Ok(new { Message = "Günlük hedef başarıyla güncellendi." }) : Results.BadRequest();
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
    catch (ArgumentException ex) { return Results.BadRequest(new { Message = ex.Message }); }
})
.RequireAuthorization();
#endregion

#region AI Chat Endpoints
app.MapPost("/api/ai-chat/send", async (AiChatRequestDto dto, ClaimsPrincipal user, IGeminiChatService aiService) =>
{
    try
    {
        var userId = GetUserId(user);
        var reply = await aiService.GetChatReplyAsync(userId, dto);
        return Results.Ok(reply);
    }
    catch (UnauthorizedAccessException) { return Results.Unauthorized(); }
    catch (Exception ex) { return Results.BadRequest(new { Message = ex.Message }); }
})
.RequireAuthorization()
.WithName("SendAiChatMessage");
#endregion

app.Run();