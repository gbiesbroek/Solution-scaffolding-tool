using Scaffold.Cli.Handlers;
using Spectre.Console.Testing;

namespace Scaffold.Cli.Tests.Handlers;

public class DotnetNewHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_RunsCorrectExecutable()
    {
        var stub = new StubCommandRunner(0, "output", "");
        var handler = new DotnetNewHandler("gitignore", stub);
        var console = new TestConsole().Interactive();
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Equal("dotnet", stub.LastExecutable);
    }

    [Fact]
    public async Task ExecuteAsync_PassesTemplateNameAsArg()
    {
        var stub = new StubCommandRunner(0, "output", "");
        var handler = new DotnetNewHandler("gitignore", stub);
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(new TestConsole().Interactive(), ctx);

        Assert.Contains("new", stub.LastArgs!);
        Assert.Contains("gitignore", stub.LastArgs!);
    }

    [Fact]
    public async Task ExecuteAsync_DisplaysOutput()
    {
        var stub = new StubCommandRunner(0, "File created.", "");
        var handler = new DotnetNewHandler("gitignore", stub);
        var console = new TestConsole().Interactive();
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(console, ctx);

        Assert.Contains("File created.", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_CallsGoBack()
    {
        var stub = new StubCommandRunner(0, "", "");
        var handler = new DotnetNewHandler("gitignore", stub);
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(new TestConsole().Interactive(), ctx);

        Assert.True(ctx.ShouldGoBack);
    }

    [Fact]
    public async Task ExecuteAsync_WithExtraArgs_IncludesThemInCall()
    {
        var stub = new StubCommandRunner(0, "", "");
        var handler = new DotnetNewHandler("gitignore", stub, new[] { "--force" });
        var ctx = new HandlerContext(Directory.GetCurrentDirectory());

        await handler.ExecuteAsync(new TestConsole().Interactive(), ctx);

        Assert.Contains("--force", stub.LastArgs!);
    }

    [Fact]
    public void DisplayName_IsDotnetNew()
    {
        var handler = new DotnetNewHandler("gitignore", new StubCommandRunner(0, "", ""));
        Assert.Equal("dotnet new", handler.DisplayName);
    }
}
