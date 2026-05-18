namespace Scaffold.Cli.Handlers;

public interface ICommandRunner
{
    Task<CommandResult> RunAsync(string executable, IEnumerable<string> args, string workingDirectory);
}
