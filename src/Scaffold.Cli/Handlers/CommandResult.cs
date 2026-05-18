namespace Scaffold.Cli.Handlers;

public sealed class CommandResult
{
    public int ExitCode { get; }
    public string Output { get; }
    public string Error { get; }

    public CommandResult(int exitCode, string output, string error)
    {
        ExitCode = exitCode;
        Output = output;
        Error = error;
    }
}
