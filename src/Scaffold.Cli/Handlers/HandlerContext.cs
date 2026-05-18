namespace Scaffold.Cli.Handlers;

public class HandlerContext
{
    public string WorkingDirectory { get; }
    public bool ShouldGoBack { get; private set; }

    public HandlerContext(string workingDirectory)
    {
        WorkingDirectory = workingDirectory;
    }

    public void GoBack() => ShouldGoBack = true;
}
