# Quickstart: WebToFileHandler and Handler Preview Strings

**Feature**: 004-web-to-file-handler-preview | **Branch**: `004-web-to-file-handler-preview`

---

## What This Feature Adds

Feature 004 builds on the Root submenu (feature 003) and adds:

1. **`WebToFileHandler`** — a second handler type that downloads a file from a URL and saves it to the working directory.
2. **Handler preview** — after selecting a handler, the user sees a description of what will happen before confirming.
3. **Overwrite protection** — if the target file already exists, the user is asked before anything is written.

---

## Running the Tool

```powershell
# From any directory (scaffold tool must be installed):
scaffold
```

Navigate: Root → `.gitignore` → select a handler → review preview → confirm.

---

## Handler Selection Flow (updated)

```
? Root scaffolding                        ← Root item menu
> .gitignore
  .gitattributes
  <- Back

? How would you like to scaffold .gitignore?    ← Handler list (DisplayName only)
> dotnet new
  Github/Dotnet/Core/.gitignore
  <- Back

Runs: dotnet new gitignore               ← Preview for the `dotnet new` handler
# or:
Downloads from: https://raw.githubusercontent.com/dotnet/core/refs/heads/main/.gitignore → .gitignore

? Proceed?                                ← Preview confirmation
> Confirm
  <- Back
```

Selecting `"<- Back"` at any step navigates one level up:
- From confirm → handler list
- From handler list → Root item menu
- From Root item menu → top-level menu

---

## File Overwrite Flow

If `.gitignore` already exists in the current directory:

```
File '.gitignore' already exists.

? How would you like to proceed?
> Overwrite
  <- Back
```

- **Overwrite** → the new file replaces the existing one.
- **`<- Back`** → nothing is written; returns to Root item menu.

---

## Adding a New `WebToFileHandler`

1. Open `src/Scaffold.Cli/Root/RootItemRegistry.cs`.
2. Add a new `WebToFileHandler` instance to the appropriate `RootItem`'s handler list:

```csharp
new WebToFileHandler(
    "My Source/.gitignore",
    "https://example.com/raw/.gitignore",
    ".gitignore",
    downloader,
    fileSystem)
```

3. That's it — no changes to navigation, rendering, or existing handlers.

---

## Adding a New `DotnetNewHandler` with Overwrite Support

```csharp
new DotnetNewHandler(
    "editorconfig",     // template name
    commandRunner,
    fileSystem,
    ".editorconfig",    // output file name (used for overwrite check)
    null                // optional extra args
)
```

---

## Running Tests

```powershell
dotnet test --project tests/Scaffold.Cli.Tests/Scaffold.Cli.Tests.csproj
dotnet test --project tests/Scaffold.Cli.EndToEnd/Scaffold.Cli.EndToEnd.csproj
```

---

## Repacking and Reinstalling the Tool

```powershell
dotnet tool uninstall Scaffold.Cli --global
dotnet pack src/Scaffold.Cli/ -o "$HOME\nuget-local" --configuration Release
dotnet tool install Scaffold.Cli --global --add-source "$HOME\nuget-local"
```
