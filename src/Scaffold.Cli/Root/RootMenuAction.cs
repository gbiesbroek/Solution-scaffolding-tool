using Scaffold.Cli.Actions;
using Scaffold.Cli.Handlers;
using Scaffold.Cli.Menu;
using Spectre.Console;

namespace Scaffold.Cli.Root;

public class RootMenuAction : IMenuAction
{
    private readonly IRootItemRegistry _registry;

    public RootMenuAction(IRootItemRegistry registry)
    {
        _registry = registry;
    }

    public async Task ExecuteAsync(IAnsiConsole console, NavigationContext navContext)
    {
        while (true)
        {
            var items = _registry.GetItems();

            var itemPrompt = new SelectionPrompt<string>()
                .Title("Root scaffolding")
                .WrapAround()
                .AddChoices(items.Select(i => i.DisplayName).Append("<- Back"));

            var selectedItemName = console.Prompt(itemPrompt);
            if (selectedItemName == "<- Back")
            {
                navContext.GoBack();
                return;
            }

            var selectedItem = items.First(i => i.DisplayName == selectedItemName);

            while (true)
            {
                var handlerPrompt = new SelectionPrompt<string>()
                    .Title($"How would you like to scaffold {selectedItem.DisplayName}?")
                    .WrapAround()
                    .AddChoices(selectedItem.Handlers.Select(h => h.DisplayName).Append("<- Back"));

                var selectedHandlerName = console.Prompt(handlerPrompt);
                if (selectedHandlerName == "<- Back")
                    break;

                var selectedHandler = selectedItem.Handlers.First(h => h.DisplayName == selectedHandlerName);

                console.WriteLine();
                console.MarkupLine($"  [grey]{Markup.Escape(selectedHandler.Preview)}[/]");
                console.WriteLine();
                var confirm = console.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Proceed?")
                        .WrapAround()
                        .AddChoices("Confirm", "<- Back"));

                if (confirm == "<- Back")
                    continue;

                var ctx = new HandlerContext(Directory.GetCurrentDirectory());
                await selectedHandler.ExecuteAsync(console, ctx);

                if (ctx.ShouldGoBack)
                    break;
            }
        }
    }
}
