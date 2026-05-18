---

description: "Task list for Interactive CLI NuGet Tool (scaffold)"
---

# Tasks: Interactive CLI NuGet Tool (`scaffold`)

**Input**: Design documents from `specs/001-cli-nuget-tool/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅ quickstart.md ✅

**Tests**: E2E tests are explicitly required by spec (FR-005, FR-006). Test tasks are included.
**TDD**: Per Constitution Principle II — tests MUST be written and confirmed to fail before implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2)
- Include exact file paths in descriptions

## Path Conventions

- Tool source: `src/Scaffold.Cli/`
- E2E tests: `tests/Scaffold.Cli.EndToEnd/`
- Local NuGet feed: `~/nuget-local/` (user home directory)
- Solution: `SolutionScaffoldingTool.sln`

---

## Phase 1: Setup

**Purpose**: Solution and project initialization

- [x] T001 Create `~/nuget-local/` directory in user's home directory
- [x] T002 `.gitignore` already covers `*.nupkg` globally; `nuget-local/` removed from repo
- [ ] T003 [P] Create solution file `SolutionScaffoldingTool.sln` with `dotnet new sln`
- [ ] T004 [P] Create CLI tool project `src/Scaffold.Cli/Scaffold.Cli.csproj` with `dotnet new console`
- [ ] T005 [P] Create E2E test project `tests/Scaffold.Cli.EndToEnd/Scaffold.Cli.EndToEnd.csproj` with `dotnet new xunit`
- [ ] T006 Add `src/Scaffold.Cli/` to solution: `dotnet sln add src/Scaffold.Cli/Scaffold.Cli.csproj`
- [ ] T007 Add `tests/Scaffold.Cli.EndToEnd/` to solution: `dotnet sln add tests/Scaffold.Cli.EndToEnd/Scaffold.Cli.EndToEnd.csproj`

---

## Phase 2: Foundational

**Purpose**: Project configuration that MUST be complete before any user story work begins

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T008 Configure `src/Scaffold.Cli/Scaffold.Cli.csproj` as a .NET 9 global tool — set `<PackAsTool>true</PackAsTool>`, `<ToolCommandName>scaffold</ToolCommandName>`, `<PackageId>Scaffold.Cli</PackageId>`, `<Version>1.0.0</Version>`, `<TargetFramework>net9.0</TargetFramework>`
- [ ] T009 Configure `tests/Scaffold.Cli.EndToEnd/Scaffold.Cli.EndToEnd.csproj` — set `<TargetFramework>net9.0</TargetFramework>`, add `Microsoft.Testing.Platform.MSBuild`, `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk` package references
- [x] T010 Register the local NuGet feed for the current user: `dotnet nuget add source "$HOME/nuget-local" --name scaffold-local` — documented in `quickstart.md`

**Checkpoint**: Solution builds (`dotnet build`), test project compiles, NuGet source registered.

---

## Phase 3: User Story 1 — Install and Run CLI Tool (Priority: P1) 🎯 MVP

**Goal**: A developer can pack `scaffold`, install it from `~/nuget-local`, run it, and see it wait for input then exit.

**Independent Test**: `dotnet pack src/Scaffold.Cli/ -o "$HOME/nuget-local"` followed by `dotnet tool install --global Scaffold.Cli` succeeds; invoking `scaffold` prints `Press Enter to exit...` and exits after Enter.

### Tests for User Story 1 (TDD — write first, confirm fail, then implement) ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before starting implementation (T013)**

- [ ] T011 [P] [US1] Write E2E test `Tool_DisplaysPromptMessage_OnStartup` in `tests/Scaffold.Cli.EndToEnd/ScaffoldCliTests.cs` — launch binary from build output, read stdout, assert it contains `Press Enter to exit...`
- [ ] T012 [P] [US1] Write E2E test `Tool_RemainsRunning_WhenNoInputProvided` in `tests/Scaffold.Cli.EndToEnd/ScaffoldCliTests.cs` — launch binary, wait 500ms, assert process has not exited
- [ ] T013 [US1] Write E2E test `Tool_ExitsCleanly_AfterReceivingInput` in `tests/Scaffold.Cli.EndToEnd/ScaffoldCliTests.cs` — launch binary, write any line to stdin, await process exit (timeout 5s), assert exit code 0

> **GATE**: Run `dotnet test tests/Scaffold.Cli.EndToEnd/` — all three tests MUST fail before proceeding

### Implementation for User Story 1

- [ ] T014 [US1] Implement `src/Scaffold.Cli/Program.cs` — write `Console.WriteLine("Press Enter to exit...");` then `Console.ReadLine();` (two lines; no additional logic needed)
- [ ] T015 [US1] Verify `dotnet build src/Scaffold.Cli/` succeeds and binary is produced at expected output path used by E2E tests
- [x] T016 [US1] Pack the tool: `dotnet pack src/Scaffold.Cli/ -o "$HOME/nuget-local"` — confirm `~/nuget-local/Scaffold.Cli.1.0.0.nupkg` is created
- [x] T017 [US1] Install the global tool: `dotnet tool install --global Scaffold.Cli` — confirm `scaffold` is invokable from any directory

**Checkpoint**: Run `dotnet test tests/Scaffold.Cli.EndToEnd/` — all three tests MUST now pass. Manually run `scaffold`, type input, confirm exit.

---

## Phase 4: User Story 2 — Automated End-to-End Verification (Priority: P2)

**Goal**: The E2E test suite is complete, reliable, and runs via `dotnet test` in CI without any manual install step.

**Independent Test**: `dotnet test tests/Scaffold.Cli.EndToEnd/` passes on a clean machine with only the .NET SDK — no global tool install required.

### Implementation for User Story 2

- [ ] T018 [US2] Resolve binary path in `tests/Scaffold.Cli.EndToEnd/ScaffoldCliTests.cs` using `AppContext.BaseDirectory` relative path to `src/Scaffold.Cli/bin/Debug/net9.0/Scaffold.Cli[.exe]` — handle Windows vs Unix executable extension
- [ ] T019 [US2] Add `ProcessStartInfo` helper or test fixture in `tests/Scaffold.Cli.EndToEnd/ScaffoldCliTests.cs` — configure `RedirectStandardInput = true`, `RedirectStandardOutput = true`, `UseShellExecute = false`
- [ ] T020 [US2] Ensure test `Tool_RemainsRunning_WhenNoInputProvided` uses `Task.Delay(500)` idle check and asserts `process.HasExited == false` before sending any input
- [ ] T021 [US2] Ensure test `Tool_ExitsCleanly_AfterReceivingInput` uses `process.WaitForExitAsync` with a `CancellationToken` set to 5 seconds to enforce SC-002 timeout
- [ ] T022 [US2] Run full suite `dotnet test tests/Scaffold.Cli.EndToEnd/ --logger trx` — confirm all tests pass and TRX report is generated

**Checkpoint**: All three E2E tests pass without any global tool install. `dotnet build` + `dotnet test` is the only prerequisite.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, gitignore, and final validation

- [ ] T023 [P] Validate `.gitignore` covers common C#/.NET patterns; `nuget-local/` is not in the repo and needs no gitignore entry
- [ ] T024 [P] Validate `specs/001-cli-nuget-tool/quickstart.md` against the final project structure — update any paths or commands that differ from implementation
- [ ] T025 Run the full quickstart end-to-end: register feed, pack, install, invoke `scaffold`, confirm prompt appears, provide input, confirm exit code 0
- [ ] T026 Run `dotnet test` from repository root — confirm all E2E tests pass
- [ ] T027 [P] Add XML doc comments to `src/Scaffold.Cli/Program.cs` if any public surface exists; otherwise confirm file is minimal and needs no comments

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately. T003, T004, T005 can run in parallel.
- **Foundational (Phase 2)**: Depends on Phase 1 completion. T008, T009 can run in parallel. T010 is independent.
- **User Story 1 (Phase 3)**: Depends on Foundational. T011, T012 can be written in parallel. T013 depends on T011/T012 (same file — write sequentially). T014 can start in parallel with T011–T013 (different file). T015 depends on T014. T016 depends on T015. T017 depends on T016.
- **User Story 2 (Phase 4)**: Depends on Phase 3 (tests from T011–T013 must exist). T018–T022 refine the tests.
- **Polish (Phase 5)**: Depends on all user stories. T023, T024, T027 can run in parallel.

### User Story Dependencies

- **US1 (P1)**: Depends only on Foundational — no dependency on US2.
- **US2 (P2)**: Depends on US1 test stubs (T011–T013) existing — the implementation work in Phase 4 builds on them.

### Within Each User Story

- TDD tests (T011–T013) MUST be written and confirmed failing before T014 (implementation)
- T014 (implementation) before T015 (build verification)
- T015 before T016 (pack) before T017 (install)

### Parallel Opportunities

```bash
# Phase 1 — run together:
Task T003: dotnet new sln
Task T004: dotnet new console -o src/Scaffold.Cli
Task T005: dotnet new xunit -o tests/Scaffold.Cli.EndToEnd

# Phase 2 — run together:
Task T008: Configure Scaffold.Cli.csproj
Task T009: Configure EndToEnd.csproj
Task T010: Register NuGet source

# Phase 3 — TDD tests written in parallel then implementation:
Task T011: Write prompt test
Task T012: Write liveness test
# → then T013, then T014 (implementation)

# Phase 5 — run together:
Task T023: Update .gitignore
Task T024: Validate quickstart.md
Task T027: Review Program.cs comments
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Write & fail E2E tests (T011–T013)
4. Complete Phase 3: User Story 1 (T014–T017)
5. **STOP and VALIDATE**: All E2E tests pass, `scaffold` installs and runs correctly

### Incremental Delivery

1. Setup + Foundational → builds cleanly
2. TDD tests fail → baseline confirmed
3. US1 implementation → tests pass, tool installable
4. US2 refinements → tests reliable for CI
5. Polish → docs and gitignore finalized

---

## Notes

- `[P]` tasks = different files or independent commands, can run in parallel
- `[US1]` / `[US2]` labels map tasks to user stories for traceability (Constitution Principle V)
- TDD gate between T013 and T014 is NON-NEGOTIABLE (Constitution Principle II)
- The E2E binary path in tests uses `AppContext.BaseDirectory` — ensure `dotnet build` is run before `dotnet test`
- On Windows, the binary is `Scaffold.Cli.exe`; on Linux/macOS it is `Scaffold.Cli` — handle in test helper
- nuget-local/` is in the user's home directory (outside the repo) and needs no `.gitignore` entry
