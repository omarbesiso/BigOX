# GitHub Copilot Repository Instructions — BigOX

> Scope: C# utility library + CQRS infrastructure (DI/logging/transactions) with MSTest. Targets the frameworks in the `*.csproj` files (assume .NET 10 unless specified). Output must compile with nullable enabled, XML docs, analyzers set to latest, C# LangVersion=14.0, and TreatWarningsAsErrors.

## 1) Hard constraints

- **Language/TFM**: C# (14.0), **target** frameworks as declared by the `*.csproj`. Avoid APIs not available for that TFM.
- **Dependencies**: Prefer BCL + `Microsoft.Extensions.*` (DependencyInjection, Logging). Use `System.Text.Json` for serialization; do **not** introduce Newtonsoft.Json.
- **Code style**:
  - File‑scoped namespaces.
  - Internal code: `internal sealed` by default. Public surface: minimal and deliberate.
  - Guard clauses first (`ArgumentNullException.ThrowIfNull(...)`, range checks).
  - Use `readonly struct` where applicable for value types and prefer `ReadOnlySpan<T>`/`Span<T>` when optimizing parsing/formatting.
  - Favor pure helpers as **extension methods** in `BigOX.Extensions` (small, inlineable, `[MethodImpl(MethodImplOptions.AggressiveInlining)]` where it makes sense).
- **Docs**: Public APIs require `<summary>` and param/returns XML. Keep concise, accurate, and example-driven.
- **Tests**: Framework is **MSTest**. Use `[TestClass]`, `[TestMethod]`, `Assert.ThrowsExactly{Async}` (or `Assert.IsInstanceOfType`, `Assert.AreEqual`, etc.). Organize tests mirroring source folders.
- **Patches/Diffs**: When asked to change existing code, prefer unified diffs or minimal patch blocks.
- **Safety**: No secrets, keys, or network calls in library code. Keep APIs deterministic and testable.

## 2) Patterns to follow

### 2.1 Extensions
- Keep methods small, pure, and allocation‑free. Provide unit tests verifying happy/edge cases and complementarity (`IsEmpty` vs `IsNotEmpty`).

### 2.2 Value types (Types) 

- Prefer `public readonly struct` with canonical formatting + `TryParse`.
- Implement a dedicated `JsonConverter<T>` for `System.Text.Json` if round‑trip string forms are important (e.g., `DateRangeConverter`).
- Unit tests must cover construction, invariants, parsing/formatting, and equivalence.

## 3) Test authoring rules (MSTest)

- Use `[TestClass]` and `[TestMethod]` (or `DataTestMethod`/`DataRow` for tables).
- Prefer `Assert.ThrowsExactly{Async}` for negative paths; check param names/messages only when meaningful.
- Keep tests deterministic, side‑effect free, and isolated (use in‑memory collaborators; inject via DI; avoid static state).
- Name tests: `MethodOrScenario_Condition_ExpectedResult`.

## 4) Serialization

- Default to `System.Text.Json`. Provide explicit converters for value objects; ensure `ToString()` matches the canonical converter format so tests can round‑trip.

## 5) Performance & allocations

- Use spans for parsing/formatting; avoid LINQ in hot paths.
- Mark hot, trivial extension methods with `AggressiveInlining`.
- Prefer `readonly struct` fields/properties where applicable.

## 6) Public API discipline

- Keep public types/members minimal. Everything else should be `internal` (and `sealed`) unless extensibility is an explicit goal.
- Every public member has XML docs and tests.

## 7) Copilot response formatting

- When asked to refactor or add code, **return minimal diffs** or complete buildable files for the exact paths in `src/BigOX/...` and corresponding tests in `src/BigOX.Tests/...`.

## 8) Non-goals

- No web/service calls, EF setup, or app‑level bootstrapping here. Keep BigOX generic and framework‑agnostic.