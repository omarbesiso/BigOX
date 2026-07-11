# Contributing to BigO X

Thanks for your interest in improving **BigO X**. This document is the canonical reference for
building, testing, and submitting changes.

## Prerequisites

- The **.NET 10 SDK** (the library targets `net10.0` and uses **C# 14** language features).
- A recent IDE with Roslyn analyzer support (Visual Studio 2022+, Rider, or VS Code with the C# Dev Kit)
  is recommended but not required.

Verify your SDK:

```bash
dotnet --version   # should be a 10.x SDK
```

## Repository layout

| Path                 | Purpose                                            |
| -------------------- | -------------------------------------------------- |
| `src/BigOX`          | The library (published as the `BigOX` NuGet package). |
| `src/BigOX.Tests`    | MSTest test project, mirroring the source folders.  |
| `src/BigOX.slnx`     | The solution file (XML `.slnx` format).             |

## Build and test

```bash
# Clone
git clone https://github.com/omarbesiso/BigOX.git
cd BigOX

# Build
dotnet build src/BigOX.slnx

# Run all tests
dotnet test src/BigOX.slnx

# Run a single test class
dotnet test src/BigOX.slnx --filter "FullyQualifiedName~ResultTests"

# Run a single test method
dotnet test src/BigOX.slnx --filter "FullyQualifiedName~GuardTests.NotNull_Throws_WhenNull"

# Collect code coverage (produces a Cobertura report under TestResults/)
dotnet test src/BigOX.slnx --collect:"XPlat Code Coverage"
```

## Build gates (your change must satisfy all of these)

- **Warnings are errors.** `TreatWarningsAsErrors` is on for both **Debug** and **Release**.
- **XML docs are mandatory.** `GenerateDocumentationFile` is on, so every public member needs complete XML
  documentation (`<summary>`, plus `<param>`/`<typeparam>`/`<returns>` and an `<exception>` tag for every
  throw path, including throws routed through `Guard`/`ThrowHelper`).
- **Latest analyzers.** `AnalysisLevel` is `latest` and nullable reference types are enabled everywhere.

## Public API discipline

The library uses **Microsoft.CodeAnalysis.PublicApiAnalyzers**. The committed public surface lives in
`src/BigOX/PublicAPI.Shipped.txt`. Any change to the public API surface will fail the build until the
tracking files are updated.

When you add, change, or remove public members:

1. Build the project — the analyzer reports `RS0016`/`RS0017`/etc. for undeclared or removed symbols.
2. Apply the analyzer code fix from your IDE (**"Add to public API"**), or from the CLI:

   ```bash
   dotnet format analyzers src/BigOX/BigOX.csproj --diagnostics RS0016 --severity warn
   ```

   This appends the new entries to `src/BigOX/PublicAPI.Unshipped.txt`.
3. Commit the updated `PublicAPI.*.txt` files alongside your code change.

Because every shipped version releases together, additions are typically promoted into
`PublicAPI.Shipped.txt` at release time, leaving `PublicAPI.Unshipped.txt` empty between releases.

## Coding conventions

- **File-scoped namespaces.** Keep the public surface minimal and deliberate; prefer `internal sealed`
  for types that don't need to be public.
- **C# 14 extension blocks.** Follow the `extension(Type x) { ... }` style in files that already use it.
- **Guard clauses first.** Validate arguments up front with `ArgumentNullException.ThrowIfNull(...)` or the
  `Guard` helpers before doing work.
- **Performance-aware.** Prefer `readonly struct` for value types, spans for parsing/formatting, and avoid
  LINQ on hot paths. Mark trivial hot helpers `[MethodImpl(MethodImplOptions.AggressiveInlining)]`.
- **Dependencies.** BCL + `Microsoft.Extensions.*` + Scrutor only. Use `System.Text.Json` for
  serialization — never Newtonsoft.Json. No network calls or file I/O in library code.
- **Value objects** should have a canonical `ToString()` that matches their `JsonConverter` output so
  round-trip tests pass.

## Tests

- MSTest, in `src/BigOX.Tests/` **mirroring the source folder structure exactly**.
- Use `[TestClass]`/`[TestMethod]`, and `Assert.ThrowsExactly<T>` / `Assert.ThrowsExactlyAsync<T>` for
  negative paths.
- Name tests `MethodOrScenario_Condition_ExpectedResult`.
- **Every behavior change needs tests**, and every new public API needs coverage.

## Submitting a change

1. Fork the repository and create a topic branch from `main`.
2. Make your change with tests and XML docs; keep the build green (`dotnet test src/BigOX.slnx`).
3. Update `PublicAPI.*.txt` if you touched the public surface.
4. Open a pull request with a clear description and a link to the relevant issue (if any).

## Reporting bugs and requesting features

- Bugs and feature requests: [GitHub Issues](https://github.com/omarbesiso/BigOX/issues).
- Security concerns: see [SECURITY.md](SECURITY.md).
