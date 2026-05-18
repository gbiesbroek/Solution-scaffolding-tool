# Feature Specification: Interactive CLI NuGet Tool

**Feature Branch**: `001-cli-nuget-tool`
**Created**: 2026-05-18
**Status**: Draft
**Input**: Build a C# CLI tool that gets published to local NuGet. Runs in interactive mode only —
waits for user input then exits. Source under `/src`. End-to-end tests under `/tests`
using xunit and Microsoft.Testing.Platform verifying the app waits for user input and closes.

## Clarifications

### Session 2026-05-18

- Q: What is the CLI tool's invocation command name? → A: scaffold
- Q: Does the tool display a message while waiting for input? → A: Yes — display a short prompt message (e.g., `Press Enter to exit...`) before waiting
- Q: Where is the local NuGet feed located? → A: ~~Use the machine's default local NuGet cache (`~/.nuget/packages`)~~ **Amended**: Create a dedicated local directory as a NuGet feed source and register it as a named NuGet source for the current user

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Install and Run CLI Tool (Priority: P1)

A developer installs the CLI tool from a local NuGet feed and runs it. The tool starts,
waits for the user to type any input and press Enter, and then exits cleanly.

**Why this priority**: This is the core deliverable. Without a working, installable interactive
CLI, nothing else is meaningful.

**Independent Test**: Install the tool from the local NuGet feed, launch it, provide any input,
and confirm it exits with code 0. Delivers a fully functional CLI artifact.

**Acceptance Scenarios**:

1. **Given** the tool is packed and the `/nuget-local` feed directory is registered, **When**
   a developer runs `dotnet tool install --global scaffold` using the local feed, **Then** the
   tool installs successfully and is invokable as `scaffold`.
2. **Given** the tool is installed, **When** the developer invokes `scaffold`, **Then** it
   displays a short prompt message (e.g., `Press Enter to exit...`) and waits for input
   without exiting on its own.
3. **Given** the tool is waiting for input, **When** the developer types any text and presses
   Enter, **Then** the tool exits with exit code 0.

---

### User Story 2 - Automated End-to-End Verification (Priority: P2)

An automated test suite confirms the CLI tool's interactive behavior: it waits for input
and closes correctly. Tests are authored with xunit and run via Microsoft.Testing.Platform.

**Why this priority**: Ensures the tool's interactive contract is maintained as the codebase
evolves. Depends on User Story 1 being complete.

**Independent Test**: Run `dotnet test` in the `/tests` project; all tests pass and verify
the tool process waits for stdin and terminates upon receiving input.

**Acceptance Scenarios**:

1. **Given** the test project is configured with xunit and Microsoft.Testing.Platform,
   **When** the test suite runs, **Then** all tests complete and results are reported.
2. **Given** the CLI tool process is launched by a test, **When** the test writes input to
   stdin, **Then** the process terminates within a reasonable time.
3. **Given** the CLI tool process is launched by a test, **When** no input is provided for
   a short interval, **Then** the process remains running (it is waiting, not crashed).

---

### Edge Cases

- An empty line (just Enter) counts as valid input and triggers exit.
- Piping empty stdin (EOF without content) causes immediate exit; acceptable in v1.
- If publish fails (e.g., feed unreachable), the `.NET` CLI MUST report an error; no
  special recovery is required in this version.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The CLI tool MUST be packaged as a .NET global tool with command name `scaffold`
  and published to a dedicated local NuGet feed directory in the user's home directory (`~/nuget-local`).
- **FR-002**: The CLI tool MUST operate exclusively in interactive mode; it MUST wait for
  user input from stdin before exiting.
- **FR-003**: The CLI tool MUST exit cleanly (exit code 0) after receiving any line of input.
- **FR-004**: The tool source code MUST reside under `/src` in the repository.
- **FR-005**: An end-to-end test project MUST exist under `/tests`, using xunit and
  Microsoft.Testing.Platform.
- **FR-006**: The end-to-end tests MUST verify that the tool waits for user input and
  exits after receiving it.
- **FR-007**: A local NuGet feed directory (`~/nuget-local`) MUST exist in the user's home
  directory and be registered as a named NuGet source for the current user (via `dotnet nuget add source`),
  so that `dotnet tool install --global Scaffold.Cli` resolves the package from this feed.
- **FR-009**: The project MUST provide a setup script or documented command sequence that:
  registers the `~/nuget-local` directory as a NuGet source, packs the tool, and publishes
  the `.nupkg` to that directory — all without touching the machine's global NuGet cache.
- **FR-008**: When launched, the tool MUST write a short prompt message to stdout before
  reading input (e.g., `Press Enter to exit...`). The E2E tests MUST assert this message
  is present in stdout.

### Key Entities

- **CLI Tool (`scaffold`)**: The .NET global tool package — entry point, interactive loop, exit behavior.
- **Local NuGet Feed**: A dedicated directory (`~/nuget-local`) in the user's home directory,
  registered as a named NuGet source for the current user; receives packed `.nupkg` artifacts
  and serves them to `dotnet tool install`.
- **End-to-End Test Suite**: An xunit-based test project that launches the CLI tool as a
  child process and validates its interactive behavior.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can install and invoke the CLI tool from the local NuGet feed
  in under 2 minutes on a clean machine with .NET installed.
- **SC-002**: The tool exits within 5 seconds of receiving user input during manual or
  automated testing.
- **SC-003**: The tool remains running (does not crash or auto-exit) for at least 10 seconds
  when no input is provided.
- **SC-004**: 100% of the end-to-end tests pass on a standard developer machine with .NET
  and xunit configured.
- **SC-005**: The publish-to-local-NuGet workflow (register feed source, pack, publish to
  `~/nuget-local`) completes without errors using standard .NET CLI commands.

## Assumptions

- The local NuGet feed is a dedicated directory (`~/nuget-local`) in the user's home directory,
  registered as a named NuGet source for the current user via `dotnet nuget add source`. It does
  not use or modify the machine's global NuGet packages cache (`~/.nuget/packages`).
- The tool targets a current LTS version of .NET (.NET 8 or .NET 9).
- The end-to-end tests launch the CLI as a separate OS process and interact via stdin/stdout.
- An empty line (just Enter) counts as valid input and triggers exit.
- Piping empty stdin (EOF without content) may cause immediate exit; this is acceptable
  behavior and does not need special handling in v1.
- No authentication, logging, or configuration files are required for this initial version.
- The repository contains a single solution with the `/src` and `/tests` project layout.
