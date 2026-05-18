using Scaffold.Cli.Handlers;

namespace Scaffold.Cli.Root;

public class RootItemRegistry : IRootItemRegistry
{
    private readonly IReadOnlyList<RootItem> _items;

    /// <summary>
    /// ADD NEW ROOT ITEMS HERE — each entry is a display name + one or more
    /// IScaffoldHandler implementations.
    /// </summary>
    public RootItemRegistry(ICommandRunner commandRunner)
    {
        _items = new List<RootItem>
        {
            new(".gitignore", new List<IScaffoldHandler>
            {
                new DotnetNewHandler("gitignore", commandRunner)
            }),
            new(".gitattributes", new List<IScaffoldHandler>
            {
                new DotnetNewHandler("gitattributes", commandRunner)
            }),
        };
    }

    public IReadOnlyList<RootItem> GetItems() => _items;
}
