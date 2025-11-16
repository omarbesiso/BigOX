# BigO X

High‑performance, allocation‑aware utilities for .NET 10 with explicit, typed result handling and small, composable building blocks.

BigO X is designed for back‑end and infrastructure code where:
- Failures must be explicit and observable
- Performance and allocations matter
- APIs are predictable, testable, and framework‑agnostic

Target: .NET 10 (C# 14.0)


## Table of contents
- Overview
- Core concepts
  - `Result<T>` / `Result`
  - `Result<TValue, TError>`
  - `ResultStatus`
  - Errors: `IError`, `Error`, `ErrorKind`
- Factories and utilities
  - `GuidFactory` – sequential v7 GUIDs
- Design principles
- Usage patterns
  - Simple success/failure
  - Mapping and binding
  - Propagating failures
  - Metadata and messages
- Testing guidance
- Versioning and compatibility
- Contributing


## Overview
BigO X provides:
- Strongly‑typed, allocation‑conscious result abstractions
- Clear Success/Failure states with immutable metadata
- Strongly typed error items and basic taxonomy
- Utility factories such as `GuidFactory` for performant identifiers

Favors:
- File‑scoped namespaces and minimal dependencies
- Deterministic, testable code with no hidden global state
- BCL‑first approach (`Microsoft.Extensions.*` only when needed)


## Core concepts

### `Result<T>` – value result with default `Error`
Success:
- Holds a value of type `T`
- May carry `Message` and `Metadata`

Failure:
- Carries `IReadOnlyList<Error>` errors
- `Value` is default

Key members:
- `Result<T>.Success(T value, string? message = null, IReadOnlyDictionary<string, object?>? metadata = null)`
- `Result<T>.Failure(Error error, string? message = null, IReadOnlyDictionary<string, object?>? metadata = null)`
- `Result<T>.Failure(IEnumerable<Error> errors, ...)`
- `bool IsSuccess(out T value)`
- `bool IsFailure(out IReadOnlyList<Error>? errors)`
- `TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)`
- `Result<TNext> Map<TNext>(Func<T, TNext> map)`
- `Result<TNext> Bind<TNext>(Func<T, Result<TNext>> bind)`
- `static implicit operator Result<T>(Error error)`


### `Result` – unit result (no value)
A no‑payload result wrapping `Result<Unit, Error>`.

Key members:
- `Result.Success(string? message = null, IReadOnlyDictionary<string, object?>? metadata = null)`
- `Result.Failure(Error error, string? message = null, IReadOnlyDictionary<string, object?>? metadata = null)`
- `Result.Failure(IEnumerable<Error> errors, ...)`
- `static implicit operator Result(Error error)`


### `Result<TValue, TError>` – generic result with typed errors
Low‑level, fully generic result type where `TError : IError`.

State and data:
- `ResultStatus Status` (`Uninitialized`, `Success`, `Failure`)
- `TValue? Value`
- `IReadOnlyList<TError> Errors`
- `TError FirstError` (throws when not in failure)

Flow:
- `bool IsSuccess(out TValue value)`
- `bool IsFailure(out IReadOnlyList<TError>? errors)`
- `TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<IReadOnlyList<TError>, TResult> onFailure)`
- `Result<TNext, TError> Map<TNext>(Func<TValue, TNext> map)`
- `Result<TNext, TError> Bind<TNext>(Func<TValue, Result<TNext, TError>> bind)`
- `Result<TNext, TError> AsFailure<TNext>()`

Factories:
- `Success(TValue value, string? message = null, IReadOnlyDictionary<string, object?>? metadata = null)`
- `Failure(IEnumerable<TError> errors, ...)`
- `Failure(TError error, ...)`
- `Failure(string? message = null, IReadOnlyDictionary<string, object?>? metadata = null, params TError[] errors)`
- `static implicit operator Result<TValue, TError>(TError error)`

Pattern matching:
- `void Deconstruct(out bool isSuccess, out TValue? value, out IReadOnlyList<TError>? errors)`


### `ResultStatus`
Simple enum describing the result state:
- `Uninitialized` (0)
- `Failure` (1)
- `Success` (2)


### Errors: `IError`, `Error`, `ErrorKind`
- `IError` is the contract (message, code, exception, kind, metadata)
- `Error` is the default implementation
  - `Error.Create(...)` for general errors
  - `Error.Unexpected(...)` for unclassified failures
- `ErrorKind` is a lightweight, string‑backed discriminator with built‑ins:
  - `ErrorKind.Default`
  - `ErrorKind.Unexpected`


## Factories and utilities

### `GuidFactory` – sequential version 7 GUIDs
- `Guid GuidFactory.NewSequentialGuid()`
- `IEnumerable<Guid> GuidFactory.NewSequentialGuids(int count)`

Properties validated by tests:
- Non‑empty GUIDs
- Uniqueness within a batch
- Version nibble is `7` (time‑ordered GUID per RFC v7)

Use when you need DB‑friendly, roughly monotonic identifiers with better index locality than `Guid.NewGuid()`.


## Design principles
- Explicit success/failure; no exceptions for control flow
- Strongly‑typed errors for rich domain diagnostics
- Immutable metadata; internal arrays cloned only when needed
- Performance: readonly structs, minimal allocations, span where helpful
- Framework‑agnostic: minimal dependencies, deterministic code


## Usage patterns

Simple success/failure:
```csharp
using BigOX.Results;

public static Result<int> ParsePositiveInt(string input)
{
    if (!int.TryParse(input, out var value))
    {
        return Error.Create("Input is not a valid integer.");
    }

    if (value <= 0)
    {
        return Error.Create("Value must be positive.");
    }

    return Result<int>.Success(value, message: "Parsed positive integer.");
}

var r = ParsePositiveInt("42");
var text = r.Match(v => $"Value: {v}", errs => $"Failed with {errs.Count} error(s)");
```

Mapping and binding:
```csharp
Result<string> Compose()
{
    return GetUserId()
        .Bind(LoadUser)
        .Bind(SendWelcomeEmail);
}

Result<int> GetUserId() => Result<int>.Success(7);
Result<User> LoadUser(int id) => Result<User>.Success(new User(id));
Result<string> SendWelcomeEmail(User u) => Result<string>.Success($"Sent to {u.Id}");

public sealed record User(int Id);
```

Propagating failures across types (generic form):
```csharp
Result<Order, Error> LoadOrder()
{
    var user = Result<User, Error>.Failure(Error.Create("no user"));
    return user.AsFailure<Order>();
}

public sealed record Order(int Id);
public sealed record User(int Id);
```

Metadata and messages:
```csharp
var meta = new Dictionary<string, object?> { ["correlationId"] = Guid.NewGuid().ToString("N") };
var ok = Result<int, Error>.Success(5, message: "ok", metadata: meta);
var mapped = ok.Map(x => x * 2);
_ = mapped.Metadata["correlationId"]; // preserved
```


## Testing guidance
- MSTest with `[TestClass]`/`[TestMethod]`
- Prefer `Assert.ThrowsExactly<ExceptionType>(...)` for negative paths
- Mirror source layout in test folders
- Assert both state (`Status`, `IsSuccess`/`IsFailure`) and payload/errors

Example (`GuidFactoryTests`):
```csharp
var g = GuidFactory.NewSequentialGuid();
Span<byte> bytes = stackalloc byte[16];
g.TryWriteBytes(bytes);
var version = (bytes[7] >> 4) & 0x0F;
Assert.AreEqual(7, version);
```


## Versioning and compatibility
- Target frameworks: `.NET 10`
- Language version: C# 14.0
- Nullability annotated; warnings treated seriously
- Public API kept minimal; internal surface can evolve for perf/correctness


## Contributing
1. Ensure you have an SDK supporting .NET 10 / C# 14.0
2. Build and run tests (`BigOX.Tests`)
3. Add/update tests with any change; keep APIs documented and immutable by default

Issues and PRs that align with explicit results, strong error modeling, and high‑quality utilities are welcome.