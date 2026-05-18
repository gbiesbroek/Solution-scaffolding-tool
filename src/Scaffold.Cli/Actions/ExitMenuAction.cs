using Scaffold.Cli.Menu;
using Spectre.Console;

namespace Scaffold.Cli.Actions;

public class ExitMenuAction : IMenuAction
{
    public Task ExecuteAsync(IAnsiConsole console, NavigationContext context)
    {
        context.Exit();
        return Task.CompletedTask;
    }
}
