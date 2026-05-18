# Research: Interactive CLI NuGet Tool

**Feature**: 001-cli-nuget-tool
**Phase**: 0 — Research & Decision Log

---

## 1. .NET Target Version

**Decision**: .NET 9.0

**Rationale**: .NET 9 is the current standard release as of the feature date (2026-05-18).
It offers the latest C# language features and long-term platform support for developer
tooling. Both .NET 8 (LTS) and .NET 9 are viable; .NET 9 is preferred as the more current
option for a new project.

**Alternatives considered**:
- .NET 8 — still an LTS release and fully supported; choose this if the target machine
  has only .NET 8 installed.

---

## 2. .NET Global Tool Packaging

**Decision**: Use `PackAsTool = true` with `ToolCommandName = scaffold` in the `.csproj`.

**Rationale**: The standard .NET global tool mechanism requires only three extra project
properties (`PackAsTool`, `ToolCommandName`, `PackageId`). No third-party tooling is
needed. The tool is packed with `dotnet pack` and installed with
`dotnet tool install --global scaffold`.

**Key `.csproj` properties**:
```xml
<PackAsTool>true</PackAsTool>
<ToolCommandName>scaffold</ToolCommandName>
<PackageId>Scaffold.Cli</PackageId>
<Version>1.0.0</Version>
```

**Alternatives considered**:
- Single-file executable published to PATH — skips NuGet entirely; rejected because the
  spec explicitly requires the NuGet global tool pattern.

---

## 3. Local NuGet Feed — Directory + Registration

**Decision**: Use `/nuget-local` at repository root as the feed directory.
Register it per-user with `dotnet nuget add source`.

**Rationale**: A flat directory feed is the simplest NuGet source type — no server
required. `dotnet nuget add source` writes a named entry into the user-level
`NuGet.Config` (`~/.nuget/NuGet/NuGet.Config` on Linux/macOS,
`%APPDATA%\NuGet\NuGet.Config` on Windows), making it available to all .NET CLI
commands on the machine without modifying any repository-committed config.

**Workflow**:
```bash
# 1. Register feed (once per machine)
dotnet nuget add source <absolute-path>/nuget-local --name scaffold-local

# 2. Pack
dotnet pack src/Scaffold.Cli/ -o nuget-local/

# 3. Install global tool
dotnet tool install --global scaffold --add-source ./nuget-local
# or simply: dotnet tool install --global scaffold  (if source is already registered)
```

**Notes**:
- `nuget-local/` should be listed in `.gitignore` (or only ignore `*.nupkg` inside it).
- Reinstalling requires uninstalling first:
  `dotnet tool uninstall --global scaffold && dotnet tool install --global scaffold`

**Alternatives considered**:
- Commit `nuget.config` at repo root — rejected; would affect all contributors and
  could conflict with CI feeds.
- Use `~/.nuget/packages` directly — rejected per spec amendment (Q3).

---

## 4. E2E Test Strategy — Process Launch

**Decision**: E2E tests launch the `scaffold` executable directly from the build output
path, not via the installed global tool.

**Rationale**: Launching from the build output (`dotnet run` or direct binary path) means
tests do not depend on a global tool install step, making them portable and CI-friendly.
The binary path is resolved via MSBuild properties or a known relative output path at
test time.

**Approach**:
```csharp
var exePath = Path.Combine(
    AppContext.BaseDirectory,
    "..", "..", "..", "..", "..",
    "src", "Scaffold.Cli", "bin", "Debug", "net9.0", "Scaffold.Cli"
);
var process = new Process { StartInfo = new ProcessStartInfo(exePath) { ... } };
```

Or use `dotnet run --project src/Scaffold.Cli` as the process command for simplicity.

**Alternatives considered**:
- Test via installed global tool (`scaffold`) — depends on install being done first;
  fragile in CI without a pre-install step.

---

## 5. xunit + Microsoft.Testing.Platform Integration

**Decision**: Use xunit v2 with `Microsoft.Testing.Platform` via
`Microsoft.Testing.Platform.MSBuild` and `xunit.runner.visualstudio`.

**Rationale**: xunit v2 is the most widely used and documented .NET test framework.
`Microsoft.Testing.Platform` (MTP) provides a modern, fast test runner that integrates
with `dotnet test` and VS/VS Code. The `xunit.runner.visualstudio` adapter bridges
xunit v2 to MTP.

**Package set**:
```xml
<PackageReference Include="Microsoft.Testing.Platform.MSBuild" Version="1.*" />
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
```

**Test execution**: `dotnet test tests/Scaffold.Cli.EndToEnd/`

**Alternatives considered**:
- xunit v3 (native MTP support) — available but less mature ecosystem as of feature
  date; can be adopted in a future iteration.
- NUnit / MSTest — rejected; spec explicitly requires xunit.

---

## 6. Interactive Stdin/Stdout Pattern in C#

**Decision**: Use `Console.WriteLine` for the prompt and `Console.ReadLine()` for input.

**Rationale**: `Console.ReadLine()` blocks until the user presses Enter (or stdin is
closed), which is exactly the interactive behavior required. This is the simplest correct
implementation — no additional libraries needed.

```csharp
Console.WriteLine("Press Enter to exit...");
Console.ReadLine();
```

**Edge case handling**:
- Empty line (just Enter): `ReadLine()` returns `""` — triggers exit. ✅
- EOF / closed stdin: `ReadLine()` returns `null` — also triggers exit. ✅
  (No special handling needed per spec.)

---

## Summary of Decisions

| Area | Decision |
|------|----------|
| .NET version | .NET 9.0 |
| Tool command | `scaffold` |
| Package ID | `Scaffold.Cli` |
| NuGet feed | `/nuget-local` dir, registered via `dotnet nuget add source` |
| E2E binary | Launched from build output path |
| Test framework | xunit v2 + Microsoft.Testing.Platform |
| Stdin pattern | `Console.ReadLine()` blocking call |
