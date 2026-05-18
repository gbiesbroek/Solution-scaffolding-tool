using Scaffold.Cli.Menu;
using Spectre.Console;

namespace Scaffold.Cli.Actions;

public class StubMenuAction : IMenuAction
{
    private readonly string _categoryName;

    public StubMenuAction(string categoryName)
    {
        _categoryName = categoryName;
    }

    public async Task ExecuteAsync(IAnsiConsole console, NavigationContext context)
    {
        console.MarkupLine($"[yellow]{_categoryName}[/] coming soon...");

        var prompt = new SelectionPrompt<string>()
            .Title("What would you like to do?")
            .AddChoices("<- Back");

        await Task.FromResult(console.Prompt(prompt));
        context.GoBack();
    }
}
