# Tasks: Root Submenu with Command Handlers

**Input**: Design documents from `specs/003-root-submenu-handlers/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2)
- Include exact file paths in descriptions

## Path Conventions

- Tool source: `src/Scaffold.Cli/`
- Unit tests: `tests/Scaffold.Cli.Tests/`
- E2E tests: `tests/Scaffold.Cli.EndToEnd/`
- Solution: `SolutionScaffoldingTool.sln`

---

## Phase 1: Setup

**Purpose**: Confirm the baseline before any feature 003 changes

- [X] T001 Run `dotnet build && dotnet test` — confirm all existing tests pass as the baseline before feature 003 work begins

**Checkpoint**: 0 failures. 17 existing tests pass. ✓

---

## Phase 2: Foundational

**Purpose**: All interfaces and value objects. MUST be complete before all user story work.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T002 [P] Create `src/Scaffold.Cli/Handlers/HandlerContext.cs` — class with `string WorkingDirectory` (set at construction), `bool ShouldGoBack { get; private set; }`, `void GoBack()` (sets ShouldGoBack=true); no dependency on `NavigationContext`
- [X] T003 [P] Create `src/Scaffold.Cli/Handlers/IScaffoldHandler.cs` — interface with `string DisplayName { get; }` and `Task ExecuteAsync(IAnsiConsole console, HandlerContext context)`; add `using Spectre.Console;`
- [X] T004 [P] Create `src/Scaffold.Cli/Handlers/CommandResult.cs` — sealed class (or record) with `int ExitCode`, `string Output`, `string Error`; constructor sets all three
- [X] T005 [P] Create `src/Scaffold.Cli/Handlers/ICommandRunner.cs` — interface with `Task<CommandResult> RunAsync(string executable, IEnumerable<string> args, string workingDirectory)`
- [X] T006 [P] Create `src/Scaffold.Cli/Root/RootItem.cs` — sealed class with `string DisplayName` and `IReadOnlyList<IScaffoldHandler> Handlers`; constructor sets both; `using Scaffold.Cli.Handlers;`
- [X] T007 [P] Create `src/Scaffold.Cli/Root/IRootItemRegistry.cs` — interface with `IReadOnlyList<RootItem> GetItems()`; `using Scaffold.Cli.Root;`
- [X] T008 Write `tests/Scaffold.Cli.Tests/Handlers/HandlerContextTests.cs` — tests: `NewContext_WorkingDirectory_MatchesConstructorArg`, `NewContext_ShouldGoBackIsFalse`, `GoBack_SetsShouldGoBackTrue`; run `dotnet test` → must **PASS immediately** (pure value object)
- [X] T009 Write `tests/Scaffold.Cli.Tests/Root/RootItemRegistryTests.cs` — tests: `GetItems_ReturnsTwoItems`, `GetItems_FirstItemIsGitignore`, `GetItems_SecondItemIsGitattributes`, `GetItems_EachItemHasAtLeastOneHandler`, `GetItems_GitignoreHandler_IsDotnetNewHandler`; run `dotnet test` → must **FAIL** (`RootItemRegistry` not yet created)

**Checkpoint**: `dotnet build` succeeds. T008 PASSes. T009 FAILs. ✓

---

## Phase 3: User Stories 1 & 2 — Root Navigation + Handler Execution (Priority: P1) 🎯 MVP

**Goal**: Running `scaffold` → Root → `.gitignore` → "dotnet new" → executes `dotnet new gitignore` and returns to Root submenu. "<- Back" at Root item menu returns to top-level.

**Independent Test**: `dotnet test tests/Scaffold.Cli.Tests/` passes all US1+US2 unit tests. Manually: `scaffold` → Root → `.gitignore` → "dotnet new" → file created; Back from Root item menu returns to top-level.

### Test Helpers & TDD Gate (write first, confirm fail, then implement) ⚠️

> **NOTE: Write T010–T012 FIRST, ensure T011–T012 FAIL before starting implementation (T013)**

- [X] T010 [P] [US1] Create `tests/Scaffold.Cli.Tests/Handlers/StubCommandRunner.cs` — test helper implementing `ICommandRunner`; constructor takes `(int exitCode, string output, string error)`; `RunAsync` returns `Task.FromResult(new CommandResult(exitCode, output, error))` and records the last call in `LastExecutable`, `LastArgs`, `LastWorkingDirectory` properties
- [X] T011 [P] [US2] Write `tests/Scaffold.Cli.Tests/Handlers/DotnetNewHandlerTests.cs` using `StubCommandRunner`:
  - `ExecuteAsync_RunsCorrectExecutable`: assert `LastExecutable == "dotnet"`
  - `ExecuteAsync_PassesTemplateNameAsArg`: create `DotnetNewHandler("gitignore", stub)`, assert `LastArgs` contains `"new"` and `"gitignore"`
  - `ExecuteAsync_DisplaysOutput`: assert `console.Output` contains stub output text
  - `ExecuteAsync_CallsGoBack`: assert `context.ShouldGoBack == true` after execution
  - `ExecuteAsync_WithExtraArgs_IncludesThemInCall`: create `DotnetNewHandler("gitignore", stub, new[]{"--force"})`, assert `LastArgs` contains `"--force"`
  - Run `dotnet test` → must **FAIL** (`DotnetNewHandler` not yet created)
- [X] T012 [P] [US1] Write `tests/Scaffold.Cli.Tests/Root/RootMenuActionTests.cs` — create a `StubRootItemRegistry` inner class (or local stub) returning a fixed list with one item `new RootItem(".gitignore", new List<IScaffoldHandler>{ new StubHandler() })`; create `StubHandler` inner class that records if it was called and sets `context.GoBack()`:
  - `ExecuteAsync_BackSelected_CallsNavContextGoBack`: push DownArrow once + Enter (select "<- Back" - 2nd item when 1 root item exists), assert `navContext.ShouldGoBack == true`
  - `ExecuteAsync_ItemThenBackSelected_ReturnsToRootMenu`: push Enter (select .gitignore), push DownArrow + Enter (select "<- Back" from handler menu), assert `navContext.ShouldGoBack == false` (back at root loop, no Exit yet); push DownArrow + Enter (select "<- Back" from root), assert `navContext.ShouldGoBack == true`
  - `ExecuteAsync_HandlerSelected_ExecutesHandler`: push Enter (select .gitignore) + Enter (select handler), assert StubHandler was invoked
  - Run `dotnet test` → must **FAIL** (`RootMenuAction` not yet created)

> **GATE**: Run `dotnet test tests/Scaffold.Cli.Tests/` — T011, T012 must FAIL; T008 must PASS; T009 must FAIL. Do not proceed until confirmed.

### Implementation for User Stories 1 & 2

- [X] T013 [P] [US2] Implement `src/Scaffold.Cli/Handlers/ProcessCommandRunner.cs` — implements `ICommandRunner`; uses `System.Diagnostics.Process` with `ProcessStartInfo`: `ArgumentList.Add()` per arg (safe, no shell injection), `RedirectStandardOutput = true`, `RedirectStandardError = true`, `UseShellExecute = false`, `WorkingDirectory = workingDirectory`; awaits `WaitForExitAsync()`; returns `new CommandResult(exitCode, stdout, stderr)`
- [X] T014 [US2] Implement `src/Scaffold.Cli/Handlers/DotnetNewHandler.cs` — `string DisplayName => "dotnet new"`; constructor `(string templateName, ICommandRunner commandRunner, IReadOnlyList<string>? extraArgs = null)`; `ExecuteAsync`: builds args list `["new", templateName, ...extraArgs]`, calls `commandRunner.RunAsync("dotnet", args, context.WorkingDirectory)`, writes `result.Output` via `console.WriteLine` if non-empty, writes `result.Error` via `console.MarkupLine("[red]{result.Error}[/]")` if non-empty, calls `context.GoBack()`; run `dotnet test tests/Scaffold.Cli.Tests/Handlers/DotnetNewHandlerTests.cs` → T011 must **PASS**
- [X] T015 [US1] Implement `src/Scaffold.Cli/Root/RootItemRegistry.cs` — constructor `(ICommandRunner commandRunner)`; `_items` list:
  - `new RootItem(".gitignore", new List<IScaffoldHandler>{ new DotnetNewHandler("gitignore", commandRunner) })`
  - `new RootItem(".gitattributes", new List<IScaffoldHandler>{ new DotnetNewHandler("gitattributes", commandRunner) })`
  - Add comment: `// ADD NEW ROOT ITEMS HERE`
  - Run `dotnet test tests/Scaffold.Cli.Tests/Root/RootItemRegistryTests.cs` → T009 must **PASS**
- [X] T016 [US1] Implement `src/Scaffold.Cli/Root/RootMenuAction.cs` — implements `IMenuAction`; constructor `(IRootItemRegistry registry)`; `ExecuteAsync(IAnsiConsole console, NavigationContext navContext)`: outer `while(true)` loop → show `SelectionPrompt<string>` with `items.Select(i => i.DisplayName).Append("<- Back")` as title "Root scaffolding"; if "<- Back" selected → `navContext.GoBack(); return`; else look up item by display name → show inner `SelectionPrompt<string>` with `item.Handlers.Select(h => h.DisplayName).Append("<- Back")` as title `$"How would you like to scaffold {item.DisplayName}?"`; if "<- Back" → `continue`; else look up handler → `new HandlerContext(Directory.GetCurrentDirectory())` → `await handler.ExecuteAsync(console, ctx)`; run `dotnet test tests/Scaffold.Cli.Tests/Root/RootMenuActionTests.cs` → T012 must **PASS**
- [X] T017 [US1] Update `src/Scaffold.Cli/Categories/CategoryRegistry.cs` — add `RootMenuAction rootMenuAction` parameter to constructor; replace `new StubMenuAction("Root")` with `new Category("Root", rootMenuAction)`; also update `tests/Scaffold.Cli.Tests/Categories/CategoryRegistryTests.cs` to pass a `RootMenuAction` (using a stub `IRootItemRegistry` that returns empty list) so existing tests compile and continue to pass
- [X] T018 [US1] Update `src/Scaffold.Cli/Infrastructure/ServiceCollectionExtensions.cs` — add: `services.AddSingleton<ICommandRunner, ProcessCommandRunner>()`, `services.AddSingleton<IRootItemRegistry>(sp => new RootItemRegistry(sp.GetRequiredService<ICommandRunner>()))`, `services.AddSingleton<RootMenuAction>(sp => new RootMenuAction(sp.GetRequiredService<IRootItemRegistry>()))`; update `CategoryRegistry` factory to resolve and pass `RootMenuAction` from provider
- [X] T019 [US1] Run `dotnet test tests/Scaffold.Cli.Tests/` → all unit tests pass; run `dotnet build` → 0 errors

**Checkpoint**: `dotnet test` passes all unit tests. Run `scaffold` manually — Root submenu shows `.gitignore`, `.gitattributes`, `<- Back`. Select `.gitignore` → handler menu shows `dotnet new`, `<- Back`. Select `dotnet new` → `dotnet new gitignore` runs, output visible, returns to Root submenu. Select `<- Back` from Root submenu → top-level menu reappears.

---

## Phase 4: User Story 3 — Extensible Root Item Definition (Priority: P2)

**Goal**: A developer can add a new root scaffold item by editing only `RootItemRegistry.cs`. Reusing `DotnetNewHandler` with a different template name requires zero new classes.

**Independent Test**: Extensibility verified by code review and test: one-file change per new item (registry only). No handler class required if template name differs only.

- [X] T020 [P] [US3] Write `tests/Scaffold.Cli.Tests/Root/RootItemExtensibilityTests.cs`:
  - `AddingNewRootItem_OnlyRequiresRegistryChange`: create a `TestRootItemRegistry` that adds a third item `new RootItem(".editorconfig", new List<IScaffoldHandler>{ new DotnetNewHandler("editorconfig", new StubCommandRunner(0,"","")) })` alongside the standard two; assert count is 3 and `.editorconfig` is present
  - `ReusingDotnetNewHandler_RequiresNoNewClass`: assert `new DotnetNewHandler("editorconfig", new StubCommandRunner(0,"",""))` compiles and `DisplayName == "dotnet new"` — same class, different template
  - Run `dotnet test` → must **PASS immediately** (design already supports this; validates FR-011, FR-012)
- [X] T021 [US3] Add XML doc comment to `src/Scaffold.Cli/Root/RootItemRegistry.cs` above the item list: `/// <summary>ADD NEW ROOT ITEMS HERE — each entry is a display name + one or more IScaffoldHandler implementations.</summary>`
- [X] T022 [US3] Add XML doc comment to `src/Scaffold.Cli/Handlers/IScaffoldHandler.cs` documenting the interface contract: `DisplayName` shown in handler selection menu; `ExecuteAsync` MUST call `context.GoBack()` exactly once before returning; MUST NOT call `Environment.Exit()`

**Checkpoint**: T020 passes. Extensibility confirmed. Adding `.editorconfig` requires editing exactly one file (`RootItemRegistry.cs`).

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, packaging, documentation accuracy

- [X] T023 [P] Run full `dotnet test` from repository root — all three test projects pass: `dotnet test`
- [X] T024 [P] Build and pack to local NuGet feed: `dotnet pack src/Scaffold.Cli/ -o "$HOME\nuget-local" --configuration Release`; reinstall: `dotnet tool uninstall --global Scaffold.Cli && dotnet tool install --global Scaffold.Cli`
- [X] T025 [P] Validate `specs/003-root-submenu-handlers/quickstart.md` — walk through all steps; update any paths or command outputs that differ from final implementation
- [X] T026 Manual validation: launch `scaffold`; navigate Root → `.gitignore` → "dotnet new" → verify `.gitignore` created in current directory; navigate Root → `.gitattributes` → "dotnet new" → verify file created; verify `<- Back` at all levels works; verify Exit closes process cleanly (exit code 0)
- [X] T027 [P] Update `specs/003-root-submenu-handlers/checklists/requirements.md` — mark all items complete; add note confirming TDD gate was enforced

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Requires Phase 1 — **BLOCKS all user stories**
- **US1+US2 (Phase 3)**: Requires Phase 2 complete — MVP gate
- **US3 (Phase 4)**: Requires Phase 3 (uses `DotnetNewHandler` and `StubCommandRunner` from Phase 3)
- **Polish (Phase 5)**: Requires all user story phases complete

### User Story Dependencies

- **US1+US2 (P1)**: Single phase — both required for the first working slice
- **US3 (P2)**: Depends on US1+US2 (`DotnetNewHandler` and `RootItemRegistry` must exist for extensibility test)

### Within Phase 3

- T010–T012 (tests) must be written and FAIL before T013
- T014 depends on T010 (needs StubCommandRunner)
- T015 depends on T014 (DotnetNewHandler must exist for RootItemRegistry to use)
- T016 depends on T007 (IRootItemRegistry interface)
- T017 depends on T016 (RootMenuAction must exist for CategoryRegistry injection)
- T018 depends on T015 + T016 + T017

### Parallel Opportunities

- T002–T007 (foundational types): all parallel — different files
- T010–T012 (write failing tests): all parallel — different files
- T013–T014 (ProcessCommandRunner + DotnetNewHandler): parallel — different files
- T023, T024, T025, T027 (Polish): all parallel

---

## Parallel Example: Phase 3 TDD Gate

```
# Write all failing tests in parallel (different files):
T010: tests/Scaffold.Cli.Tests/Handlers/StubCommandRunner.cs       (test helper)
T011: tests/Scaffold.Cli.Tests/Handlers/DotnetNewHandlerTests.cs   (FAIL)
T012: tests/Scaffold.Cli.Tests/Root/RootMenuActionTests.cs         (FAIL)

# After GATE confirmed — implement in parallel where possible:
T013: src/Scaffold.Cli/Handlers/ProcessCommandRunner.cs            (parallel with T014)
T014: src/Scaffold.Cli/Handlers/DotnetNewHandler.cs                (parallel with T013)
# T015 follows T014; T016 follows T007; T017 follows T016; T018 follows T015+T016+T017
```

---

## Implementation Strategy

### MVP First (User Stories 1 & 2 — Phase 3 Only)

1. Complete Phase 1: Baseline verify
2. Complete Phase 2: All interfaces + value objects
3. Write T010–T012 (tests) → confirm T011, T012 FAIL
4. Complete Phase 3: Full implementation (T013–T019)
5. **STOP and VALIDATE**: `dotnet test` all pass; `scaffold` Root submenu works end-to-end

### Incremental Delivery

1. Phase 1 + 2 → Foundation
2. Phase 3 (US1+US2) → Root submenu + handler execution ← **MVP demo point**
3. Phase 4 (US3) → Extensibility confirmed by test ← **P2 delivery**
4. Phase 5 → Polish + pack + install

---

## Notes

- [P] = different files, no dependencies within same phase
- TDD gate is NON-NEGOTIABLE (Constitution Principle II) — T011 and T012 MUST fail before T013
- T008 (HandlerContextTests) will PASS immediately — pure value object, expected and correct
- T020 (extensibility test) will PASS immediately — design already satisfies US3, this is validation
- T010 (StubCommandRunner) is a test helper, not a test file itself — it does not have its own test
- `RootMenuAction` uses `SelectionPrompt<string>` with sentinel `"<- Back"` — display names for root items start with `.` so they cannot collide with the sentinel
- `ProcessCommandRunner` is not unit-tested directly (trivial OS wrapper) — covered by E2E
- T017 changes `CategoryRegistry` constructor signature — ALL existing `CategoryRegistryTests` must be updated to pass a stub `RootMenuAction` to compile

