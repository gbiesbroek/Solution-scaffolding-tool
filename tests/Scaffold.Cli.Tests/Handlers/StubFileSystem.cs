using Scaffold.Cli.Handlers;

namespace Scaffold.Cli.Tests.Handlers;

public class StubFileSystem : IFileSystem
{
    public bool FileExistsResult { get; set; } = false;
    public string? WrittenPath { get; private set; }
    public string? WrittenContent { get; private set; }

    public bool FileExists(string path) => FileExistsResult;
    public void WriteAllText(string path, string content)
    {
        WrittenPath = path;
        WrittenContent = content;
    }
}
