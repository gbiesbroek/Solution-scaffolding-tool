using Spectre.Console;

namespace Scaffold.Cli.Handlers;

public class WebToFileHandler : IScaffoldHandler
{
    private readonly string _displayName;
    private readonly string _url;
    private readonly string _outputFileName;
    private readonly IHttpFileDownloader _downloader;
    private readonly IFileSystem _fileSystem;

    public string DisplayName => _displayName;
    public string Preview => $"Downloads from: {_url} -> {_outputFileName}";

    public WebToFileHandler(
        string displayName,
        string url,
        string outputFileName,
        IHttpFileDownloader downloader,
        IFileSystem fileSystem)
    {
        _displayName = displayName;
        _url = url;
        _outputFileName = outputFileName;
        _downloader = downloader;
        _fileSystem = fileSystem;
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

        try
        {
            var content = await _downloader.DownloadStringAsync(_url);
            _fileSystem.WriteAllText(filePath, content);
            console.MarkupLine($"[green]Created {Markup.Escape(_outputFileName)}[/]");
        }
        catch (Exception ex)
        {
            console.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
        }

        context.GoBack();
    }
}
