# Contract: WebToFileHandler, Preview Strings, and Overwrite Flow

**Feature**: 004-web-to-file-handler-preview | **Date**: 2026-05-18
**Supersedes / extends**: `specs/003-root-submenu-handlers/contracts/scaffold-root-handler-contract.md`

---

## Updated Handler Selection Flow

After the user selects a handler, a preview confirmation step is inserted before execution:

```
? How would you like to scaffold .gitignore?
> dotnet new
  Github/Dotnet/Core/.gitignore
  <- Back
```

After selecting "dotnet new":

```
[grey]Runs: dotnet new gitignore[/]

? Proceed?
> Confirm
  <- Back
```

After selecting "Github/Dotnet/Core/.gitignore":

```
[grey]Downloads from: https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore → .gitignore[/]

? Proceed?
> Confirm
  <- Back
```

- `"<- Back"` on the confirm prompt returns to the handler selection menu; nothing executes.
- `"Confirm"` triggers the file-exists check (if applicable) then handler execution.

---

## Updated `IScaffoldHandler` Contract

```csharp
public interface IScaffoldHandler
{
    string DisplayName { get; }
    string Preview { get; }
    Task ExecuteAsync(IAnsiConsole console, HandlerContext context);
}
```

Any implementation MUST:
1. Expose a non-empty `DisplayName` (shown in the handler selection list).
2. Expose a non-empty `Preview` (shown on the confirmation screen before execution).
3. Use only the injected `IAnsiConsole` for all terminal output — no static `AnsiConsole.*` calls.
4. Call `context.GoBack()` exactly once before returning, regardless of success or failure.
5. NOT call `Environment.Exit()` directly.

---

## Updated `DotnetNewHandler` Contract

```csharp
public DotnetNewHandler(
    string templateName,
    ICommandRunner commandRunner,
    IFileSystem fileSystem,
    string outputFileName,
    IReadOnlyList<string>? extraArgs = null)
```

- `DisplayName` returns `"dotnet new"`.
- `Preview` returns `$"Runs: dotnet new {templateName}[ extraArgs...]"`.
- Before running, checks `fileSystem.FileExists(outputFileName in workingDirectory)`. If exists, shows overwrite prompt. If user declines, calls `context.GoBack()` and returns without running.
- On confirm (or file not existing), executes: `dotnet new <templateName> [extraArgs...]` in `context.WorkingDirectory`.

---

## `WebToFileHandler` Contract

```csharp
public WebToFileHandler(
    string displayName,
    string url,
    string outputFileName,
    IHttpFileDownloader downloader,
    IFileSystem fileSystem)
```

- `DisplayName` returns the value passed as `displayName`.
- `Preview` returns `$"Downloads from: {url} → {outputFileName}"`.
- Before downloading, checks `fileSystem.FileExists(outputFileName in workingDirectory)`. If exists, shows overwrite prompt. If user declines, calls `context.GoBack()` and returns.
- Downloads content via `IHttpFileDownloader.DownloadStringAsync(url)`.
- On network error or exception: displays error message, calls `context.GoBack()` and returns.
- On success: writes content via `IFileSystem.WriteAllText`, displays confirmation, calls `context.GoBack()`.

**Concrete registration for `.gitignore`**:

| Property | Value |
|---|---|
| `displayName` | `"Github/Dotnet/Core/.gitignore"` |
| `url` | `https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore` |
| `outputFileName` | `".gitignore"` |

---

## `IFileSystem` Contract

```csharp
public interface IFileSystem
{
    bool FileExists(string path);
    void WriteAllText(string path, string content);
}
```

Any implementation MUST:
1. `FileExists` returns `false` (not throw) when path is inaccessible.
2. `WriteAllText` overwrites existing content silently — the caller is responsible for the overwrite prompt.

---

## `IHttpFileDownloader` Contract

```csharp
public interface IHttpFileDownloader
{
    Task<string> DownloadStringAsync(string url);
}
```

Any implementation MUST:
1. Throw on non-2xx HTTP status codes.
2. Enforce a 10-second timeout.
3. Throw `InvalidOperationException` if downloaded content is null or whitespace.
4. Not catch `TaskCanceledException` — propagate it to the caller.

---

## Overwrite Prompt Contract

When any handler detects an existing output file:

```
[yellow]File '.gitignore' already exists.[/]

? How would you like to proceed?
> Overwrite
  <- Back
```

- `"Overwrite"` → handler proceeds with its action.
- `"<- Back"` → `context.GoBack()` is called; handler returns immediately; user returns to Root item menu.

---

## Testability Contract (updated)

All previous rules from feature 003 apply, plus:

- `DotnetNewHandler` MUST accept `IFileSystem` via constructor.
- `WebToFileHandler` MUST accept `IHttpFileDownloader` and `IFileSystem` via constructor.
- Tests MUST use `StubFileSystem` and `StubHttpFileDownloader` — no real file I/O or HTTP in unit tests.
- Tests for overwrite flow MUST set `StubFileSystem.FileExistsResult = true` and simulate keyboard selection via `TestConsole.Interactive()`.
