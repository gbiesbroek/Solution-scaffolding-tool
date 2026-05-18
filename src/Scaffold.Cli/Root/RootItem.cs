using Scaffold.Cli.Handlers;

namespace Scaffold.Cli.Root;

public sealed class RootItem
{
    public string DisplayName { get; }
    public IReadOnlyList<IScaffoldHandler> Handlers { get; }

    public RootItem(string displayName, IReadOnlyList<IScaffoldHandler> handlers)
    {
        DisplayName = displayName;
        Handlers = handlers;
    }
}
