using Scaffold.Cli.Handlers;
using Spectre.Console.Testing;

namespace Scaffold.Cli.Tests.Handlers;

public class DotnetNewHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_RunsCorrectExecutable()
    {
        var stub = new StubCommandRunner(0, "output", "");
        var handler = new DotnetNewHandler("gitignore", stub, new StubFileSystem(), ".gitignore");
        var console = new TestConsole().Interactive();
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Equal("dotnet", stub.LastExecutable);
    }

    [Fact]
    public async Task ExecuteAsync_PassesTemplateNameAsArg()
    {
        var stub = new StubCommandRunner(0, "output", "");
        var handler = new DotnetNewHandler("gitignore", stub, new StubFileSystem(), ".gitignore");
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(new TestConsole().Interactive(), ctx);

        Assert.Contains("new", stub.LastArgs!);
        Assert.Contains("gitignore", stub.LastArgs!);
    }

    [Fact]
    public async Task ExecuteAsync_DisplaysOutput()
    {
        var stub = new StubCommandRunner(0, "File created.", "");
        var handler = new DotnetNewHandler("gitignore", stub, new StubFileSystem(), ".gitignore");
        var console = new TestConsole().Interactive();
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Contains("File created.", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_CallsGoBack()
    {
        var stub = new StubCommandRunner(0, "", "");
        var handler = new DotnetNewHandler("gitignore", stub, new StubFileSystem(), ".gitignore");
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(new TestConsole().Interactive(), ctx);

        Assert.True(ctx.ShouldGoBack);
    }

    [Fact]
    public async Task ExecuteAsync_WithExtraArgs_IncludesThemInCall()
    {
        var stub = new StubCommandRunner(0, "", "");
        var handler = new DotnetNewHandler("gitignore", stub, new StubFileSystem(), ".gitignore", new[] { "--force" });
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(new TestConsole().Interactive(), ctx);

        Assert.Contains("--force", stub.LastArgs!);
    }

    [Fact]
    public void DisplayName_IsDotnetNew()
    {
        var handler = new DotnetNewHandler("gitignore", new StubCommandRunner(0, "", ""), new StubFileSystem(), ".gitignore");
        Assert.Equal("dotnet new", handler.DisplayName);
    }

    [Fact]
    public void Preview_MatchesTemplateFormat()
    {
        var handler = new DotnetNewHandler("gitignore", new StubCommandRunner(0, "", ""), new StubFileSystem(), ".gitignore");
        Assert.Equal("Runs: dotnet new gitignore", handler.Preview);
    }

    [Fact]
    public void Preview_WithExtraArgs_IncludesArgs()
    {
        var handler = new DotnetNewHandler("gitignore", new StubCommandRunner(0, "", ""), new StubFileSystem(), ".gitignore", new[] { "--force" });
        Assert.Equal("Runs: dotnet new gitignore --force", handler.Preview);
    }

    [Fact]
    public async Task ExecuteAsync_FileNotExists_RunsCommandDirectly()
    {
        var stub = new StubCommandRunner(0, "", "");
        var fs = new StubFileSystem { FileExistsResult = false };
        var handler = new DotnetNewHandler("gitignore", stub, fs, ".gitignore");
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(new TestConsole().Interactive(), ctx);

        Assert.Equal("dotnet", stub.LastExecutable);
    }

    [Fact]
    public async Task ExecuteAsync_FileExists_UserDeclinesOverwrite_CommandNotRun()
    {
        var stub = new StubCommandRunner(0, "", "");
        var fs = new StubFileSystem { FileExistsResult = true };
        var handler = new DotnetNewHandler("gitignore", stub, fs, ".gitignore");
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Null(stub.LastExecutable);
    }

    [Fact]
    public async Task ExecuteAsync_FileExists_UserConfirmsOverwrite_CommandRuns()
    {
        var stub = new StubCommandRunner(0, "", "");
        var fs = new StubFileSystem { FileExistsResult = true };
        var handler = new DotnetNewHandler("gitignore", stub, fs, ".gitignore");
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Equal("dotnet", stub.LastExecutable);
    }
}
