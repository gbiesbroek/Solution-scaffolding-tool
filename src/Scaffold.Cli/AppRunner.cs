using Scaffold.Cli.Categories;
using Scaffold.Cli.Menu;
using Spectre.Console;

namespace Scaffold.Cli;

public class AppRunner
{
    private readonly IAnsiConsole _console;
    private readonly ICategoryRegistry _registry;
    private readonly IMenuRenderer _renderer;

    public AppRunner(IAnsiConsole console, ICategoryRegistry registry, IMenuRenderer renderer)
    {
        _console = console;
        _registry = registry;
        _renderer = renderer;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            var ctx = new NavigationContext();
            var category = await _renderer.ShowMenuAsync(_registry.GetCategories(), "What would you like to scaffold?");
            await category.Action.ExecuteAsync(_console, ctx);
            if (ctx.ShouldExit) break;
        }
    }
}
