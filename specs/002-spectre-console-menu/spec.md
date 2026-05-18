# Feature Specification: Spectre.Console Interactive Category Menu

**Feature Branch**: `002-spectre-console-menu`
**Created**: 2026-05-18
**Status**: Draft
**Input**: User description: "Add spectre.console. Present the user with a list of toplevel categories to choose from. Categories: Aspire, Frontend, Core, Root, Exit. More can be added later and items will get more properties. Each choice has a follow-up action. Exit exits the program. Others will implement follow-up choices later. Have interfaces to cover future scenarios. Option to move back to previous/top-level."

---

## Clarifications

### Session 2026-05-18

- Q: What mechanism triggers "back" navigation — a dedicated menu item or the Escape key? → A: A dedicated **"← Back"** entry at the bottom of every sub-context menu.
- Q: At the top-level menu, what happens when the user has no further back option — ignore, or prompt to exit? → A: **Exit** is the only way to quit; no implicit back action or Escape handling at top-level.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Navigate Top-Level Category Menu (Priority: P1)

A developer runs `scaffold` and is presented with an interactive menu listing the top-level
categories: **Aspire**, **Frontend**, **Core**, **Root**, and **Exit**. They can use arrow
keys to move the selection and press Enter to confirm. The menu is clearly rendered and
responds to keyboard navigation without requiring the user to type raw commands.

**Why this priority**: This is the entire interactive surface of the tool at this stage.
Without the menu, the tool has no useful functionality.

**Independent Test**: Running `scaffold` displays a menu with the 5 categories. The user
can navigate and select **Exit** to quit the program cleanly.

**Acceptance Scenarios**:

1. **Given** the tool is launched, **When** the menu is rendered, **Then** all five category
   entries (Aspire, Frontend, Core, Root, Exit) are visible and one entry is highlighted/selected.
2. **Given** the menu is displayed, **When** the user presses the down/up arrow, **Then** the
   selection moves to the adjacent item and wraps around at the boundaries.
3. **Given** the menu is displayed, **When** the user selects **Exit** and presses Enter,
   **Then** the program exits cleanly with exit code 0.
4. **Given** the menu is displayed, **When** the user selects any non-Exit category and presses
   Enter, **Then** a follow-up sub-menu or placeholder screen is shown (future implementation
   hook is invoked).

---

### User Story 2 — Navigate Back to Top-Level from Sub-Context (Priority: P2)

After selecting a non-Exit category and entering a sub-context (or placeholder), the user
can navigate back to the top-level category menu without restarting the program.

**Why this priority**: The "back" navigation is essential for a usable interactive tool.
Without it the user is trapped in a sub-context with no escape route except Ctrl+C.

**Independent Test**: Selecting any non-Exit category and then invoking the back action
returns the user to the top-level category menu. The state of the top-level menu is reset
to the default selection.

**Acceptance Scenarios**:

1. **Given** the user has selected a category (e.g., Core) and is in its sub-context,
   **When** the user selects the **"← Back"** entry at the bottom of the sub-context menu,
   **Then** the top-level category menu is displayed again.
2. **Given** the user is already at the top-level menu, **When** there is no "← Back" entry
   present, **Then** the only way to exit is by selecting **Exit** from the menu.

---

### User Story 3 — Extensible Category Definition (Priority: P3)

A developer can add a new category (e.g., "Infrastructure") to the list without touching the
menu rendering or navigation logic — only the category data needs to be updated.

**Why this priority**: The spec explicitly states that more categories will be added later.
The design must accommodate extension without requiring structural code changes.

**Independent Test**: Adding a new category entry to the category list causes it to appear
in the menu automatically, with no other code changes required.

**Acceptance Scenarios**:

1. **Given** a new category is added to the category registry, **When** the menu is displayed,
   **Then** the new category appears in the list alongside the existing ones.
2. **Given** a category has additional properties beyond its display name, **When** the menu
   renders it, **Then** those properties are accessible to the follow-up action handler without
   changes to the menu infrastructure.

---

### Edge Cases

- What happens when the menu has only one item (e.g., Exit only)? Navigation should still work without wrapping errors.
- What happens if a category's follow-up action throws an unhandled exception? The tool should not crash; a fallback to the top-level menu is acceptable.
- What happens if the terminal does not support interactive rendering (e.g., piped stdout)? The tool should handle this gracefully (Spectre.Console detects non-interactive terminals automatically).
- What happens when the user presses Ctrl+C at any level? The tool should exit cleanly (exit code 0 or standard interrupt code).

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The CLI tool MUST replace the current plain-text prompt with an interactive
  category selection menu rendered using Spectre.Console.
- **FR-002**: The menu MUST display the following initial top-level categories: **Aspire**,
  **Frontend**, **Core**, **Root**, and **Exit**.
- **FR-003**: The menu MUST support keyboard navigation (up/down arrow keys, Enter to select).
- **FR-004**: Selecting **Exit** MUST terminate the program with exit code 0.
- **FR-005**: Selecting any non-Exit category MUST invoke that category's associated follow-up
  action handler, which will be implemented in future iterations.
- **FR-006**: The category model MUST support additional properties beyond a display name, so
  that future iterations can attach metadata (e.g., description, icon, configuration) without
  changing the menu infrastructure.
- **FR-007**: The tool MUST expose an abstraction (interface or equivalent contract) that each
  category's follow-up action implements, enabling new categories to plug in without modifying
  core menu logic.
- **FR-008**: Every sub-context menu MUST include a **"← Back"** entry as the last item in
  the list. Selecting it returns the user to the parent context (or top-level menu if there
  is no deeper nesting). The top-level category menu does NOT include a "← Back" entry.
- **FR-009**: The category list MUST be defined in a single, dedicated location so that adding
  a new category requires changing only that location.
- **FR-010**: The tool MUST continue to exit cleanly (exit code 0) in all expected flows.

### Key Entities

- **Category**: A named menu entry with a display name and a reference to its follow-up action
  handler. Designed to carry additional properties in future iterations (e.g., description,
  tags, order). Current instances: Aspire, Frontend, Core, Root, Exit.
- **IMenuAction**: An abstraction representing the follow-up behaviour triggered when a
  category is selected. At minimum exposes an `Execute` method that receives navigation context.
  Exit is a built-in implementation; others are stubs for future development.
- **NavigationContext**: Carries the state of the current menu traversal — specifically the
  ability to navigate back to the parent or top-level menu. Passed into `IMenuAction.Execute`.
- **MenuRenderer**: Responsible for displaying the category list, handling keyboard input, and
  returning the user's selection. Delegates all rendering to Spectre.Console.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can launch the tool and reach the top-level category menu in under
  1 second on a standard developer workstation.
- **SC-002**: All five initial categories are visible in a single menu render without scrolling.
- **SC-003**: Selecting Exit from the top-level menu terminates the tool in under 500 ms.
- **SC-004**: Adding a new category to the registry causes it to appear in the menu with zero
  changes to menu rendering or navigation code — verified by code review of the diff.
- **SC-005**: The "back" navigation from any non-top-level context returns the user to the
  top-level menu without restarting the process.
- **SC-006**: The automated E2E test suite passes 100% after implementation.

---

## Assumptions

- Spectre.Console's built-in `SelectionPrompt` (or equivalent) is used for the interactive
  menu; no custom terminal rendering is needed for v1.
- "Back" navigation does not exist at the top-level menu — the top-level has no "← Back" entry.
  The **Exit** item is the sole way to terminate the program from the top-level menu.
- Follow-up actions for Aspire, Frontend, Core, and Root are stub implementations for this
  iteration — they display a placeholder message and immediately offer the back option.
- The tool runs in an interactive terminal; non-interactive environments (CI pipelines, piped
  input) are out of scope for this feature.
- Category ordering in the menu matches the order they are defined in the registry.
- No persistence of the user's last selection is required in v1.
- The existing E2E test infrastructure (xunit v3, MTP) will be extended to cover the new
  interactive menu behaviour.
