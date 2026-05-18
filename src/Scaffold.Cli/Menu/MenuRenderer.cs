using Scaffold.Cli.Categories;
using Spectre.Console;

namespace Scaffold.Cli.Menu;

public class MenuRenderer : IMenuRenderer
{
    private readonly IAnsiConsole _console;

    public MenuRenderer(IAnsiConsole console)
    {
        _console = console;
    }

    public Task<Category> ShowMenuAsync(IReadOnlyList<Category> categories, string title)
    {
        var prompt = new SelectionPrompt<Category>()
            .Title(title)
            .UseConverter(c => c.DisplayName)
            .WrapAround()
            .AddChoices(categories);

        return Task.FromResult(_console.Prompt(prompt));
    }
}
