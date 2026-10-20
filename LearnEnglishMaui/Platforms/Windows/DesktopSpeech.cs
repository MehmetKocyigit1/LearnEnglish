using System.Globalization;
using System.Runtime.InteropServices;

namespace IngilizceProjeMAUI.Services;

/// <summary>Uses installed desktop voices that Windows.Media does not expose.</summary>
internal static class DesktopSpeech
{
    public static Task<bool> TrySpeakEnglishAsync(string text, CancellationToken cancellationToken)
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                completion.TrySetResult(SpeakOnThread(text, cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                completion.TrySetCanceled(cancellationToken);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
        }) { IsBackground = true, Name = "English pronunciation" };

        // Keep all SAPI objects on their owning COM apartment and off the UI thread.
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }

    private static bool SpeakOnThread(string text, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
        if (voiceType is null) return false;

        object? speaker = null;
        object? voices = null;
        object? englishVoice = null;
        try
        {
            speaker = Activator.CreateInstance(voiceType);
            if (speaker is null) return false;
            dynamic speech = speaker;
            voices = speech.GetVoices("", "");
            dynamic availableVoices = voices;

            for (var index = 0; index < (int)availableVoices.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                object? candidate = availableVoices.Item(index);
                try
                {
                    var languageIds = ((string)((dynamic)candidate).GetAttribute("Language")).Split(';');
                    var isEnglish = languageIds.Any(id => IsEnglishLanguage(id, out _));
                    var isAmerican = languageIds.Any(id => IsEnglishLanguage(id, out var isUs) && isUs);
                    if (isEnglish && (englishVoice is null || isAmerican))
                    {
                        Release(englishVoice);
                        englishVoice = candidate;
                        candidate = null;
                        if (isAmerican) break;
                    }
                }
                finally
                {
                    Release(candidate);
                }
            }

            if (englishVoice is null) return false;
            speech.Voice = englishVoice;
            speech.Volume = 100;
            cancellationToken.ThrowIfCancellationRequested();

            // SVSFlagsAsync | SVSFIsNotXML: read user input as plain text.
            speech.Speak(text, 1 | 16);
            while (!(bool)speech.WaitUntilDone(50))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    // SVSFlagsAsync | SVSFPurgeBeforeSpeak stops the active utterance.
                    speech.Speak("", 1 | 2);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            return true;
        }
        finally
        {
            Release(speaker);
            Release(englishVoice);
            Release(voices);
        }
    }

    private static bool IsEnglishLanguage(string languageId, out bool isAmerican)
    {
        isAmerican = false;
        if (!int.TryParse(languageId.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var lcid))
            return false;

        try
        {
            var culture = CultureInfo.GetCultureInfo(lcid);
            isAmerican = culture.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase);
            return culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase);
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    private static void Release(object? instance)
    {
        if (instance is not null && Marshal.IsComObject(instance))
            Marshal.ReleaseComObject(instance);
    }
}
