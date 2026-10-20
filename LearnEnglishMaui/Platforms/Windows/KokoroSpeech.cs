using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace IngilizceProjeMAUI.Services;

/// <summary>Starts the local Kokoro service and plays its generated WAV audio.</summary>
internal static class KokoroSpeech
{
    private static readonly HttpClient Client = new()
    {
        BaseAddress = new Uri("http://127.0.0.1:17787"),
        Timeout = TimeSpan.FromMinutes(3)
    };
    private static readonly SemaphoreSlim StartLock = new(1, 1);
    private static Process? _server;

    public static async Task SpeakAsync(string text, CancellationToken cancellationToken)
    {
        await EnsureServerAsync(cancellationToken);
        using var response = await Client.PostAsJsonAsync("/speech", new { text }, cancellationToken);
        response.EnsureSuccessStatusCode();
        var audio = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        await PlayAsync(audio, cancellationToken);
    }

    private static async Task EnsureServerAsync(CancellationToken cancellationToken)
    {
        if (await IsKokoroServerAsync(cancellationToken)) return;

        await StartLock.WaitAsync(cancellationToken);
        try
        {
            if (await IsKokoroServerAsync(cancellationToken)) return;
            if (_server is null || _server.HasExited)
            {
                var directory = FindServiceDirectory();
                if (!File.Exists(Path.Combine(directory, "node_modules", "kokoro-js", "package.json")))
                    throw new InvalidOperationException("Önce tools/kokoro-tts/Setup-Kokoro.ps1 çalıştırılmalı.");

                var installedNode = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs", "node.exe");
                var startInfo = new ProcessStartInfo
                {
                    FileName = File.Exists(installedNode) ? installedNode : "node.exe",
                    WorkingDirectory = directory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                startInfo.ArgumentList.Add(Path.Combine(directory, "server.mjs"));
                startInfo.ArgumentList.Add("--parent-pid");
                startInfo.ArgumentList.Add(Environment.ProcessId.ToString());
                _server?.Dispose();
                _server = Process.Start(startInfo) ?? throw new InvalidOperationException("Kokoro başlatılamadı.");
                _server.OutputDataReceived += (_, args) => Debug.WriteLine($"Kokoro: {args.Data}");
                _server.ErrorDataReceived += (_, args) => Debug.WriteLine($"Kokoro: {args.Data}");
                _server.BeginOutputReadLine();
                _server.BeginErrorReadLine();
            }

            for (var attempt = 0; attempt < 40; attempt++)
            {
                if (await IsKokoroServerAsync(cancellationToken)) return;
                if (_server.HasExited)
                    throw new InvalidOperationException("Kokoro servisi kapandı. Kurulum ve model dosyalarını kontrol edin.");
                await Task.Delay(200, cancellationToken);
            }
            throw new TimeoutException("Kokoro servisi başlatılamadı.");
        }
        finally
        {
            StartLock.Release();
        }
    }

    private static async Task<bool> IsKokoroServerAsync(CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(500);
        try
        {
            using var response = await Client.GetAsync("/health", timeout.Token);
            if (!response.IsSuccessStatusCode) return false;
            using var data = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(timeout.Token), cancellationToken: timeout.Token);
            return data.RootElement.TryGetProperty("engine", out var engine) && engine.GetString() == "kokoro";
        }
        catch (HttpRequestException) { return false; }
        catch (JsonException) { return false; }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return false; }
    }

    private static string FindServiceDirectory()
    {
        var configured = Environment.GetEnvironmentVariable("LEARNENGLISH_KOKORO_DIR");
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(Path.Combine(configured, "server.mjs")))
            return Path.GetFullPath(configured);

        for (DirectoryInfo? current = new(AppContext.BaseDirectory); current is not null; current = current.Parent)
        {
            var candidate = Path.Combine(current.FullName, "tools", "kokoro-tts");
            if (File.Exists(Path.Combine(candidate, "server.mjs"))) return candidate;
        }
        throw new DirectoryNotFoundException("tools/kokoro-tts bulunamadı. LEARNENGLISH_KOKORO_DIR ayarlanabilir.");
    }

    private static async Task PlayAsync(byte[] audio, CancellationToken cancellationToken)
    {
        using var stream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(stream))
        {
            writer.WriteBytes(audio);
            await writer.StoreAsync();
            writer.DetachStream();
        }
        cancellationToken.ThrowIfCancellationRequested();
        stream.Seek(0);

        using var source = MediaSource.CreateFromStream(stream, "audio/wav");
        using var player = new MediaPlayer { Source = source, Volume = 1 };
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Ended(MediaPlayer sender, object args) => finished.TrySetResult();
        void Failed(MediaPlayer sender, MediaPlayerFailedEventArgs args) =>
            finished.TrySetException(new InvalidOperationException($"Ses çalınamadı: {args.ErrorMessage}"));

        player.MediaEnded += Ended;
        player.MediaFailed += Failed;
        using var registration = cancellationToken.Register(() =>
        {
            player.Pause();
            finished.TrySetCanceled(cancellationToken);
        });
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            player.Play();
            await finished.Task;
        }
        finally
        {
            player.MediaEnded -= Ended;
            player.MediaFailed -= Failed;
        }
    }
}
