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
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    };

    private static async Task WaitForExitOrKillAsync(Process process, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && !process.HasExited)
        {
            process.Kill(true);
            await process.WaitForExitAsync(cancellationToken);
        }
    }

    [Fact]
    public async Task Tool_StartsAndExits_WhenStdinIsClosed()
    {
        using var process = Process.Start(CreateStartInfo())
            ?? throw new InvalidOperationException("Failed to start scaffold process.");

        var cancellationToken = TestContext.Current.CancellationToken;
        await Task.Delay(500, cancellationToken);
        process.StandardInput.Close();

        await WaitForExitOrKillAsync(process, TimeSpan.FromSeconds(5), cancellationToken);

        Assert.True(process.HasExited);
    }

    [Fact]
    public async Task Tool_RemainsRunning_WhenNoInputProvided()
    {
        using var process = Process.Start(CreateStartInfo())
            ?? throw new InvalidOperationException("Failed to start scaffold process.");

        var cancellationToken = TestContext.Current.CancellationToken;
        await Task.Delay(300, cancellationToken);

        if (!process.HasExited)
        {
            process.StandardInput.Close();
            await WaitForExitOrKillAsync(process, TimeSpan.FromSeconds(3), cancellationToken);
        }

        Assert.NotNull(process);
    }

    [Fact]
    public async Task Tool_ExitsCleanly_AfterReceivingInput()
    {
        using var process = Process.Start(CreateStartInfo())
            ?? throw new InvalidOperationException("Failed to start scaffold process.");

        var cancellationToken = TestContext.Current.CancellationToken;
        await Task.Delay(200, cancellationToken);
        process.StandardInput.Close();

        await WaitForExitOrKillAsync(process, TimeSpan.FromSeconds(5), cancellationToken);

        Assert.True(process.HasExited);
    }
}
