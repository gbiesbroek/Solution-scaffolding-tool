using Scaffold.Cli.Actions;

namespace Scaffold.Cli.Categories;

public class CategoryRegistry : ICategoryRegistry
{
    private readonly IReadOnlyList<Category> _categories;

    // ADD NEW CATEGORIES HERE — above the Exit entry
    public CategoryRegistry(ExitMenuAction exitAction)
    {
        _categories = new List<Category>
        {
            new("Aspire", new StubMenuAction("Aspire")),
            new("Frontend", new StubMenuAction("Frontend")),
            new("Core", new StubMenuAction("Core")),
            new("Root", new StubMenuAction("Root")),
            new("Exit", exitAction),
        };
    }

    public IReadOnlyList<Category> GetCategories() => _categories;
}
