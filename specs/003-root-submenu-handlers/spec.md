# Feature Specification: Root Submenu with Command Handlers

**Feature Branch**: `003-root-submenu-handlers`
**Created**: 2026-05-18
**Status**: Draft
**Input**: User description: "In the category Root add subitems that show up as new menu once Root is picked. Item DisplayNames are '.gitignore' and '.gitattributes'. More items may be added later. Keep the back option as well. Once an item like '.gitignore' is selected, the user needs to see another menu with options to handle the selected choice. Create a generic handler interface that can deal with executing commands (example dotnet, npm, pwsh etc) or asking user for input (like a fileName). Create a basic DotnetNewHandler as example and register it for both '.gitignore' and '.gitattributes'. If selected the handler should execute the command, in this case 'dotnet new gitignore' and 'dotnet new gitattributes'. Make sure the commands are flexible for adding arguments and changing names that may be required for future options."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Navigate the Root Submenu (Priority: P1)

A developer selects "Root" from the top-level scaffold menu. A new submenu appears listing available root-level scaffold items (".gitignore", ".gitattributes"). The developer can choose an item or select "<- Back" to return to the top-level menu. No action is taken until an item is confirmed.

**Why this priority**: Delivers the core navigation structure that all subsequent handler work builds on. Provides immediate value — the Root category becomes functional instead of a stub.

**Independent Test**: Can be fully tested by launching `scaffold`, selecting Root, verifying the submenu appears with the correct items and a back option, and confirming back navigation returns to the top-level menu.

**Acceptance Scenarios**:

1. **Given** the tool is running and the top-level menu is visible, **When** the user selects "Root", **Then** a submenu appears listing ".gitignore", ".gitattributes", and "<- Back" — in that order.
2. **Given** the Root submenu is visible, **When** the user selects "<- Back"", **Then** the top-level menu is re-displayed and the process remains running.
3. **Given** the Root submenu is visible, **When** a new root item is added to the registry, **Then** it appears in the submenu without changes to any other file.

---

### User Story 2 — Execute a Root Scaffold Item via Handler (Priority: P1)

A developer selects ".gitignore" (or ".gitattributes") from the Root submenu. A handler selection menu appears showing all registered handlers for that item. The developer selects a handler, which executes the appropriate `dotnet new` command on their behalf. The developer sees the command output and is returned to the Root submenu.

**Why this priority**: Same priority as US1 because the submenu without any working action delivers no real value. Together US1 + US2 form the first complete working feature slice.

**Independent Test**: Can be tested by selecting Root → ".gitignore" and verifying that the `dotnet new gitignore` command is executed and its output is visible to the user.

**Acceptance Scenarios**:

1. **Given** the Root submenu is visible, **When** the user selects ".gitignore", **Then** a handler selection menu appears listing all registered handlers for ".gitignore" (initially: "dotnet new") plus "<- Back".
2. **Given** the handler selection menu is visible, **When** the user selects "dotnet new", **Then** the `dotnet new gitignore` command is executed and its output is displayed.
3. **Given** the handler selection menu is visible for ".gitattributes", **When** the user selects "dotnet new", **Then** the `dotnet new gitattributes` command is executed and its output is displayed.
4. **Given** a handler fails (e.g., command not found or non-zero exit code), **When** the command is executed, **Then** the error output is displayed to the user and the tool does not crash.
5. **Given** a handler completes successfully or with an error, **When** execution finishes, **Then** the user is returned to the Root submenu.

---

### User Story 3 — Add a New Root Item with Minimal Changes (Priority: P2)

A developer extending the tool can add a new root scaffold item (e.g., ".editorconfig") by editing only the Root item registry and optionally registering a handler. No changes to menu rendering, navigation, or existing handlers are required.

**Why this priority**: Validates the extensibility design. Without this, the feature is a closed system. P2 because the first two stories deliver immediate value; this is a quality-of-design validation.

**Independent Test**: Can be verified by code review: adding a new entry to the Root registry requires changes to exactly one file beyond the handler implementation itself.

**Acceptance Scenarios**:

1. **Given** the Root item registry, **When** a developer adds a new `RootItem` entry with a display name and handler, **Then** the item appears in the Root submenu with zero changes to navigation or rendering code.
2. **Given** an existing handler type (e.g., `DotnetNewHandler`), **When** it is reused for a new item with different command arguments, **Then** no new handler class is required.

---

### Edge Cases

- What happens when `dotnet new gitignore` is run and a `.gitignore` already exists? The command output (warning or overwrite prompt) is passed through to the user as-is.
- What happens if the working directory is not a valid project root? The command output is surfaced; the tool does not crash.
- How does the system handle a handler that takes a long time to execute? Output is streamed or displayed after completion; the tool remains responsive.
- What if the Root submenu has only one non-back item? The menu still renders correctly with a single item and the back option.

---

## Clarifications

### Session 2026-05-18

- Q: After selecting a Root submenu item, does the user see an intermediate menu before the command runs? → A: Yes — a handler selection menu is shown listing all handlers registered for that item (each with a display name), plus "<- Back". The user picks a handler, which then executes. A single Root item (e.g. ".gitignore") can have multiple handlers registered and more may be added later.
- Q: Should `HandlerContext` be a new standalone type or reuse `NavigationContext`? → A: New standalone type. `HandlerContext` has its own `GoBack()` signal and carries a `WorkingDirectory` property. No dependency on `NavigationContext`.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: When the user selects "Root" from the top-level menu, a submenu MUST be displayed listing all registered root scaffold items plus a "<- Back" entry as the last item.
- **FR-002**: The Root submenu MUST include ".gitignore" and ".gitattributes" as the initial items, in that order, above the "<- Back" entry.
- **FR-003**: Selecting "<- Back" from the Root submenu MUST return the user to the top-level menu without restarting the process.
- **FR-004**: Each Root submenu item MUST be associated with one or more `IScaffoldHandler` implementations. When the user selects a Root item, a handler selection menu MUST be displayed listing all registered handlers for that item (by their `DisplayName`) plus a "<- Back" entry as the last item.
- **FR-005**: The `IScaffoldHandler` interface MUST expose a `DisplayName` property (shown in the handler selection menu) and support at minimum two execution modes: running an external command with configurable name and arguments, and prompting the user for free-text input.
- **FR-004b**: Selecting "<- Back" from the handler selection menu MUST return the user to the Root submenu.
- **FR-004c**: `DotnetNewHandler` MUST have a `DisplayName` (e.g., "dotnet new") shown in the handler selection menu. Future handlers for the same item (e.g., a manual file creation handler) will appear alongside it.
- **FR-006**: A `DotnetNewHandler` MUST be providedthat executes `dotnet new <templateName>` with a configurable template name and optional additional arguments.
- **FR-007**: `DotnetNewHandler` MUST be registered as the handler for both ".gitignore" (template: `gitignore`) and ".gitattributes" (template: `gitattributes`).
- **FR-008**: Handler execution output (stdout and stderr) MUST be displayed to the user during or after execution.
- **FR-009**: If a handler's command exits with a non-zero code, the error output MUST be shown and the tool MUST return the user to the Root submenu rather than crashing.
- **FR-010**: After a handler completes (success or failure), the user MUST be returned to the Root submenu.
- **FR-011**: Adding a new Root scaffold item MUST require changes to only the Root item registry (and a new handler class if a new handler type is needed). No changes to menu rendering, navigation loop, or existing handlers are permitted.
- **FR-012**: The Root submenu item list MUST be independently extensible — a developer MUST be able to add, remove, or reorder items by editing a single registration location.

### Key Entities

- **RootItem**: A named scaffold item shown in the Root submenu. Has a `DisplayName` and an ordered list of one or more `IScaffoldHandler` instances. When selected, shows the handler selection menu. Analogous to `Category` at the top level but scoped to Root.
- **IRootItemRegistry**: Source of truth for all Root submenu items. Returns an ordered `IReadOnlyList<RootItem>`.
- **IScaffoldHandler**: Abstraction for the action executed when a Root item is selected. Supports command execution and user input collection. Receives the console and a context object.
- **DotnetNewHandler**: Concrete `IScaffoldHandler` that runs `dotnet new <templateName> [args...]`. Template name and extra arguments are configured at registration time.
- **HandlerContext**: Standalone per-invocation state passed to `IScaffoldHandler.ExecuteAsync`. Has its own `GoBack()` signal (sets `ShouldGoBack = true`) and a `WorkingDirectory` string property (set to the process working directory at launch). No dependency on `NavigationContext`.
- **CommandResult**: Captures exit code, stdout, and stderr from an external command execution. Used by `DotnetNewHandler` internally.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can navigate from the top-level menu into the Root submenu and back to the top-level menu in under 3 seconds of interaction time.
- **SC-002**: Selecting ".gitignore" or ".gitattributes" causes the corresponding `dotnet new` command to complete and its output to be visible within the time the command itself takes plus under 500 ms overhead.
- **SC-003**: Adding a new Root scaffold item using an existing handler type requires editing exactly one file (the Root item registry) beyond adding the handler itself.
- **SC-004**: 100% of handler errors (non-zero exit codes) are surfaced to the user without the tool crashing or requiring restart.

---

## Assumptions

- `HandlerContext` carries the working directory (set to the process launch directory). Handlers use this to determine where scaffold files are created.
- `dotnet` is available on the user's `PATH`; the tool does not verify this before invoking the command (error output handles the case where it is not).
- After a handler completes, the user is returned to the Root submenu (not the top-level menu), enabling them to run another root item without re-navigating.
- The Root submenu uses the same Spectre.Console `SelectionPrompt` mechanism already in use for the top-level menu — no new UI primitives are needed.
- `IScaffoldHandler` does not need to support async user-input prompts (e.g., file name input) in this iteration; that is left as a future extension point but the interface MUST accommodate it without breaking changes.
- The `DotnetNewHandler` streams or displays command output after the process exits; live streaming is out of scope for this iteration.
- The top-level `StubMenuAction` for "Root" will be replaced by a real `RootMenuAction` that renders the Root submenu — this is the primary change to existing code.
