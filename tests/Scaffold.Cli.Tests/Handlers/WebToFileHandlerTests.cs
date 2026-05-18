using Scaffold.Cli.Handlers;
using Spectre.Console.Testing;

namespace Scaffold.Cli.Tests.Handlers;

public class WebToFileHandlerTests
{
    private const string TestUrl = "https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore";
    private const string TestOutputFile = ".gitignore";
    private const string TestDisplayName = "Github/Dotnet/Core/.gitignore";

    private static WebToFileHandler CreateHandler(
        IHttpFileDownloader? downloader = null,
        IFileSystem? fileSystem = null)
        => new(
            TestDisplayName,
            TestUrl,
            TestOutputFile,
            downloader ?? new StubHttpFileDownloader("# content"),
            fileSystem ?? new StubFileSystem());

    [Fact]
    public void DisplayName_ReturnsConfiguredDisplayName()
    {
        Assert.Equal(TestDisplayName, CreateHandler().DisplayName);
    }

    [Fact]
    public void Preview_ReturnsFormattedString()
    {
        Assert.Equal(
            $"Downloads from: {TestUrl} -> {TestOutputFile}",
            CreateHandler().Preview);
    }

    [Fact]
    public async Task ExecuteAsync_FileNotExists_DownloadsAndWritesFile()
    {
        var fs = new StubFileSystem { FileExistsResult = false };
        var downloader = new StubHttpFileDownloader("# gitignore content");
        var handler = CreateHandler(downloader, fs);
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(new TestConsole().Interactive(), ctx);

        Assert.NotNull(fs.WrittenContent);
        Assert.Equal("# gitignore content", fs.WrittenContent);
        Assert.EndsWith(TestOutputFile, fs.WrittenPath);
    }

    [Fact]
    public async Task ExecuteAsync_DownloadSuccess_ShowsConfirmation()
    {
        var handler = CreateHandler(fileSystem: new StubFileSystem());
        var console = new TestConsole().Interactive();
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Contains(".gitignore", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DownloadSuccess_CallsGoBack()
    {
        var handler = CreateHandler();
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(new TestConsole().Interactive(), ctx);

        Assert.True(ctx.ShouldGoBack);
    }

    [Fact]
    public async Task ExecuteAsync_FileExists_UserDeclinesOverwrite_NoDownload()
    {
        var fs = new StubFileSystem { FileExistsResult = true };
        var downloader = new StubHttpFileDownloader("content");
        var handler = CreateHandler(downloader, fs);
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Null(downloader.LastUrl);
        Assert.Null(fs.WrittenContent);
        Assert.True(ctx.ShouldGoBack);
    }

    [Fact]
    public async Task ExecuteAsync_FileExists_UserConfirmsOverwrite_WritesFile()
    {
        var fs = new StubFileSystem { FileExistsResult = true };
        var handler = CreateHandler(new StubHttpFileDownloader("new content"), fs);
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Equal("new content", fs.WrittenContent);
    }

    [Fact]
    public async Task ExecuteAsync_NetworkError_DisplaysError()
    {
        var handler = CreateHandler(
            new StubHttpFileDownloader(new HttpRequestException("connection refused")),
            new StubFileSystem());
        var console = new TestConsole().Interactive();
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Contains("connection refused", console.Output);
        Assert.True(ctx.ShouldGoBack);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyContent_DisplaysError()
    {
        var handler = CreateHandler(
            new StubHttpFileDownloader(new InvalidOperationException("Downloaded content is empty.")),
            new StubFileSystem());
        var console = new TestConsole().Interactive();
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Contains("empty", console.Output);
        Assert.True(ctx.ShouldGoBack);
    }
}
