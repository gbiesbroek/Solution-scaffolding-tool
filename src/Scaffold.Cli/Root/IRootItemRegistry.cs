namespace Scaffold.Cli.Root;

public interface IRootItemRegistry
{
    IReadOnlyList<RootItem> GetItems();
}
