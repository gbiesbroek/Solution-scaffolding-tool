namespace Scaffold.Cli.Handlers;

public interface IFileSystem
{
    bool FileExists(string path);
    void WriteAllText(string path, string content);
}
