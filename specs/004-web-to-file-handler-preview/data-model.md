# Data Model: WebToFileHandler and Handler Preview Strings

**Feature**: 004-web-to-file-handler-preview | **Date**: 2026-05-18

---

## Entities

### `IFileSystem` *(new)*

Minimal abstraction over file-system side effects. Injected into any handler that reads or writes files.

```csharp
public interface IFileSystem
{
    bool FileExists(string path);
    void WriteAllText(string path, string content);
}
```

**Rules**:
- `FileExists` must not throw; returns `false` when path does not exist or is inaccessible.
- `WriteAllText` overwrites an existing file if present (caller is responsible for the overwrite prompt).

---

### `RealFileSystem` *(new)*

Production implementation of `IFileSystem`.

```csharp
public class RealFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
    public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
}
```

---

### `IHttpFileDownloader` *(new)*

Thin abstraction over HTTP GET for text content. Enables unit-testing `WebToFileHandler` without real network I/O.

```csharp
public interface IHttpFileDownloader
{
    Task<string> DownloadStringAsync(string url);
}
```

**Rules**:
- Must throw on non-2xx HTTP responses (`EnsureSuccessStatusCode`).
- Must enforce 10-second timeout (configured on the underlying `HttpClient`).
- Must throw on empty response content (validated after download).

---

### `HttpFileDownloader` *(new)*

Production implementation of `IHttpFileDownloader`. Wraps a singleton `HttpClient` with a 10-second timeout.

```csharp
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
```

**DI registration**:
```csharp
services.AddSingleton(_ => new HttpClient { Timeout = TimeSpan.FromSeconds(10) });
services.AddSingleton<IHttpFileDownloader, HttpFileDownloader>();
```

---

### `IScaffoldHandler` *(modified — adds `Preview`)*

Updated interface. All implementors must provide a `Preview` property.

```csharp
public interface IScaffoldHandler
{
    string DisplayName { get; }
    string Preview { get; }                                    // NEW
    Task ExecuteAsync(IAnsiConsole console, HandlerContext context);
}
```

---

### `DotnetNewHandler` *(modified — adds `OutputFileName`, `IFileSystem`, `Preview`, overwrite check)*

Constructor gains two new required parameters: `outputFileName` and `IFileSystem`.

```csharp
public class DotnetNewHandler : IScaffoldHandler
{
    public string DisplayName => "dotnet new";
    public string Preview => $"Runs: dotnet new {_templateName}{(_extraArgs.Any() ? " " + string.Join(" ", _extraArgs) : "")}";

    // Constructor:
    public DotnetNewHandler(
        string templateName,
        ICommandRunner commandRunner,
        IFileSystem fileSystem,
        string outputFileName,
        IReadOnlyList<string>? extraArgs = null)
}
```

**Overwrite check flow** (inside `ExecuteAsync`):
1. `var filePath = Path.Combine(context.WorkingDirectory, _outputFileName)`
2. If `_fileSystem.FileExists(filePath)`:
   - `console.MarkupLine($"[yellow]File '{_outputFileName}' already exists.[/]")`
   - Show `SelectionPrompt<string>` with `"Overwrite"` / `"<- Back"`
   - If `"<- Back"` → `context.GoBack()` and return
3. Execute command; display output/error; `context.GoBack()`

---

### `WebToFileHandler` *(new)*

Downloads a text file from a URL and saves it to the working directory.

```csharp
public class WebToFileHandler : IScaffoldHandler
{
    public string DisplayName { get; }
    public string Preview => $"Downloads from: {_url} → {_outputFileName}";

    public WebToFileHandler(
        string displayName,
        string url,
        string outputFileName,
        IHttpFileDownloader downloader,
        IFileSystem fileSystem)
}
```

**`ExecuteAsync` flow**:
1. `var filePath = Path.Combine(context.WorkingDirectory, _outputFileName)`
2. If `_fileSystem.FileExists(filePath)`:
   - Notify user; show `"Overwrite"` / `"<- Back"` prompt
   - If `"<- Back"` → `context.GoBack()` and return
3. `try { var content = await _downloader.DownloadStringAsync(_url); }`
   - On exception: display error message; `context.GoBack()` and return
4. `_fileSystem.WriteAllText(filePath, content)`
5. `console.MarkupLine($"[green]Created {_outputFileName}[/]")`
6. `context.GoBack()`

---

### `RootMenuAction` *(modified — adds preview confirmation step)*

After the user selects a handler from the handler selection list, a new confirmation step is inserted:

```
// NEW: show preview + confirm/back before executing
console.MarkupLine($"[grey]{Markup.Escape(selectedHandler.Preview)}[/]");
var confirm = console.Prompt(
    new SelectionPrompt<string>()
        .Title("Proceed?")
        .AddChoices("Confirm", "<- Back"));

if (confirm == "<- Back")
    continue; // return to handler selection (inner loop)

// Execute
var ctx = new HandlerContext(Directory.GetCurrentDirectory());
await selectedHandler.ExecuteAsync(console, ctx);
```

**Updated navigation state machine**:
```
Root menu loop (outer):
  → Show Root item list + "<- Back"
  → If "<- Back": navContext.GoBack(); return

  Handler loop (inner):
    → Show handler list + "<- Back"
    → If "<- Back": break inner loop (back to Root item list)

    Confirm step:
      → Show Preview + "Confirm" / "<- Back"
      → If "<- Back": continue inner loop (re-show handler list)
      → If "Confirm": ExecuteAsync → continue inner loop after return
```

---

### `RootItemRegistry` *(modified — adds WebToFileHandler, adds outputFileName to DotnetNewHandlers)*

```csharp
public class RootItemRegistry : IRootItemRegistry
{
    public RootItemRegistry(ICommandRunner commandRunner, IFileSystem fileSystem, IHttpFileDownloader downloader)
    {
        _items = new List<RootItem>
        {
            new(".gitignore", new List<IScaffoldHandler>
            {
                new DotnetNewHandler("gitignore", commandRunner, fileSystem, ".gitignore"),
                new WebToFileHandler(
                    "Github/Dotnet/Core/.gitignore",
                    "https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore",
                    ".gitignore",
                    downloader,
                    fileSystem)
            }),
            new(".gitattributes", new List<IScaffoldHandler>
            {
                new DotnetNewHandler("gitattributes", commandRunner, fileSystem, ".gitattributes")
            }),
        };
    }
}
```

---

## State Machine: Handler Execution with Preview + Overwrite

```
[Root Item Selected]
       │
       ▼
[Handler Selection Menu]
   ├── "<- Back" ──────────────────────────────────► [Root Item Menu]
   └── <handler selected>
              │
              ▼
       [Preview + Confirm Menu]
          ├── "<- Back" ──────────────────────────► [Handler Selection Menu]
          └── "Confirm"
                   │
                   ▼
            [FileExists check]
               ├── No ──────────────────────────────► [Execute Action]
               └── Yes
                     │
                     ▼
              [Overwrite Prompt]
                 ├── "<- Back" ─────────────────────► [Handler Selection Menu]
                 └── "Overwrite"
                          │
                          ▼
                   [Execute Action]
                          │
                          ▼
                   [Root Item Menu]
```

---

## Test Helpers *(new, in test project)*

### `StubFileSystem`

```csharp
public class StubFileSystem : IFileSystem
{
    public bool FileExistsResult { get; set; } = false;
    public string? WrittenPath { get; private set; }
    public string? WrittenContent { get; private set; }

    public bool FileExists(string path) => FileExistsResult;
    public void WriteAllText(string path, string content)
    {
        WrittenPath = path;
        WrittenContent = content;
    }
}
```

### `StubHttpFileDownloader`

```csharp
public class StubHttpFileDownloader : IHttpFileDownloader
{
    private readonly string? _content;
    private readonly Exception? _exception;

    public StubHttpFileDownloader(string content) => _content = content;
    public StubHttpFileDownloader(Exception exception) => _exception = exception;

    public Task<string> DownloadStringAsync(string url)
    {
        if (_exception is not null) throw _exception;
        return Task.FromResult(_content!);
    }
}
```
