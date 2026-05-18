using Spectre.Console;

namespace Scaffold.Cli.Handlers;

/// <summary>
/// Defines a root scaffold handler shown in handler selection menus.
/// DisplayName is shown to the user. ExecuteAsync must call context.GoBack()
/// exactly once before returning and must not call Environment.Exit().
/// </summary>
public interface IScaffoldHandler
{
    string DisplayName { get; }
    Task ExecuteAsync(IAnsiConsole console, HandlerContext context);
}
