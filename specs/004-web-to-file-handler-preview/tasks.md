# Tasks: WebToFileHandler and Handler Preview Strings

**Input**: Design documents from `specs/004-web-to-file-handler-preview/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Tool source: `src/Scaffold.Cli/`
- Unit tests: `tests/Scaffold.Cli.Tests/`
- E2E tests: `tests/Scaffold.Cli.EndToEnd/`
- Solution: `SolutionScaffoldingTool.sln`

---

## Phase 1: Setup

**Purpose**: Confirm 37/37 baseline before any feature 004 changes.

- [X] T001 Run `dotnet build && dotnet test` — confirm all 37 existing tests pass as baseline before feature 004 work begins

**Checkpoint**: 0 failures. 37 existing tests pass. ✓

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: New interfaces and test helpers required by all three user stories. MUST be complete before any user story work.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T002 [P] Create `src/Scaffold.Cli/Handlers/IFileSystem.cs` — interface with two methods: `bool FileExists(string path)` and `void WriteAllText(string path, string content)`; namespace `Scaffold.Cli.Handlers`
- [X] T003 [P] Create `src/Scaffold.Cli/Handlers/RealFileSystem.cs` — implements `IFileSystem`; `FileExists` wraps `File.Exists(path)`; `WriteAllText` wraps `File.WriteAllText(path, content)`; namespace `Scaffold.Cli.Handlers`
- [X] T004 [P] Create `src/Scaffold.Cli/Handlers/IHttpFileDownloader.cs` — interface with one method: `Task<string> DownloadStringAsync(string url)`; must throw on non-2xx responses and empty content; namespace `Scaffold.Cli.Handlers`
- [X] T005 [P] Create `src/Scaffold.Cli/Handlers/HttpFileDownloader.cs` — implements `IHttpFileDownloader`; constructor `(HttpClient httpClient)`; `DownloadStringAsync`: calls `_httpClient.GetAsync(url)`, calls `response.EnsureSuccessStatusCode()`, reads content with `ReadAsStringAsync()`, throws `InvalidOperationException("Downloaded content is empty.")` if null/whitespace; namespace `Scaffold.Cli.Handlers`
- [X] T006 [P] Create `tests/Scaffold.Cli.Tests/Handlers/StubFileSystem.cs` — test helper implementing `IFileSystem`; `bool FileExistsResult { get; set; } = false`; `string? WrittenPath { get; private set; }`; `string? WrittenContent { get; private set; }`; `FileExists(path)` returns `FileExistsResult`; `WriteAllText` records path and content; namespace `Scaffold.Cli.Tests.Handlers`
- [X] T007 [P] Create `tests/Scaffold.Cli.Tests/Handlers/StubHttpFileDownloader.cs` — test helper implementing `IHttpFileDownloader`; two constructors: `(string content)` returns it; `(Exception exception)` throws it; records `string? LastUrl { get; private set; }`; namespace `Scaffold.Cli.Tests.Handlers`

**Checkpoint**: `dotnet build` succeeds. All 37 tests still pass. ✓

---

## Phase 3: US2 + US3 — Preview Confirmation + Overwrite Protection (Priority: P1) 🎯 MVP-A

**Goal**: Every handler shows a preview confirmation screen after selection. Any handler that writes a file checks for existence first and prompts the user before overwriting. `DotnetNewHandler` fully updated with `Preview` property and overwrite check.

**Independent Test**: Selecting a handler in the handler list now shows a confirm/back step. With `.gitignore` already present in the working directory, selecting either handler for `.gitignore` shows the overwrite prompt. Selecting `<- Back` at the overwrite prompt returns to the Root submenu without writing anything.

### TDD Gate — Write Failing Tests First ⚠️

> **NOTE: Write T008 and T009 FIRST, ensure they FAIL before starting T010+**

- [X] T008 [US2] [US3] Extend `tests/Scaffold.Cli.Tests/Handlers/DotnetNewHandlerTests.cs` with new overwrite and preview tests (append to existing file — keep passing tests intact):
  - `Preview_MatchesTemplateFormat`: create `new DotnetNewHandler("gitignore", stub, fs, ".gitignore")`, assert `Preview == "Runs: dotnet new gitignore"`
  - `Preview_WithExtraArgs_IncludesArgs`: create handler with `extraArgs: new[]{"--force"}`, assert `Preview == "Runs: dotnet new gitignore --force"`
  - `ExecuteAsync_FileNotExists_RunsCommandDirectly`: `StubFileSystem.FileExistsResult = false`, execute, assert command was run (stub records call)
  - `ExecuteAsync_FileExists_UserDeclinesOverwrite_CommandNotRun`: `FileExistsResult = true`, push `{DownArrow}{Enter}` (select `<- Back`), assert `LastExecutable == null` (command not called)
  - `ExecuteAsync_FileExists_UserConfirmsOverwrite_CommandRuns`: `FileExistsResult = true`, push `{Enter}` (select "Overwrite"), assert `LastExecutable == "dotnet"`
  - Run `dotnet test tests/Scaffold.Cli.Tests/Handlers/DotnetNewHandlerTests.cs` → MUST **FAIL** (constructor doesn't accept `IFileSystem`/`outputFileName` yet; `Preview` not yet on `IScaffoldHandler`)

- [X] T009 [US2] Add new failing tests to `tests/Scaffold.Cli.Tests/Root/RootMenuActionTests.cs` (append — keep passing tests intact):
  - `ExecuteAsync_HandlerSelected_ShowsPreviewConfirmBeforeExecuting`: select `.gitignore` (Enter), select handler (Enter), then press `{DownArrow}{Enter}` for "<- Back" on confirm → assert handler was NOT called (go back from confirm)
  - `ExecuteAsync_HandlerSelected_ConfirmsAndExecutes`: select `.gitignore` (Enter), select handler (Enter), press `{Enter}` on "Confirm" → assert handler WAS called; then push `{DownArrow}{Enter}` to exit root loop
  - Run `dotnet test tests/Scaffold.Cli.Tests/Root/RootMenuActionTests.cs` → MUST **FAIL** (confirm step not yet in `RootMenuAction`)

> **GATE**: T008 and T009 FAIL; T002–T007 compile; existing RootMenuActionTests and DotnetNewHandlerTests still PASS. Do not proceed until confirmed.

### Implementation for US2 + US3

- [X] T010 [US2] [US3] Update `src/Scaffold.Cli/Handlers/IScaffoldHandler.cs` — add `string Preview { get; }` property to the interface (this is a breaking change; `DotnetNewHandler` will not compile until T011 is complete — that's expected)

- [X] T011 [US2] [US3] Update `src/Scaffold.Cli/Handlers/DotnetNewHandler.cs` — add `IFileSystem fileSystem` and `string outputFileName` as new required constructor parameters (after existing `commandRunner`); implement `string Preview => $"Runs: dotnet new {_templateName}{(_extraArgs.Any() ? " " + string.Join(" ", _extraArgs) : "")}"`; add overwrite check at start of `ExecuteAsync`:
  ```
  var filePath = Path.Combine(context.WorkingDirectory, _outputFileName);
  if (_fileSystem.FileExists(filePath))
  {
      console.MarkupLine($"[yellow]File '{_outputFileName}' already exists.[/]");
      var choice = console.Prompt(new SelectionPrompt<string>().Title("How would you like to proceed?").AddChoices("Overwrite", "<- Back"));
      if (choice == "<- Back") { context.GoBack(); return; }
  }
  ```
  Then run existing command; also update ALL constructor calls in `tests/Scaffold.Cli.Tests/Handlers/DotnetNewHandlerTests.cs` to pass `new StubFileSystem()` and `".gitignore"` (or `".gitattributes"` as applicable); run `dotnet test tests/Scaffold.Cli.Tests/Handlers/DotnetNewHandlerTests.cs` → T008 MUST **PASS**

- [X] T012 [US2] Update `src/Scaffold.Cli/Root/RootMenuAction.cs` — in the inner handler selection loop, after the user picks a handler and before calling `handler.ExecuteAsync`, insert a preview confirmation step:
  ```
  console.MarkupLine($"[grey]{Markup.Escape(selectedHandler.Preview)}[/]");
  var confirm = console.Prompt(new SelectionPrompt<string>().Title("Proceed?").WrapAround().AddChoices("Confirm", "<- Back"));
  if (confirm == "<- Back") continue; // back to handler selection
  ```
  Update ALL existing `RootMenuActionTests.cs` keyboard sequences to account for the extra confirm prompt (existing passing tests now need an additional `{Enter}` for "Confirm" when they expect execution); run `dotnet test tests/Scaffold.Cli.Tests/Root/RootMenuActionTests.cs` → T009 MUST **PASS** and all previously passing tests MUST still pass

- [X] T013 Run `dotnet test tests/Scaffold.Cli.Tests/` → all unit tests pass; `dotnet build` → 0 errors

**Checkpoint**: Preview confirm step works. Overwrite prompt fires when file exists. `<- Back` at overwrite prompt returns to Root submenu. All 37+ unit tests pass. ✓

---

## Phase 4: US1 — WebToFileHandler (Priority: P1) 🎯 MVP-B

**Goal**: Selecting Root → `.gitignore` → "Github/Dotnet/Core/.gitignore" downloads the file from GitHub and saves it to `.gitignore` in the working directory. Full overwrite protection applies.

**Independent Test**: Can be tested in isolation by running `WebToFileHandlerTests` with `StubHttpFileDownloader` + `StubFileSystem`. No real HTTP or disk access required. E2E validated by `dotnet test` + manual `scaffold` run.

### TDD Gate — Write Failing Tests First ⚠️

> **NOTE: Write T014 FIRST, ensure it FAILS before implementing T015**

- [X] T014 [US1] Write `tests/Scaffold.Cli.Tests/Handlers/WebToFileHandlerTests.cs`:
  - `DisplayName_ReturnsConfiguredDisplayName`: assert `DisplayName == "Github/Dotnet/Core/.gitignore"` for the standard registration
  - `Preview_Format`: assert `Preview == "Downloads from: https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore → .gitignore"`
  - `ExecuteAsync_FileNotExists_DownloadsAndWritesFile`: `FileExistsResult=false`, stub returns `"# content"`, assert `StubFileSystem.WrittenContent == "# content"` and `WrittenPath` ends with `.gitignore`
  - `ExecuteAsync_DownloadSuccess_ShowsConfirmation`: assert `console.Output` contains `"Created .gitignore"` (or similar)
  - `ExecuteAsync_DownloadSuccess_CallsGoBack`: assert `context.ShouldGoBack == true`
  - `ExecuteAsync_FileExists_UserDeclinesOverwrite_NoDownload`: `FileExistsResult=true`, push `{DownArrow}{Enter}` ("<- Back"), assert `StubHttpFileDownloader.LastUrl == null` (download not called) and `WrittenContent == null`
  - `ExecuteAsync_FileExists_UserConfirmsOverwrite_WritesFile`: `FileExistsResult=true`, push `{Enter}` ("Overwrite"), assert file was written
  - `ExecuteAsync_NetworkError_DisplaysError`: stub throws `HttpRequestException("timeout")`, assert `console.Output` contains error text; assert `context.ShouldGoBack == true`
  - `ExecuteAsync_EmptyContent_DisplaysError`: stub throws `InvalidOperationException("Downloaded content is empty.")`, assert error displayed; `ShouldGoBack == true`
  - Run `dotnet test tests/Scaffold.Cli.Tests/Handlers/WebToFileHandlerTests.cs` → MUST **FAIL** (`WebToFileHandler` not yet created)

### Implementation for US1

- [X] T015 [US1] Create `src/Scaffold.Cli/Handlers/WebToFileHandler.cs` — implements `IScaffoldHandler`; constructor `(string displayName, string url, string outputFileName, IHttpFileDownloader downloader, IFileSystem fileSystem)`; `DisplayName` returns `displayName`; `Preview` returns `$"Downloads from: {_url} → {_outputFileName}"`; `ExecuteAsync`:
  1. File-exists check + overwrite prompt (same SelectionPrompt pattern as `DotnetNewHandler`)
  2. `try { var content = await _downloader.DownloadStringAsync(_url); }` — catch any exception, display via `console.MarkupLine("[red]{Markup.Escape(ex.Message)}[/]")`, `context.GoBack(); return`
  3. `_fileSystem.WriteAllText(filePath, content)`
  4. `console.MarkupLine($"[green]Created {_outputFileName}[/]")`
  5. `context.GoBack()`
  Run `dotnet test tests/Scaffold.Cli.Tests/Handlers/WebToFileHandlerTests.cs` → T014 MUST **PASS**

- [X] T016 [US1] Update `src/Scaffold.Cli/Root/RootItemRegistry.cs` — add `IHttpFileDownloader downloader` and `IFileSystem fileSystem` constructor parameters; add `outputFileName` to existing `DotnetNewHandler` instances (`.gitignore` → `".gitignore"`, `.gitattributes` → `".gitattributes"`); add `WebToFileHandler` as a second handler for `.gitignore`:
  ```csharp
  new WebToFileHandler(
      "Github/Dotnet/Core/.gitignore",
      "https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore",
      ".gitignore",
      downloader,
      fileSystem)
  ```

- [X] T017 [US1] Update `tests/Scaffold.Cli.Tests/Root/RootItemRegistryTests.cs` — update `CreateRegistry()` to pass `new StubCommandRunner(0,"","")`, `new StubFileSystem()`, `new StubHttpFileDownloader("content")`; add test `GetItems_GitignoreHasTwoHandlers`: asserts `.gitignore` item has 2 handlers; add test `GetItems_GitignoreSecondHandler_IsWebToFileHandler`: asserts `Handlers[1]` is `WebToFileHandler`; run `dotnet test` → PASS

- [X] T018 [US1] Update `tests/Scaffold.Cli.Tests/Root/RootItemExtensibilityTests.cs` — update `RootItemRegistry` constructor call to pass three stubs (same as T017); run `dotnet test` → PASS

- [X] T019 [US1] Update `src/Scaffold.Cli/Infrastructure/ServiceCollectionExtensions.cs` — add: `services.AddSingleton<IFileSystem, RealFileSystem>()`; add: `services.AddSingleton(_ => new HttpClient { Timeout = TimeSpan.FromSeconds(10) })`; add: `services.AddSingleton<IHttpFileDownloader, HttpFileDownloader>()`; update `RootItemRegistry` factory to resolve and pass `IFileSystem` and `IHttpFileDownloader` from service provider

- [X] T020 Run `dotnet test tests/Scaffold.Cli.Tests/` → all unit tests pass; `dotnet build` → 0 errors

**Checkpoint**: `WebToFileHandler` downloads and writes file. Two handlers for `.gitignore` in selection menu. Overwrite prompt fires for both handler types. All tests pass. ✓

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, packaging, documentation accuracy.

- [X] T021 [P] Run full `dotnet test` from repository root across all three test projects: `dotnet test`; confirm 0 failures
- [X] T022 [P] Build and pack to local NuGet feed: `dotnet pack src/Scaffold.Cli/ -o "$HOME\nuget-local" --configuration Release`; uninstall and reinstall: `dotnet tool uninstall --global Scaffold.Cli` then `dotnet tool install --global Scaffold.Cli --add-source "$HOME\nuget-local"`
- [X] T023 [P] Validate `specs/004-web-to-file-handler-preview/quickstart.md` — walk through all steps; update any paths, command outputs, or menu text that differ from the final implementation
- [X] T024 Manual validation: launch `scaffold` from an empty temp directory; navigate Root → `.gitignore` → confirm two handlers shown ("dotnet new", "Github/Dotnet/Core/.gitignore"); select each in turn; verify preview shown before each action; verify files created; re-run a handler when file exists — verify overwrite prompt fires; verify `<- Back` at all levels works; verify Exit closes cleanly (exit code 0)
- [X] T025 [P] Update `specs/004-web-to-file-handler-preview/checklists/requirements.md` — mark all items complete; add note confirming TDD gates (T008, T009, T014) were enforced

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Requires Phase 1 — **BLOCKS all user stories**
- **US2+US3 (Phase 3)**: Requires Phase 2 complete — first MVP gate
- **US1 (Phase 4)**: Requires Phase 3 complete (needs updated `IScaffoldHandler.Preview`, `IFileSystem`, overwrite pattern established in `DotnetNewHandler`)
- **Polish (Phase 5)**: Requires Phase 4 complete

### User Story Dependencies

- **US2+US3 (P1)**: Combined in Phase 3 — tightly coupled (both modify `DotnetNewHandler` + `RootMenuAction`); deliver together as first working slice
- **US1 (P1)**: Depends on Phase 3 (`IScaffoldHandler.Preview` must exist; `IFileSystem` pattern must be established)

### Within Phase 3

- T008 + T009 must FAIL before T010 begins
- T010 (`IScaffoldHandler.Preview`) must precede T011 (`DotnetNewHandler`) and T012 (`RootMenuAction`)
- T011 and T012 can run in parallel (different files)
- T013 (full test run) follows T011 + T012

### Within Phase 4

- T014 (failing tests) must FAIL before T015
- T015 → T016 → T017 → T018 → T019 (sequential; each builds on the previous)

### Breaking Changes to Fix

When T010 adds `Preview` to `IScaffoldHandler` and T011 changes `DotnetNewHandler`'s constructor, the following existing tests MUST be updated in the same task:
- `DotnetNewHandlerTests.cs` — all constructor calls (in T011)
- When T016 changes `RootItemRegistry`'s constructor, fix: `RootItemRegistryTests.cs` (T017) and `RootItemExtensibilityTests.cs` (T018)
- When T012 adds confirm step to `RootMenuAction`, fix all existing `RootMenuActionTests.cs` keyboard sequences (in T012)

### Parallel Opportunities

- T002–T007 (foundational types + test helpers): all parallel — different files
- T008 + T009 (write failing tests): parallel — different files
- T011 + T012 (DotnetNewHandler + RootMenuAction): parallel after T010
- T021, T022, T023, T025 (Polish): all parallel

---

## Parallel Example: Phase 3 TDD Gate

```
# Write failing tests in parallel:
T008: tests/Scaffold.Cli.Tests/Handlers/DotnetNewHandlerTests.cs  (FAIL)
T009: tests/Scaffold.Cli.Tests/Root/RootMenuActionTests.cs        (FAIL)

# After GATE confirmed — implement in parallel:
T011: src/Scaffold.Cli/Handlers/DotnetNewHandler.cs               (parallel with T012)
T012: src/Scaffold.Cli/Root/RootMenuAction.cs                     (parallel with T011)
# T013 (test run) follows T011 + T012
```

---

## Implementation Strategy

### MVP First (US2 + US3 — Phase 3 Only)

1. Complete Phase 1: Baseline verify
2. Complete Phase 2: New interfaces + test helpers
3. Write T008 + T009 → confirm FAIL
4. Complete Phase 3: Update `DotnetNewHandler` + `RootMenuAction`
5. **STOP and VALIDATE**: Preview confirm + overwrite works for existing handlers

### Incremental Delivery

1. Phase 1 + 2 → Foundation
2. Phase 3 (US2+US3) → Preview confirm + overwrite for existing handlers ← **MVP-A demo point**
3. Phase 4 (US1) → `WebToFileHandler` + `.gitignore` from GitHub ← **MVP-B demo point**
4. Phase 5 → Polish + pack + install

---

## Notes

- [P] = different files, no dependencies within same phase
- TDD gates are NON-NEGOTIABLE (Constitution Principle II) — T008, T009, T014 MUST fail before their respective implementation tasks
- T010 (`IScaffoldHandler.Preview`) is a breaking interface change — project will NOT compile until T011 is complete; this is expected and accepted
- `HttpFileDownloader` is not unit-tested directly (trivial wrapper); its behaviour is validated via `WebToFileHandlerTests` with `StubHttpFileDownloader`
- `RealFileSystem` is not unit-tested directly (trivial wrapper); validated via E2E
- `HttpClient.Timeout = TimeSpan.FromSeconds(10)` set at DI registration in T019 — not per-request
- The overwrite `SelectionPrompt` sentinel `"<- Back"` does not collide with file names (file names in scope all start with `.`)
- `Markup.Escape()` MUST be used on any user-facing/external string passed to `MarkupLine` to prevent injection from error messages or file content
