using Scaffold.Cli.Actions;
using Scaffold.Cli.Menu;
using Spectre.Console.Testing;

namespace Scaffold.Cli.Tests.Actions;

public class ExitMenuActionTests
{
    [Fact]
    public async Task ExecuteAsync_SetsContextShouldExit()
    {
        var console = new TestConsole();
        var context = new NavigationContext();
        var action = new ExitMenuAction();

        await action.ExecuteAsync(console, context);

        Assert.True(context.ShouldExit);
        Assert.False(context.ShouldGoBack);
    }
}
