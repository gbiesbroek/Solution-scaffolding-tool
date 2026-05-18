# Tasks: Spectre.Console Interactive Category Menu

**Input**: Design documents from `specs/002-spectre-console-menu/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Tool source: `src/Scaffold.Cli/`
- Unit tests: `tests/Scaffold.Cli.Tests/` *(new project)*
- E2E tests: `tests/Scaffold.Cli.EndToEnd/` *(existing, updated)*
- Solution: `SolutionScaffoldingTool.sln`

---

## Phase 1: Setup

**Purpose**: Add new test project; add required NuGet packages to existing projects

- [X] T001 Create unit test project `tests/Scaffold.Cli.Tests/` using `dotnet new xunit3 --framework net10.0 -n Scaffold.Cli.Tests -o tests/Scaffold.Cli.Tests`
- [X] T002 Add `tests/Scaffold.Cli.Tests/Scaffold.Cli.Tests.csproj` to solution: `dotnet sln add tests/Scaffold.Cli.Tests/Scaffold.Cli.Tests.csproj`
- [X] T003 Add ProjectReference from `tests/Scaffold.Cli.Tests/Scaffold.Cli.Tests.csproj` to `src/Scaffold.Cli/Scaffold.Cli.csproj` (standard assembly reference, not ReferenceOutputAssembly=false)
- [X] T004 [P] Add `Spectre.Console` package to `src/Scaffold.Cli/`: `dotnet add src/Scaffold.Cli/ package Spectre.Console`
- [X] T005 [P] Add `Microsoft.Extensions.DependencyInjection` package to `src/Scaffold.Cli/`: `dotnet add src/Scaffold.Cli/ package Microsoft.Extensions.DependencyInjection`
- [X] T006 [P] Add `Spectre.Console.Testing` package to `tests/Scaffold.Cli.Tests/`: `dotnet add tests/Scaffold.Cli.Tests/ package Spectre.Console.Testing`
- [X] T007 Delete default stub test file `tests/Scaffold.Cli.Tests/UnitTest1.cs` created by the template

**Checkpoint**: `dotnet build` succeeds with zero errors across all projects.

---

## Phase 2: Foundational

**Purpose**: Core abstractions and DI infrastructure — MUST be complete before all user story work

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T008 [P] Create `src/Scaffold.Cli/Actions/IMenuAction.cs` — interface with single method `Task ExecuteAsync(IAnsiConsole console, NavigationContext context)` using `Spectre.Console` and the not-yet-created `NavigationContext` type
- [X] T009 [P] Create `src/Scaffold.Cli/Menu/NavigationContext.cs` — mutable per-frame value object with `bool ShouldExit { get; private set; }`, `bool ShouldGoBack { get; private set; }`, `void Exit()` (sets ShouldExit=true), `void GoBack()` (sets ShouldGoBack=true)
- [X] T010 [P] Create `src/Scaffold.Cli/Categories/Category.cs` — sealed record or class with `string DisplayName` and `IMenuAction Action`; add optional `string? Description = null` property as a future extension point (FR-006)
- [X] T011 [P] Create `src/Scaffold.Cli/Categories/ICategoryRegistry.cs` — interface with `IReadOnlyList<Category> GetCategories()`
- [X] T012 [P] Create `src/Scaffold.Cli/Menu/IMenuRenderer.cs` — interface with `Task<Category> ShowMenuAsync(IReadOnlyList<Category> categories, string title)`
- [X] T013 Create `src/Scaffold.Cli/Infrastructure/ServiceCollectionExtensions.cs` — static class with `AddScaffoldServices(this IServiceCollection services)` extension method registering: `IAnsiConsole` → `AnsiConsole.Console` (singleton), `ICategoryRegistry` → `CategoryRegistry` (singleton), `IMenuRenderer` → `MenuRenderer` (singleton); leave stubs for CategoryRegistry and MenuRenderer until Phase 3

**Checkpoint**: `dotnet build` succeeds — all interfaces and NavigationContext compile. DI extension method compiles with forward references.

---

## Phase 3: User Story 1 — Navigate Top-Level Category Menu (Priority: P1) 🎯 MVP

**Goal**: Running `scaffold` shows an interactive Spectre.Console menu with Aspire, Frontend, Core, Root, Exit. Selecting Exit quits the program cleanly.

**Independent Test**: `dotnet test tests/Scaffold.Cli.Tests/` passes all US1 unit tests. `dotnet test tests/Scaffold.Cli.EndToEnd/` passes with menu title visible in stdout.

### Tests for User Story 1 (TDD — write first, confirm fail, then implement) ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before starting implementation (T018)**

- [X] T014 [P] [US1] Write `tests/Scaffold.Cli.Tests/Menu/NavigationContextTests.cs` — unit tests: `Exit_SetsExitFlag`, `GoBack_SetsGoBackFlag`, `NewContext_BothFlagsFalse`; run `dotnet test` → these should **PASS immediately** (pure value object, no dependencies yet)
- [X] T015 [P] [US1] Write `tests/Scaffold.Cli.Tests/Actions/ExitMenuActionTests.cs` — test `ExecuteAsync_SetsContextShouldExit`: create `new TestConsole()`, `new NavigationContext()`, call `new ExitMenuAction().ExecuteAsync(console, context)`, assert `context.ShouldExit == true` and `context.ShouldGoBack == false`; run `dotnet test` → must **FAIL** (`ExitMenuAction` not yet implemented)
- [X] T016 [P] [US1] Write `tests/Scaffold.Cli.Tests/Menu/MenuRendererTests.cs` — test `ShowMenuAsync_ReturnsSelectedCategory`: create `TestConsole`, push `ConsoleKey.Enter` via `console.Input.PushKey(ConsoleKey.Enter)`, call `ShowMenuAsync` with a two-item list, assert returned `Category.DisplayName` equals the first item; run `dotnet test` → must **FAIL** (`MenuRenderer` not yet implemented)
- [X] T017 [P] [US1] Write `tests/Scaffold.Cli.Tests/Categories/CategoryRegistryTests.cs` — tests: `GetCategories_ReturnsFiveCategories`, `GetCategories_LastCategoryIsExit`, `GetCategories_FirstCategoryIsAspire`, `GetCategories_ExitAction_IsExitMenuAction`; run `dotnet test` → must **FAIL** (`CategoryRegistry` not yet implemented)

> **GATE**: Run `dotnet test tests/Scaffold.Cli.Tests/` — T015, T016, T017 must FAIL; T014 must PASS. Do not proceed until confirmed.

### Implementation for User Story 1

- [X] T018 [US1] Implement `src/Scaffold.Cli/Actions/ExitMenuAction.cs` — class implementing `IMenuAction`; `ExecuteAsync` calls `context.Exit()` and returns `Task.CompletedTask`; run `dotnet test tests/Scaffold.Cli.Tests/Actions/ExitMenuActionTests.cs` → T015 must now **PASS**
- [X] T019 [US1] Implement `src/Scaffold.Cli/Actions/StubMenuAction.cs` — class implementing `IMenuAction`; constructor takes `string categoryName`; `ExecuteAsync`: writes `"[categoryName] coming soon..."` via `console.MarkupLine(...)`, shows a one-item `SelectionPrompt<string>` with only `"← Back"` using the passed `console`, then calls `context.GoBack()`
- [X] T020 [US1] Implement `src/Scaffold.Cli/Menu/MenuRenderer.cs` — class implementing `IMenuRenderer`; constructor takes `IAnsiConsole`; `ShowMenuAsync` builds `new SelectionPrompt<Category>().Title(title).UseConverter(c => c.DisplayName).WrapAround().AddChoices(categories)` and returns `await Task.FromResult(console.Prompt(prompt))`; run `dotnet test tests/Scaffold.Cli.Tests/Menu/MenuRendererTests.cs` → T016 must now **PASS**
- [X] T021 [US1] Implement `src/Scaffold.Cli/Categories/CategoryRegistry.cs` — class implementing `ICategoryRegistry`; constructor takes `ExitMenuAction exitAction`; `GetCategories()` returns fixed ordered list: Aspire/StubMenuAction("Aspire"), Frontend/StubMenuAction("Frontend"), Core/StubMenuAction("Core"), Root/StubMenuAction("Root"), Exit/exitAction; run `dotnet test tests/Scaffold.Cli.Tests/Categories/CategoryRegistryTests.cs` → T017 must now **PASS**
- [X] T022 [US1] Update `src/Scaffold.Cli/Infrastructure/ServiceCollectionExtensions.cs` — complete `AddScaffoldServices()`: add `.AddSingleton<ExitMenuAction>()` and factory-register `ICategoryRegistry` as `CategoryRegistry` resolving `ExitMenuAction` from the provider; verify `dotnet build` succeeds
- [X] T023 [US1] Replace `src/Scaffold.Cli/Program.cs` — full implementation: build `ServiceCollection`, call `.AddScaffoldServices()`, `BuildServiceProvider()` in `using` block, resolve `ICategoryRegistry` and `IMenuRenderer`, run navigation loop: `while(true) { var ctx = new NavigationContext(); var cat = await renderer.ShowMenuAsync(registry.GetCategories(), "What would you like to scaffold?"); await cat.Action.ExecuteAsync(console, ctx); if (ctx.ShouldExit) break; }`
- [X] T024 [US1] Update `tests/Scaffold.Cli.EndToEnd/ScaffoldCliTests.cs` — update `Tool_DisplaysPromptMessage_OnStartup` to assert stdout contains `"What would you like to scaffold?"` instead of `"Press Enter to exit..."`; update `Tool_ExitsCleanly_AfterReceivingInput` to account for Spectre.Console non-interactive fallback behaviour (close stdin after short delay); run `dotnet test tests/Scaffold.Cli.EndToEnd/` → all 3 tests must **PASS**

**Checkpoint**: `dotnet test` passes all unit and E2E tests. Run `scaffold` manually — menu appears, navigate with arrow keys, select Exit → process exits with code 0.

---

## Phase 4: User Story 2 — Navigate Back to Top-Level (Priority: P2)

**Goal**: Selecting Aspire/Frontend/Core/Root shows a stub sub-context with "← Back". Selecting "← Back" returns to the top-level menu without restarting the process.

**Independent Test**: Unit tests verify StubMenuAction calls `context.GoBack()`. Integration test verifies the navigation loop redisplays the top-level menu after a GoBack signal.

### Tests for User Story 2 (TDD — write first, confirm fail, then implement) ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before starting implementation (T027)**

- [X] T025 [P] [US2] Write `tests/Scaffold.Cli.Tests/Actions/StubMenuActionTests.cs` — tests: `ExecuteAsync_SetsGoBackFlag` (create `TestConsole`, push `ConsoleKey.Enter`, create `StubMenuAction("Core")`, execute, assert `context.ShouldGoBack == true` and `context.ShouldExit == false`); `ExecuteAsync_DisplaysCategoryName` (assert `console.Output` contains `"Core"`); run `dotnet test` → must **FAIL** (`StubMenuAction` not yet fully tested — it was created in T019 but tests were not written first)
- [X] T026 [US2] Write `tests/Scaffold.Cli.Tests/Menu/NavigationLoopTests.cs` — test `AfterGoBack_TopLevelMenuIsShownAgain`: use `TestConsole` with queued keys: first `DownArrow` × 3 + `Enter` (select Root → sub-menu), then `Enter` (select ← Back), then `DownArrow` × 4 + `Enter` (navigate to Exit), assert final `context.ShouldExit == true`; run `dotnet test` → must **FAIL** (loop logic not separately unit-testable yet — extract loop to `AppRunner` class)

> **GATE**: Run `dotnet test tests/Scaffold.Cli.Tests/` — T025, T026 must FAIL. Do not proceed until confirmed.

### Implementation for User Story 2

- [X] T027 [US2] Extract navigation loop from `src/Scaffold.Cli/Program.cs` into a new `src/Scaffold.Cli/AppRunner.cs` class — `AppRunner(IAnsiConsole, ICategoryRegistry, IMenuRenderer)` with `async Task RunAsync()` containing the while loop; register `AppRunner` as transient in `ServiceCollectionExtensions.cs`; run `dotnet test tests/Scaffold.Cli.Tests/Actions/StubMenuActionTests.cs` and `NavigationLoopTests.cs` → T025 and T026 must **PASS**
- [X] T028 [US2] Update `src/Scaffold.Cli/Program.cs` — resolve `AppRunner` from DI and call `await runner.RunAsync()`; confirm `dotnet test` still passes all tests

**Checkpoint**: Run `scaffold` — select Core → see "Core coming soon..." + "← Back" menu → select ← Back → top-level menu reappears. All tests pass.

---

## Phase 5: User Story 3 — Extensible Category Definition (Priority: P3)

**Goal**: A developer can add a new category by editing only `CategoryRegistry.cs`. No menu rendering or navigation code changes required.

**Independent Test**: Unit test adds a category to a test-subclass of `CategoryRegistry` and verifies it appears in the menu. Code review of a one-line category addition confirms zero changes to any other file.

### Tests for User Story 3 (TDD — write first, confirm fail, then implement) ⚠️

- [X] T029 [P] [US3] Write `tests/Scaffold.Cli.Tests/Categories/CategoryExtensibilityTests.cs` — test `AddingNewCategory_AppearsInMenu`: subclass `CategoryRegistry` overriding `GetCategories()` to return base list + one extra `Category("Infrastructure", new StubMenuAction("Infrastructure"))`; verify count is 6 and "Infrastructure" appears in `IMenuRenderer.ShowMenuAsync` output; run `dotnet test` → must **PASS** (extensibility is already designed in, this is a verification test)

> **GATE**: Confirm T029 passes without any implementation changes — this validates the design satisfies US3 without additional work.

### Implementation for User Story 3

- [X] T030 [US3] Add XML doc comment to `src/Scaffold.Cli/Categories/CategoryRegistry.cs` above the category list clearly marking: `// ADD NEW CATEGORIES HERE — above the Exit entry`; no functional code changes required
- [X] T031 [US3] Verify `src/Scaffold.Cli/Categories/Category.cs` `Description` property is accessible in tests and usable by future action handlers — write one assertion in `CategoryExtensibilityTests.cs`: `new Category("Test", new ExitMenuAction()) { Description = "A description" }` compiles and the property round-trips; confirm `dotnet test` passes

**Checkpoint**: Extensibility verified by test. Adding a new category to `CategoryRegistry.cs` requires zero changes elsewhere — confirmed by test subclass pattern.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, packaging, documentation accuracy

- [X] T032 [P] Run full `dotnet test` from repository root — confirm all three test projects pass (unit + E2E): `dotnet test`
- [X] T033 [P] Build and pack to local NuGet feed: `dotnet pack src/Scaffold.Cli/ -o "$HOME\nuget-local" --configuration Release`; reinstall: `dotnet tool uninstall --global Scaffold.Cli && dotnet tool install --global Scaffold.Cli`
- [X] T034 [P] Validate `specs/002-spectre-console-menu/quickstart.md` — walk through all steps (run tool, run tests, build/pack, add category example); update any paths or command outputs that differ from final implementation
- [X] T035 Run the complete manual validation: launch `scaffold`, navigate full menu, verify all 5 categories appear, verify Exit exits cleanly (code 0), verify stub sub-contexts show placeholder + "← Back", verify Back returns to top-level
- [X] T036 [P] Update `specs/002-spectre-console-menu/checklists/requirements.md` — mark all items complete; add note confirming TDD gate was enforced (tests written and confirmed failing before implementation)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Requires Phase 1 complete — **BLOCKS all user stories**
- **US1 (Phase 3)**: Requires Phase 2 complete — MVP gate
- **US2 (Phase 4)**: Requires US1 complete (AppRunner extraction depends on Phase 3 loop)
- **US3 (Phase 5)**: Requires Phase 2 complete — independent of US1/US2 in theory, but Category.cs and CategoryRegistry.cs created in Phase 3
- **Polish (Phase 6)**: Requires all user story phases complete

### User Story Dependencies

- **US1 (P1)**: Depends only on Foundational phase — delivers the full MVP
- **US2 (P2)**: Depends on US1 (AppRunner extracted in T027 builds on T023 loop)
- **US3 (P3)**: Depends on US1 (CategoryRegistry.cs created in T021)

### Within Each User Story

- **Tests MUST be written and confirmed FAILING before implementation** (Constitution Principle II)
- T014 (NavigationContextTests) is expected to PASS immediately — it tests a pure value object
- T015, T016, T017 must FAIL before T018, T020, T021 respectively
- T025, T026 must FAIL before T027
- All [P] tasks within a phase can run in parallel

### Parallel Opportunities

- T004, T005, T006 (package installs) — all parallel in Phase 1
- T008–T012 (interface/entity definitions) — all parallel in Phase 2
- T014–T017 (write tests) — all parallel in Phase 3
- T015, T016, T017 (write failing tests) — all parallel, no file conflicts
- T018–T020 (implement ExitMenuAction, StubMenuAction, MenuRenderer) — all parallel

---

## Parallel Example: User Story 1

```bash
# Write all failing tests in parallel (different files):
Task T014: tests/Scaffold.Cli.Tests/Menu/NavigationContextTests.cs
Task T015: tests/Scaffold.Cli.Tests/Actions/ExitMenuActionTests.cs
Task T016: tests/Scaffold.Cli.Tests/Menu/MenuRendererTests.cs
Task T017: tests/Scaffold.Cli.Tests/Categories/CategoryRegistryTests.cs

# After GATE confirmed — implement in parallel (different files):
Task T018: src/Scaffold.Cli/Actions/ExitMenuAction.cs
Task T019: src/Scaffold.Cli/Actions/StubMenuAction.cs
Task T020: src/Scaffold.Cli/Menu/MenuRenderer.cs
Task T021: src/Scaffold.Cli/Categories/CategoryRegistry.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (packages + new test project)
2. Complete Phase 2: Foundational (interfaces + DI shell)
3. Write US1 tests (T014–T017) → confirm T015–T017 FAIL
4. Complete Phase 3: US1 implementation (T018–T024)
5. **STOP and VALIDATE**: `dotnet test` all pass; `scaffold` shows interactive menu; Exit works
6. Ship MVP

### Incremental Delivery

1. Phase 1 + 2 → Foundation
2. Phase 3 (US1) → Interactive menu + Exit ← **MVP demo point**
3. Phase 4 (US2) → Stub sub-contexts with back navigation ← **P2 delivery**
4. Phase 5 (US3) → Extensibility confirmed by test ← **P3 delivery**
5. Phase 6 → Polish + pack + install

---

## Notes

- [P] = different files, no dependencies within same phase
- TDD gate is NON-NEGOTIABLE (Constitution Principle II) — do not skip
- `NavigationContextTests` (T014) will pass immediately — that is expected and correct
- E2E tests (T024) must be updated before Phase 3 can be considered complete — they currently assert `"Press Enter to exit..."` which will break
- `StubMenuAction` constructor takes `categoryName: string` so each stub displays its category name in the placeholder message
- `AppRunner` (T027) is the key refactor that makes the navigation loop unit-testable; do not skip
- `Scaffold.Cli.Tests` uses a direct assembly reference to `Scaffold.Cli` (not `ReferenceOutputAssembly=false`) because it needs to test internal types, not launch a process
