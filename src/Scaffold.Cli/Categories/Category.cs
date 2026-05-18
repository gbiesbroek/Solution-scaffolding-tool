using Scaffold.Cli.Actions;

namespace Scaffold.Cli.Categories;

public sealed class Category
{
    public string DisplayName { get; }
    public IMenuAction Action { get; }
    public string? Description { get; init; }

    public Category(string displayName, IMenuAction action)
    {
        DisplayName = displayName;
        Action = action;
    }
}
