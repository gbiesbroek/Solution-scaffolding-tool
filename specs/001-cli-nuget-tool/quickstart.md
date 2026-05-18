# Quickstart: scaffold CLI Tool

**Feature**: 001-cli-nuget-tool

---

## Prerequisites

- .NET 10 SDK installed (`dotnet --version` should show `10.x.x`)
- Repository cloned

---

## 1. Register the Local NuGet Feed (once per machine)

The NuGet feed lives at `~/nuget-local` (next to the `.nuget` folder in your home directory).

```bash
# macOS / Linux
dotnet nuget add source "$HOME/nuget-local" --name scaffold-local
```

On Windows (PowerShell):
```powershell
dotnet nuget add source "$HOME\nuget-local" --name scaffold-local
```

Create the directory first if it does not exist:
```bash
mkdir -p ~/nuget-local          # macOS / Linux
```
```powershell
New-Item -ItemType Directory -Force "$HOME\nuget-local"   # Windows
```

Verify it was registered:
```bash
dotnet nuget list source
# scaffold-local [Enabled] ~/nuget-local  ← should appear
```

---

## 2. Build & Pack the Tool

```bash
dotnet pack src/Scaffold.Cli/ -o ~/nuget-local/
```

On Windows (PowerShell):
```powershell
dotnet pack src/Scaffold.Cli/ -o "$HOME\nuget-local"
```

You should see `~/nuget-local/Scaffold.Cli.1.0.0.nupkg` created.

---

## 3. Install the Global Tool

```bash
dotnet tool install --global Scaffold.Cli
```

Verify installation:
```bash
scaffold
```

---

## 4. Run the Tool

```bash
scaffold
```

Expected output:
```
Press Enter to exit...
```

Type anything and press Enter — the tool exits with code 0.

---

## 5. Run the End-to-End Tests

```bash
dotnet test tests/Scaffold.Cli.EndToEnd/
```

All tests should pass.

---

## Updating the Tool

When you make changes to the source:

```powershell
# Uninstall old version
dotnet tool uninstall --global Scaffold.Cli

# Bump version in src/Scaffold.Cli/Scaffold.Cli.csproj if needed
# Repack and reinstall
dotnet pack src/Scaffold.Cli/ -o "$HOME\nuget-local"
dotnet tool install --global Scaffold.Cli
```

---

## Removing the Local Feed Registration

```bash
dotnet nuget remove source scaffold-local
```
