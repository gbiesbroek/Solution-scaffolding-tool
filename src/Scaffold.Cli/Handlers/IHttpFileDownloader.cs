namespace Scaffold.Cli.Handlers;

public interface IHttpFileDownloader
{
    Task<string> DownloadStringAsync(string url);
}
