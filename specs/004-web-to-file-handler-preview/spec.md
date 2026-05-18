# Feature Specification: WebToFileHandler and Handler Preview Strings

**Feature Branch**: `004-web-to-file-handler-preview`  
**Created**: 2026-05-18  
**Status**: Draft  
**Input**: User description: "Create another handler, a WebToFileHandler that retrieves a webpage and stores it as a file. Add a concrete one for 'gitignore' with DisplayName 'Github/Dotnet/Core/.gitignore', which retrieves the raw file from 'https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore' and saves it as '.gitignore' in the working directory. To both DotnetNewHandler and WebToFileHandler add a preview string with a description of what will be executed. Check if the file already exists and handle appropriately (notify it exists, ask to overwrite)."

---

## Clarifications

### Session 2026-05-18

- Q: How is the `Preview` string displayed in the handler selection menu? → A: Preview is shown only after the user selects a handler — not in the list. After selection the preview is displayed and the user confirms or goes back to the handler list.
- Q: Where does the overwrite check responsibility live? → A: A shared utility/abstraction (e.g., `IFileSystem`) is injected into each handler — overwrite logic is implemented once and reused by all handlers.
- Q: What is the HTTP request timeout for `WebToFileHandler`? → A: Fixed 10-second timeout — request fails with a timeout error message if exceeded.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Download a File from the Web to the Working Directory (Priority: P1)

A developer selects "Root" → ".gitignore" from the scaffold tool. The handler selection menu now includes an entry "Github/Dotnet/Core/.gitignore" alongside the existing "dotnet new" handler. The developer selects it and the tool downloads the `.gitignore` from the dotnet/core GitHub repository and writes it to the current working directory. The developer sees confirmation and is returned to the Root submenu.

**Why this priority**: Introduces a second, distinct handler type — the core deliverable of this feature. Without this, the feature has no value.

**Independent Test**: Can be tested by selecting Root → .gitignore → "Github/Dotnet/Core/.gitignore" and verifying that `.gitignore` is created in the working directory with content matching the file at the configured URL.

**Acceptance Scenarios**:

1. **Given** the handler selection menu for ".gitignore" is visible, **When** the user selects "Github/Dotnet/Core/.gitignore", **Then** the handler fetches the file from `https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore` and saves it as `.gitignore` in the working directory.
2. **Given** the download completes successfully, **When** the file is written, **Then** the user sees a confirmation message and is returned to the Root submenu.
3. **Given** the remote URL returns an error or is unreachable, **When** the handler attempts to download, **Then** an error message is displayed and the tool returns to the Root submenu without crashing.
4. **Given** a new `WebToFileHandler` with a different URL and output file name is registered, **When** the user selects it, **Then** the correct file is downloaded and saved under the correct file name — with no changes to navigation or rendering code.

---

### User Story 2 — Review a Handler Preview Before Confirming Execution (Priority: P1)

A developer opens the handler selection menu for a Root item (e.g., ".gitignore"). The menu lists handler names only. The developer selects a handler; the tool then displays the handler's preview string (describing what it will do) and asks the developer to confirm or go back to the handler list. Only after confirmation does the handler execute.

**Why this priority**: Without the preview confirmation step, selecting between handlers (e.g., "dotnet new" vs "Github/Dotnet/Core/.gitignore") is a blind action. The confirm/back step is core to the informed selection UX.

**Independent Test**: Can be tested by selecting a handler and verifying: (1) a preview string is displayed before execution, (2) declining returns to the handler list without executing, (3) confirming executes the handler.

**Acceptance Scenarios**:

1. **Given** the handler selection menu for ".gitignore" is visible, **When** the user selects any handler, **Then** a preview screen is shown displaying the handler's `Preview` string, with options to confirm or go back.
2. **Given** the preview screen is shown for `DotnetNewHandler` (template: `gitignore`), **When** the preview is displayed, **Then** it reads something like `"Runs: dotnet new gitignore"`.
3. **Given** the preview screen is shown for `WebToFileHandler` (URL: `https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore`), **When** the preview is displayed, **Then** it reads something like `"Downloads from: https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore → .gitignore"`.
4. **Given** the preview screen is shown, **When** the user selects "Go back", **Then** the handler selection menu is re-displayed and nothing is executed.
5. **Given** a future custom handler type, **When** it implements `IScaffoldHandler`, **Then** it is required to provide a `Preview` property — the interface enforces this.

---

### User Story 3 — Handle Overwrite Confirmation When a File Already Exists (Priority: P1)

A developer runs the scaffold tool in a directory where `.gitignore` already exists. They select Root → ".gitignore" → a handler. The tool detects the existing file and informs the developer, then asks whether to overwrite. If they confirm, the file is replaced. If they decline, no file is written and they are returned to the Root submenu.

**Why this priority**: Preventing silent overwrites is a data-safety concern. This applies to both handler types and is required for responsible default behaviour.

**Independent Test**: Can be tested by pre-creating a `.gitignore` in the working directory, running either handler, and verifying the overwrite prompt appears and both "overwrite" and "skip" paths behave correctly.

**Acceptance Scenarios**:

1. **Given** a `.gitignore` already exists in the working directory, **When** either handler for ".gitignore" is selected, **Then** the user is notified that the file exists before the action runs.
2. **Given** the overwrite prompt is shown, **When** the user confirms overwrite, **Then** the file is replaced with the new content.
3. **Given** the overwrite prompt is shown, **When** the user declines overwrite, **Then** the file is left unchanged and the user is returned to the Root submenu without an error.
4. **Given** the target file does not exist, **When** a handler runs, **Then** no overwrite prompt is shown and the file is created directly.
5. **Given** overwrite is declined for ".gitignore", **When** the user returns to the Root submenu and selects another item (e.g., ".gitattributes"), **Then** the tool operates normally.

---

### Edge Cases

- What happens if the network request returns a non-200 status code? The handler displays the HTTP status and error body, then returns to the Root submenu without writing any file.
- What happens if the downloaded content is empty? The handler notifies the user and does not write an empty file; it returns to the Root submenu.
- What if the working directory is read-only? Writing fails; the handler displays the error and returns to the Root submenu without crashing.
- What if the user's network is offline or the request exceeds the 10-second timeout? The handler fails with a timeout/connection error message and returns to the Root submenu.
- What happens when `.gitattributes` already exists and the user runs its handler? The same overwrite flow applies — the handler checks for file existence regardless of the file name.
- What if a `DotnetNewHandler` produces a file that already exists (e.g., `dotnet new gitignore` when `.gitignore` is present)? The tool checks for the output file before running the command. If the file exists, the overwrite prompt is shown. If the user confirms, the command runs (which may itself overwrite or fail depending on `dotnet`'s behaviour); if the user declines, the command is not run.
- What if the user selects "Go back" from the preview confirmation screen? The handler selection menu for the current root item is re-displayed; no action is taken.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `IScaffoldHandler` MUST be extended with a `Preview` property (type `string`) that returns a human-readable description of the action the handler will perform.
- **FR-002**: The handler selection menu MUST list handlers by `DisplayName` only. After the user selects a handler, the handler's `Preview` string MUST be displayed on a confirmation screen. The user MUST be able to confirm execution or go back to the handler selection menu from this screen.
- **FR-003**: `DotnetNewHandler` MUST provide a `Preview` string in the format `"Runs: dotnet new <templateName> [extraArgs]"` describing the command that will be executed.
- **FR-004**: A `WebToFileHandler` MUST be created that downloads a file from a configurable URL and saves it to a configurable output file name in the current working directory.
- **FR-005**: `WebToFileHandler` MUST expose a configurable `DisplayName` set at registration time.
- **FR-006**: `WebToFileHandler` MUST provide a `Preview` string in the format `"Downloads from: <url> → <outputFileName>"`.
- **FR-007**: A concrete `WebToFileHandler` instance MUST be registered for the ".gitignore" root item with DisplayName `"Github/Dotnet/Core/.gitignore"`, source URL `https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore`, and output file name `.gitignore`.
- **FR-008**: The `WebToFileHandler` for ".gitignore" MUST be registered in addition to (not replacing) the existing `DotnetNewHandler`.
- **FR-009**: Before any handler writes a file, a shared file-system abstraction (injected into the handler) MUST check whether the target output file already exists in the working directory.
- **FR-010**: If the target file already exists, the system MUST notify the user (displaying the file name) and prompt for overwrite confirmation before proceeding.
- **FR-011**: If the user confirms overwrite, the handler MUST proceed with its action (running the command or writing the downloaded content), replacing the existing file.
- **FR-012**: If the user declines overwrite, the handler MUST take no action and return the user to the Root submenu.
- **FR-013**: If the target file does not exist, the handler MUST proceed directly without an overwrite prompt.
- **FR-014**: `WebToFileHandler` MUST apply a 10-second timeout to the HTTP request. Network errors (timeout, unreachable host, non-2xx response, empty body) MUST be surfaced to the user as a clear message; the tool MUST return to the Root submenu without crashing.
- **FR-015**: Adding a new `WebToFileHandler` instance for a different file MUST require changes only to the Root item registry. No changes to menu rendering, navigation, or existing handlers are required.

### Key Entities

- **WebToFileHandler**: Concrete `IScaffoldHandler` that downloads a file from a URL and saves it to the working directory under a specified file name. Has a configurable `DisplayName`, source URL, and output file name. Implements the `Preview` property. Receives a shared file-system abstraction for existence checks and file writes.
- **IScaffoldHandler.Preview**: New required property on `IScaffoldHandler`. Returns a short human-readable string describing what the handler will do. Displayed to the user on a confirmation screen after the handler is selected from the handler list, before execution begins.
- **IFileSystem** (shared abstraction): Injected into handlers that write files. Exposes at minimum: `FileExists(path)` and `WriteAllText(path, content)`. Enables testing overwrite logic without touching the real filesystem. A single implementation serves both `DotnetNewHandler` and `WebToFileHandler`.
- **Overwrite check**: Implemented once inside `IFileSystem`-aware handler logic — calls `IFileSystem.FileExists` before writing, prompts via the console when the file is present. Not duplicated per handler.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can select Root → ".gitignore" → "Github/Dotnet/Core/.gitignore" and have the file written to their working directory in under 5 seconds on a typical broadband connection.
- **SC-002**: 100% of handler actions that would overwrite an existing file present an overwrite prompt — no silent overwrites occur.
- **SC-003**: 100% of `WebToFileHandler` network failures (timeout after 10 seconds, unreachable host, non-2xx status) are surfaced with an error message; the tool never crashes or blocks longer than 10 seconds waiting for a response.
- **SC-004**: After selecting any handler, the developer sees the handler's preview text before execution begins, and can go back to the handler list without triggering any side effects.
- **SC-005**: Adding a new `WebToFileHandler` for a different URL and file name requires editing exactly one file (the Root item registry).

---

## Assumptions

- The working directory used by handlers is the directory from which the `scaffold` tool was invoked (already represented by `HandlerContext.WorkingDirectory`).
- Internet connectivity is required for `WebToFileHandler`; the tool does not cache previously downloaded files between invocations.
- HTTP redirects are followed automatically; no custom redirect handling is needed.
- The overwrite check is performed via a shared `IFileSystem` abstraction injected into each handler. Both `DotnetNewHandler` and `WebToFileHandler` receive this abstraction; overwrite logic is implemented once and not duplicated.
- `DotnetNewHandler` determines its output file name from a configurable property set at registration time. When `.gitignore` is the known output, the overwrite check fires for that file before running `dotnet new`.
- The `Preview` property does not need to be computed dynamically or await any I/O — it is a static descriptive string set at construction time.
- After the user selects a handler from the list, `RootMenuAction` displays the handler's `Preview` and presents a confirm/back prompt before invoking `ExecuteAsync`. This confirmation step is implemented within `RootMenuAction`; no new menu component is introduced.
- The `WebToFileHandler` uses the standard .NET HTTP client; no third-party HTTP libraries are introduced.
- Live download progress indicators are out of scope for this iteration.
