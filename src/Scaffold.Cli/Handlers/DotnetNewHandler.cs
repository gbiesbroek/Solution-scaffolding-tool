using Spectre.Console;

namespace Scaffold.Cli.Handlers;

public class DotnetNewHandler : IScaffoldHandler
{
    private readonly string _templateName;
    private readonly ICommandRunner _commandRunner;
    private readonly IReadOnlyList<string> _extraArgs;

    public string DisplayName => "dotnet new";

    public DotnetNewHandler(string templateName, ICommandRunner commandRunner, IReadOnlyList<string>? extraArgs = null)
    {
        _templateName = templateName;
        _commandRunner = commandRunner;
        _extraArgs = extraArgs ?? Array.Empty<string>();
    }

    public async Task ExecuteAsync(IAnsiConsole console, HandlerContext context)
    {
        var args = new[] { "new", _templateName }.Concat(_extraArgs);
        var result = await _commandRunner.RunAsync("dotnet", args, context.WorkingDirectory);

        if (!string.IsNullOrWhiteSpace(result.Output))
            console.WriteLine(result.Output);

        if (!string.IsNullOrWhiteSpace(result.Error))
            console.MarkupLine($"[red]{Markup.Escape(result.Error)}[/]");

        context.GoBack();
    }
}
