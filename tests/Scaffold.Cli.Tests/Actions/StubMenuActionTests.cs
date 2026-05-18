using Scaffold.Cli.Actions;
using Scaffold.Cli.Menu;
using Spectre.Console.Testing;

namespace Scaffold.Cli.Tests.Actions;

public class StubMenuActionTests
{
    [Fact]
    public async Task ExecuteAsync_SetsGoBackFlag()
    {
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        var context = new NavigationContext();
        var action = new StubMenuAction("Core");

        await action.ExecuteAsync(console, context);

        Assert.True(context.ShouldGoBack);
        Assert.False(context.ShouldExit);
    }

    [Fact]
    public async Task ExecuteAsync_DisplaysCategoryName()
    {
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        var context = new NavigationContext();
        var action = new StubMenuAction("Core");

        await action.ExecuteAsync(console, context);

        Assert.Contains("Core", console.Output);
    }
}
