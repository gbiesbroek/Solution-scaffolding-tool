using Scaffold.Cli.Actions;
using Scaffold.Cli.Categories;
using Scaffold.Cli.Menu;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Scaffold.Cli.Tests.Menu;

public class MenuRendererTests
{
    [Fact]
    public async Task ShowMenuAsync_ReturnsSelectedCategory()
    {
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.Enter);

        var renderer = new MenuRenderer(console);
        var categories = new List<Category>
        {
            new("First", new ExitMenuAction()),
            new("Second", new ExitMenuAction()),
        };

        var result = await renderer.ShowMenuAsync(categories, "Pick one");

        Assert.Equal("First", result.DisplayName);
    }
}
