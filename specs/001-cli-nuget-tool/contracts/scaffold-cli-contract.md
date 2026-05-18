# CLI Contract: `scaffold`

**Feature**: 001-cli-nuget-tool
**Phase**: 1 — Design

---

## Invocation

```
scaffold
```

The tool takes **no arguments and no options**. Any command-line arguments passed are
silently ignored in this version.

---

## Stdin Contract

| Aspect | Specification |
|--------|---------------|
| Mode | Interactive — reads exactly **one line** from stdin |
| Trigger | Any text followed by Enter, or an empty line (just Enter) |
| EOF handling | Receiving EOF (closed stdin) is treated as valid input — tool exits |
| Encoding | UTF-8 (platform default) |

---

## Stdout Contract

| Aspect | Specification |
|--------|---------------|
| Prompt message | `Press Enter to exit...` followed by a newline (`\n`) |
| Timing | Written **before** stdin is read |
| Additional output | None in this version |
| Encoding | UTF-8 (platform default) |

**Exact stdout output** (before receiving input):
```
Press Enter to exit...
```

---

## Stderr Contract

| Aspect | Specification |
|--------|---------------|
| Normal operation | No output to stderr |
| Unhandled exceptions | .NET runtime default — stack trace to stderr; exit code non-zero |

---

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Clean exit — input was received (any line including empty) or EOF |
| non-zero | Unhandled exception (runtime error) |

---

## Timing Guarantees

| Scenario | Maximum Duration |
|----------|-----------------|
| From launch to prompt appearing on stdout | < 1 second |
| From input received to process exit | < 5 seconds (SC-002) |
| Process remains alive with no input | ≥ 10 seconds (SC-003) |

---

## NuGet Package Contract

| Property | Value |
|----------|-------|
| `PackageId` | `Scaffold.Cli` |
| `ToolCommandName` | `scaffold` |
| `Version` | `1.0.0` |
| `TargetFramework` | `net9.0` |
| `PackAsTool` | `true` |

---

## Local Feed Registration Contract

The tool is distributed via a local NuGet feed directory. The following must hold for
`dotnet tool install --global scaffold` to succeed:

1. `~/nuget-local/` directory exists in the user's home directory.
2. The directory is registered as a named source in the user's NuGet config:
   ```
   Source name: scaffold-local
   Source path: ~/nuget-local
   ```
3. At least one `Scaffold.Cli.*.nupkg` file is present in the directory.
