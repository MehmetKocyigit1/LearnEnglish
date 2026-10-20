// IngilizceProje/MAUIClient/MauiProgram.cs
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using IngilizceProjeMAUI.Services;
using IngilizceProjeMAUI.ViewModels;
using IngilizceProjeMAUI.Views;

namespace IngilizceProjeMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification() // Initialize local notification plugin
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
                    fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemiBold");
                    fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
                    fonts.AddFont("PlusJakartaSans.ttf", "PlusJakartaSans");
                });

            // Register Services
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<NotificationService>();
            builder.Services.AddSingleton<GeminiAiService>();
            builder.Services.AddSingleton<Microsoft.Maui.Media.ITextToSpeech>(Microsoft.Maui.Media.TextToSpeech.Default);
            builder.Services.AddSingleton<PronunciationService>();

            // Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<WordListViewModel>();
            builder.Services.AddTransient<WordDetailViewModel>();
            builder.Services.AddTransient<AddWordViewModel>();
            builder.Services.AddTransient<FlashcardViewModel>();
            builder.Services.AddTransient<MatchingQuizViewModel>();
            builder.Services.AddTransient<TestViewModel>();
            builder.Services.AddTransient<StatsAndGoalViewModel>();
            builder.Services.AddTransient<AiChatViewModel>();
            builder.Services.AddTransient<TechDictionaryViewModel>();

            // Register Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<WordListPage>();
            builder.Services.AddTransient<WordDetailPage>();
            builder.Services.AddTransient<AddWordPage>();
            builder.Services.AddTransient<FlashcardPage>();
            builder.Services.AddTransient<MatchingQuizPage>();
            builder.Services.AddTransient<TestPage>();
            builder.Services.AddTransient<StatsAndGoalPage>();
            builder.Services.AddTransient<AiChatPage>();
            builder.Services.AddTransient<TechDictionaryPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
