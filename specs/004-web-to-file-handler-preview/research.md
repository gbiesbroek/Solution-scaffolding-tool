# Research: WebToFileHandler and Handler Preview Strings

**Feature**: 004-web-to-file-handler-preview | **Date**: 2026-05-18

---

## Decision 1: HTTP Abstraction for `WebToFileHandler`

**Decision**: Introduce a thin `IHttpFileDownloader` interface with a single `DownloadStringAsync(url)` method. `HttpFileDownloader` wraps a real `HttpClient`. `WebToFileHandler` depends only on `IHttpFileDownloader`.

**Rationale**: Unit-testing `WebToFileHandler` requires substituting the HTTP call without running real network I/O. `HttpClient` is not easily mockable directly (no interface). A minimal `IHttpFileDownloader` abstraction is the simplest testable seam that satisfies the spec without pulling in third-party mock libraries (which would violate the "no new packages" constraint).

**Alternatives considered**:
- `IHttpClientFactory` — unnecessary overhead for a single-use CLI tool; adds DI complexity not justified for one download scenario.
- Direct `HttpClient` in `WebToFileHandler` — untestable without real network; violates TDD principle.
- `MockHttp` / `RichardSzalay.MockHttp` NuGet — ruled out; "no new packages" constraint.

---

## Decision 2: HTTP Timeout Implementation

**Decision**: Set `HttpClient.Timeout = TimeSpan.FromSeconds(10)` on the `HttpClient` instance registered in DI. This applies globally to all requests made by `HttpFileDownloader`.

**Rationale**: Simplest implementation; timeout is configured once at DI registration, not per-request. `HttpClient.Timeout` throws `TaskCanceledException` (wrapping `OperationCanceledException`) when exceeded — same exception type as a user cancellation, cleanly caught for error display.

**Alternatives considered**:
- Per-request `CancellationTokenSource.CancelAfter(10s)` — more flexible but adds boilerplate to every call with no benefit for the current single-use case.
- No timeout — fails SC-003 and the spec's edge case clause.

---

## Decision 3: `IFileSystem` Abstraction Scope

**Decision**: A minimal two-method `IFileSystem` interface: `bool FileExists(string path)` and `void WriteAllText(string path, string content)`. `RealFileSystem` wraps `File.Exists` and `File.WriteAllText`. A `StubFileSystem` test helper records calls.

**Rationale**: Both `DotnetNewHandler` (needs `FileExists` for pre-run overwrite check) and `WebToFileHandler` (needs `FileExists` + `WriteAllText`) require these two methods and nothing else. Adding more methods (e.g., `ReadAllText`, `Delete`) would violate YAGNI.

**Alternatives considered**:
- Using `System.IO.Abstractions` NuGet — violates "no new packages" constraint; also over-engineered for two methods.
- No `IFileSystem` abstraction (handlers call `File.*` directly) — untestable without real disk I/O; violates TDD.

---

## Decision 4: Preview Confirmation Screen

**Decision**: After the user selects a handler from the handler list, `RootMenuAction` displays the handler's `Preview` string using `console.MarkupLine(...)` then shows a `SelectionPrompt<string>` with two choices: `"Confirm"` and `"<- Back"`. Selecting `"<- Back"` returns to the handler selection menu; selecting `"Confirm"` calls `handler.ExecuteAsync(...)`.

**Rationale**: `SelectionPrompt<string>` is consistent with the rest of the tool's UI (already used for top-level menu, Root item menu, handler list). It is testable via `TestConsole.Interactive()` with `PushKey`. `AnsiConsole.Confirm()` / `ConfirmationPrompt` uses Y/n text input which cannot be cleanly simulated with the existing keyboard-based test pattern.

**Alternatives considered**:
- `ConfirmationPrompt` (Y/n) — inconsistent UX, harder to test with existing `TestConsole` keyboard simulation.
- Single `SelectionPrompt` combining preview text in the title — workable but couples preview rendering and navigation.

---

## Decision 5: `DotnetNewHandler` Constructor Changes for Overwrite Check

**Decision**: Add two new required constructor parameters to `DotnetNewHandler`: `string outputFileName` (the file the command will produce, e.g., `".gitignore"`) and `IFileSystem fileSystem`. Before running the command, `DotnetNewHandler` calls `fileSystem.FileExists(Path.Combine(context.WorkingDirectory, outputFileName))` and if `true`, prompts for overwrite via `IAnsiConsole`. If declined, calls `context.GoBack()` and returns without running the command.

**Rationale**: The overwrite check belongs in the handler (per clarification Q2: shared `IFileSystem`), not in `RootMenuAction`. Adding `outputFileName` to the constructor keeps the handler self-contained and testable with a `StubFileSystem`. This breaks existing `DotnetNewHandler` test constructor calls — those tests will need updating (expected, tracked in tasks).

**Alternatives considered**:
- Optional `outputFileName` (nullable) with no check when null — allows skipping the feature; ruled out as spec requires the check.
- `OutputFileName` as an interface property on `IScaffoldHandler` — over-engineered; not all future handlers will produce a file.

---

## Decision 6: `IScaffoldHandler.Preview` Property

**Decision**: Add `string Preview { get; }` to `IScaffoldHandler`. This is a breaking interface change — all existing implementors (`DotnetNewHandler`) must add the property. No default implementation (this is a `class`-targeted interface in C# terms, but we keep it as a plain interface — every handler should define its own preview).

**Preview format standards**:
- `DotnetNewHandler`: `$"Runs: dotnet new {templateName}{(extraArgs.Any() ? " " + string.Join(" ", extraArgs) : "")}"`
- `WebToFileHandler`: `$"Downloads from: {url} → {outputFileName}"`

**Rationale**: Static string set at construction time; no async I/O. Consistent with spec FR-003 and FR-006 preview format requirements. Kept as interface property (not base class method) to preserve simplicity.

---

## Summary of New and Modified Types

### New types
| Type | Location | Purpose |
|------|----------|---------|
| `IFileSystem` | `src/Scaffold.Cli/Handlers/` | Abstraction for file existence check + write |
| `RealFileSystem` | `src/Scaffold.Cli/Handlers/` | Production `IFileSystem` wrapping `File.*` |
| `IHttpFileDownloader` | `src/Scaffold.Cli/Handlers/` | Abstraction for HTTP string download |
| `HttpFileDownloader` | `src/Scaffold.Cli/Handlers/` | Production wrapper using `HttpClient` (10s timeout) |
| `WebToFileHandler` | `src/Scaffold.Cli/Handlers/` | Downloads URL → saves to file; checks overwrite |
| `StubFileSystem` | `tests/Scaffold.Cli.Tests/Handlers/` | Test helper; configurable `FileExistsResult` |
| `StubHttpFileDownloader` | `tests/Scaffold.Cli.Tests/Handlers/` | Test helper; configurable content/error |

### Modified types
| Type | Change |
|------|--------|
| `IScaffoldHandler` | Add `string Preview { get; }` |
| `DotnetNewHandler` | Add `outputFileName` + `IFileSystem` params; add overwrite check; implement `Preview` |
| `RootMenuAction` | Add preview confirmation step between handler selection and `ExecuteAsync` |
| `RootItemRegistry` | Add `WebToFileHandler` for `.gitignore`; pass `outputFileName` + `IFileSystem` to `DotnetNewHandler` |
| `ServiceCollectionExtensions` | Register `IFileSystem`, `HttpClient`, `IHttpFileDownloader` |
