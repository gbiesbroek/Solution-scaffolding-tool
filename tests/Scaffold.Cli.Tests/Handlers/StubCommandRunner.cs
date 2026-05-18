using Scaffold.Cli.Handlers;

namespace Scaffold.Cli.Tests.Handlers;

public class StubCommandRunner : ICommandRunner
{
    private readonly int _exitCode;
    private readonly string _output;
    private readonly string _error;

    public string? LastExecutable { get; private set; }
    public IEnumerable<string>? LastArgs { get; private set; }
    public string? LastWorkingDirectory { get; private set; }

    public StubCommandRunner(int exitCode, string output, string error)
    {
        _exitCode = exitCode;
        _output = output;
        _error = error;
    }

    public Task<CommandResult> RunAsync(string executable, IEnumerable<string> args, string workingDirectory)
    {
        LastExecutable = executable;
        LastArgs = args.ToList();
        LastWorkingDirectory = workingDirectory;
        return Task.FromResult(new CommandResult(_exitCode, _output, _error));
    }
}
