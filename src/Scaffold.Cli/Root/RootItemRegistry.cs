using Scaffold.Cli.Handlers;

namespace Scaffold.Cli.Root;

public class RootItemRegistry : IRootItemRegistry
{
    private readonly IReadOnlyList<RootItem> _items;

    /// <summary>
    /// ADD NEW ROOT ITEMS HERE — each entry is a display name + one or more
    /// IScaffoldHandler implementations.
    /// </summary>
    public RootItemRegistry(ICommandRunner commandRunner, IFileSystem fileSystem, IHttpFileDownloader downloader)
    {
        _items = new List<RootItem>
        {
            new(".gitignore", new List<IScaffoldHandler>
            {
                new DotnetNewHandler("gitignore", commandRunner, fileSystem, ".gitignore"),
                new WebToFileHandler(
                    "Github/Dotnet/Core/.gitignore",
                    "https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore",
                    ".gitignore",
                    downloader,
                    fileSystem)
            }),
            new(".gitattributes", new List<IScaffoldHandler>
            {
                new DotnetNewHandler("gitattributes", commandRunner, fileSystem, ".gitattributes")
            }),
        };
    }

    public IReadOnlyList<RootItem> GetItems() => _items;
}
