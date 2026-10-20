using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace IngilizceProjeMAUI.Services;

/// <summary>Uses local Kokoro on Windows and the device's English voice on mobile.</summary>
public sealed class PronunciationService
{
#if !WINDOWS
    private readonly ITextToSpeech _textToSpeech;
#endif
    private CancellationTokenSource? _currentSpeech;

    public PronunciationService(ITextToSpeech textToSpeech)
    {
#if !WINDOWS
        _textToSpeech = textToSpeech;
#endif
    }

    public void Stop()
    {
        // Calls come from UI commands and page lifecycle events on the main thread.
        _currentSpeech?.Cancel();
        _currentSpeech = null;
    }

    public async Task SpeakAsync(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        Stop();
        using var cancellation = new CancellationTokenSource();
        _currentSpeech = cancellation;

        try
        {
#if WINDOWS
            await KokoroSpeech.SpeakAsync(text.Trim(), cancellation.Token);
#else
            var locales = (await _textToSpeech.GetLocalesAsync()).ToList();
            cancellation.Token.ThrowIfCancellationRequested();

            var englishVoices = locales.Where(locale =>
                string.Equals(locale.Language, "en", StringComparison.OrdinalIgnoreCase) ||
                locale.Language.StartsWith("en-", StringComparison.OrdinalIgnoreCase) ||
                locale.Language.StartsWith("en_", StringComparison.OrdinalIgnoreCase)).ToList();
            var voice = englishVoices.FirstOrDefault(locale =>
                string.Equals(locale.Country, "US", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(locale.Language, "en-US", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(locale.Language, "en_US", StringComparison.OrdinalIgnoreCase))
                ?? englishVoices.FirstOrDefault();

            if (voice is null)
            {
                await ShowMessageAsync("Cihazında İngilizce konuşma sesi bulunamadı. " +
                    "Cihazının metin okuma veya konuşma ayarlarından İngilizce ses paketi yükleyip tekrar dene.");
                return;
            }

            await _textToSpeech.SpeakAsync(text.Trim(), new SpeechOptions
            {
                Locale = voice,
                Pitch = 1.0f,
                Volume = 1.0f
            }, cancellation.Token);
#endif
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // Changing the word or leaving the page intentionally stops playback.
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Pronunciation failed: {exception}");
            if (!cancellation.IsCancellationRequested)
#if WINDOWS
                await ShowMessageAsync("Kokoro seslendirmesi başlatılamadı. İlk kurulum için projenin " +
                    "tools/kokoro-tts/Setup-Kokoro.ps1 dosyasını çalıştırıp uygulamayı yeniden başlat. " +
                    "İlk model indirmesi internet gerektirir.");
#else
                await ShowMessageAsync("Kelime seslendirilemedi. Cihazının ses ve İngilizce metin okuma ayarlarını kontrol edip tekrar dene.");
#endif
        }
        finally
        {
            if (ReferenceEquals(_currentSpeech, cancellation))
                _currentSpeech = null;
        }
    }

    private static Task ShowMessageAsync(string message) =>
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Shell.Current is { } shell)
                await shell.DisplayAlert("Telaffuz", message, "Tamam");
        });
}
