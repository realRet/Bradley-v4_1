using System.Diagnostics;
using System.Net;

namespace Bradley_v4_1;
public class LavalinkManager
{
    private readonly string _lavalinkUrl = "https://github.com/lavalink-devs/Lavalink/releases/download/4.0.8/Lavalink.jar";
    private readonly string _lavalinkFileName = "Lavalink.jar";
    private Process _lavalinkProcess;

    public async Task DownloadAndStartLavalinkAsync()
    {
        if (!File.Exists(_lavalinkFileName))
        {
            Console.WriteLine("Downloading Lavalink...");
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(_lavalinkUrl, _lavalinkFileName);
            }
            Console.WriteLine("Lavalink downloaded successfully.");
        }

        Console.WriteLine("Starting Lavalink...");
        _lavalinkProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = $"-jar {_lavalinkFileName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        _lavalinkProcess.Start();

        _lavalinkProcess.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                Console.WriteLine($"Lavalink: {args.Data}");
            }
        };

        _lavalinkProcess.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                Console.WriteLine($"Lavalink Error: {args.Data}");
            }
        };

        _lavalinkProcess.BeginOutputReadLine();
        _lavalinkProcess.BeginErrorReadLine();

        await Task.Delay(15000);

        Console.WriteLine("Lavalink started.");
    }

    public void StopLavalink()
    {
        if (_lavalinkProcess != null && !_lavalinkProcess.HasExited)
        {
            _lavalinkProcess.Kill();
            _lavalinkProcess.Dispose();
            Console.WriteLine("Lavalink stopped.");
        }
    }
}