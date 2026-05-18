namespace Scaffold.Cli.Handlers;

public class HttpFileDownloader : IHttpFileDownloader
{
    private readonly HttpClient _httpClient;

    public HttpFileDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> DownloadStringAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Downloaded content is empty.");
        return content;
    }
}
