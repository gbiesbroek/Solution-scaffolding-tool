namespace Scaffold.Cli.Handlers;

public class RealFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
    public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
}
