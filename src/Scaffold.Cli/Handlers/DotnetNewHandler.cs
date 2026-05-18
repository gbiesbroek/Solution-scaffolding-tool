using Spectre.Console;

namespace Scaffold.Cli.Handlers;

public class DotnetNewHandler : IScaffoldHandler
{
    private readonly string _templateName;
    private readonly ICommandRunner _commandRunner;
    private readonly IFileSystem _fileSystem;
    private readonly string _outputFileName;
    private readonly IReadOnlyList<string> _extraArgs;

    public string DisplayName => "dotnet new";
    public string Preview => $"Runs: dotnet new {_templateName}{(_extraArgs.Any() ? " " + string.Join(" ", _extraArgs) : "")}";

    public DotnetNewHandler(
        string templateName,
        ICommandRunner commandRunner,
        IFileSystem fileSystem,
        string outputFileName,
        IReadOnlyList<string>? extraArgs = null)
    {
        _templateName = templateName;
        _commandRunner = commandRunner;
        _fileSystem = fileSystem;
        _outputFileName = outputFileName;
        _extraArgs = extraArgs ?? Array.Empty<string>();
    }

    public async Task ExecuteAsync(IAnsiConsole console, HandlerContext context)
    {
        var filePath = Path.Combine(context.WorkingDirectory, _outputFileName);
        if (_fileSystem.FileExists(filePath))
        {
            console.MarkupLine($"[yellow]File '{Markup.Escape(_outputFileName)}' already exists.[/]");
            var choice = console.Prompt(
                new SelectionPrompt<string>()
                    .Title("How would you like to proceed?")
                    .WrapAround()
                    .AddChoices("Overwrite", "<- Back"));
            if (choice == "<- Back")
            {
                context.GoBack();
                return;
            }
        }

        var args = new[] { "new", _templateName }.Concat(_extraArgs);
        var result = await _commandRunner.RunAsync("dotnet", args, context.WorkingDirectory);

        if (!string.IsNullOrWhiteSpace(result.Output))
            console.WriteLine(result.Output);

        if (!string.IsNullOrWhiteSpace(result.Error))
            console.MarkupLine($"[red]{Markup.Escape(result.Error)}[/]");

        context.GoBack();
    }
}
