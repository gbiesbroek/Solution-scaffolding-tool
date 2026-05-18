using Spectre.Console;
using Scaffold.Cli.Menu;

namespace Scaffold.Cli.Actions;

public interface IMenuAction
{
    Task ExecuteAsync(IAnsiConsole console, NavigationContext context);
}
