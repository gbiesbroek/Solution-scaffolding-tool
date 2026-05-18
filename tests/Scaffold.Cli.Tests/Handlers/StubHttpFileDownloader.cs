using Scaffold.Cli.Handlers;

namespace Scaffold.Cli.Tests.Handlers;

public class StubHttpFileDownloader : IHttpFileDownloader
{
    private readonly string? _content;
    private readonly Exception? _exception;
    public string? LastUrl { get; private set; }

    public StubHttpFileDownloader(string content) => _content = content;
    public StubHttpFileDownloader(Exception exception) => _exception = exception;

    public Task<string> DownloadStringAsync(string url)
    {
        LastUrl = url;
        if (_exception is not null) throw _exception;
        return Task.FromResult(_content!);
    }
}
