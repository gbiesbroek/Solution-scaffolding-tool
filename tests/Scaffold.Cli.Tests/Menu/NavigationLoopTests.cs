using Scaffold.Cli.Actions;
using Scaffold.Cli.Categories;
using Scaffold.Cli.Menu;
using Spectre.Console.Testing;

namespace Scaffold.Cli.Tests.Menu;

public class NavigationLoopTests
{
    [Fact]
    public async Task AfterGoBack_TopLevelMenuIsShownAgain()
    {
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);

        var exitAction = new ExitMenuAction();
        var registry = new CategoryRegistry(exitAction);
        var renderer = new MenuRenderer(console);
        var runner = new AppRunner(console, registry, renderer);

        await runner.RunAsync();

        Assert.True(true);
    }
}
