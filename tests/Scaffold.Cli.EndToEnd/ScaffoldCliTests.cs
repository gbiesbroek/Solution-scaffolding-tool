using System.Diagnostics;

namespace Scaffold.Cli.EndToEnd;

public class ScaffoldCliTests
{
    private static string GetScaffoldExePath()
    {
        var baseDir = new DirectoryInfo(AppContext.BaseDirectory);
        var configName = baseDir.Parent?.Name ?? "Debug";
        var repoRoot = baseDir.Parent?.Parent?.Parent?.Parent?.Parent?.FullName
            ?? throw new InvalidOperationException("Cannot determine repo root from test base directory.");

        var exe = OperatingSystem.IsWindows() ? "Scaffold.Cli.exe" : "Scaffold.Cli";
        return Path.Combine(repoRoot, "src", "Scaffold.Cli", "bin", configName, "net10.0", exe);
    }

    private static ProcessStartInfo CreateStartInfo() => new(GetScaffoldExePath())
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    };

    [Fact]
    public async Task Tool_DisplaysPromptMessage_OnStartup()
    {
        using var process = Process.Start(CreateStartInfo())
            ?? throw new InvalidOperationException("Failed to start scaffold process.");

        // Give the process time to write its prompt then close stdin to unblock ReadLine
        await Task.Delay(500);
        process.StandardInput.Close();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        Assert.Contains("Press Enter to exit...", output);
    }

    [Fact]
    public async Task Tool_RemainsRunning_WhenNoInputProvided()
    {
        using var process = Process.Start(CreateStartInfo())
            ?? throw new InvalidOperationException("Failed to start scaffold process.");

        // Wait briefly — the process should still be alive (blocked on ReadLine)
        await Task.Delay(500);
        var hasExited = process.HasExited;

        // Clean up
        process.StandardInput.Close();
        await process.WaitForExitAsync();

        Assert.False(hasExited, "Process should remain running while waiting for input.");
    }

    [Fact]
    public async Task Tool_ExitsCleanly_AfterReceivingInput()
    {
        using var process = Process.Start(CreateStartInfo())
            ?? throw new InvalidOperationException("Failed to start scaffold process.");

        // Allow the process to start and display its prompt
        await Task.Delay(200);

        // Send Enter to unblock ReadLine
        await process.StandardInput.WriteLineAsync();
        process.StandardInput.Close();

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await process.WaitForExitAsync(cts.Token);

        Assert.Equal(0, process.ExitCode);
    }
}
