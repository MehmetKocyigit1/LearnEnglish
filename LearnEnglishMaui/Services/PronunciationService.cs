using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace IngilizceProjeMAUI.Services;

/// <summary>Uses the device's English voice without an external API key.</summary>
public sealed class PronunciationService(ITextToSpeech textToSpeech)
{
    private CancellationTokenSource? _currentSpeech;

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
            var locales = (await textToSpeech.GetLocalesAsync()).ToList();
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
#if WINDOWS
                await ShowMessageAsync("Uygulamanın kullanabileceği İngilizce metin okuma sesi bulunamadı. " +
                    "Windows Ayarları > Zaman ve dil > Konuşma > Sesleri yönet > Ses ekle bölümünden " +
                    "English (United States) ses paketini yükle, ardından uygulamayı kapatıp yeniden aç. " +
                    "Yalnızca İngilizce klavye eklemek yeterli değildir.");
#else
                await ShowMessageAsync("Cihazında İngilizce konuşma sesi bulunamadı. " +
                    "Cihazının metin okuma veya konuşma ayarlarından İngilizce ses paketi yükleyip tekrar dene.");
#endif
                return;
            }

            await textToSpeech.SpeakAsync(text.Trim(), new SpeechOptions
            {
                Locale = voice,
                Pitch = 1.0f,
                Volume = 1.0f
            }, cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // Changing the word or leaving the page intentionally stops playback.
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Pronunciation failed: {exception}");
            if (!cancellation.IsCancellationRequested)
                await ShowMessageAsync("Kelime seslendirilemedi. Cihazının ses ve İngilizce metin okuma ayarlarını kontrol edip tekrar dene.");
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
