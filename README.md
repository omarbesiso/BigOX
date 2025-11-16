# BigO X

[![NuGet Version](https://img.shields.io/nuget/v/BigOX?logo=nuget&label=NuGet)](https://www.nuget.org/packages/BigOX)
[![NuGet Downloads](https://img.shields.io/nuget/dt/BigOX?logo=nuget&label=Downloads)](https://www.nuget.org/packages/BigOX)
[![GitHub License](https://img.shields.io/badge/license-Check_Repo-blue)](https://github.com/omarbesiso/BigOX)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/download/dotnet/10.0)

High-performance, allocation-aware result & error primitives for **.NET 10 / C# 14**. 
Built by me (Omar Besiso, aka BigO) to assist with my applications' back-end and infrastructure code.

I wanted to start this with a clean slate with .NET 10 without updating my previous libraries, that never got the GitHub love they deserve due to time constraints, so here we are. 

BigO X focuses on **explicit failures**, **typed errors**, and **small, composable building blocks** for back-end and infrastructure code.

---

## Why BigO X?

Most real-world systems need more than `bool` and exceptions:

- You want **explicit success/failure**, not hidden control flow.
- You want **typed errors** carrying message, code, exception, and metadata.
- You care about **allocations and performance** on hot paths.
- You want a **.NET 10-only** library that embraces **C# 14** features like extension
  blocks and first-class `Span<T>`.

BigO X gives you:

- `Result<T>` / `Result` for the common case.
- Fully generic `Result<TValue, TError>` with `TError : IError`.
- A small error model (`IError`, `Error`, `ErrorKind`) with immutable metadata.
- Utility helpers like `GuidFactory` for sequential GUIDs.
- DDD and CQRS building blocks for your services.
- A tiny auth framework-agnostic to help with authorizing CQRS operations.

Use BigO X when you need **predictable, testable, framework-agnostic** primitives around results, errors, and identifiers.

---

## Table of contents

- [Install](#install)
- [Quickstart](#quickstart)
- [Features](#features)
- [Usage Examples](#usage-examples)
- [Compatibility](#compatibility)
- [Performance & Benchmarks](#performance--benchmarks)
- [Configuration & Extensibility](#configuration--extensibility)
- [Versioning & Roadmap](#versioning--roadmap)
- [Contributing](#contributing)
- [Security](#security)
- [License](#license)
- [FAQ](#faq)
- [Notes & Assumptions](#notes--assumptions)

---

## Install

BigO X is published as a NuGet package targeting **.NET 10 (`net10.0`)**.

### .NET CLI

```bash
dotnet add package BigOX
```

### Package Manager Console

```powershell
Install-Package BigOX
```

### `PackageReference`

```xml
<ItemGroup>
  <PackageReference Include="BigOX" Version="10.0.0" />
</ItemGroup>
```

### Central package management (`Directory.Packages.props`)

```xml
<ItemGroup>
  <PackageVersion Include="BigOX" Version="10.0.0" />
</ItemGroup>
```

### File-based apps and scripting (.NET 10)

For C# file-based apps, you can reference BigO X directly via `#:package` in your
`.cs` file:

```csharp
#:package BigOX@10.0.0

using BigOX.Results;
```

---

## Quickstart

You can be up and running in under a minute.

```bash
dotnet new console -n BigOX.Demo
cd BigOX.Demo
dotnet add package BigOX
```

---

## Features

### Core namespaces & types

All of the following live in the main BigO X assembly (namespaces grouped by usage):

- **Results & errors** (primary)
  - `Result<T>` – value result with default `Error`.
  - `Result` – unit result (no value).
  - `Result<TValue, TError>` – fully generic result where `TError : IError`.
  - `ResultStatus` – `Uninitialized`, `Failure`, `Success`.
  - Error model:
    - `IError` – message, code, exception, kind, metadata.
    - `Error` – default implementation (`Create`, `Unexpected`, etc.).
    - `ErrorKind` – lightweight string-backed discriminator.
- **CQRS (Commands/Queries)**
  - Contracts:
    - `ICommand`, `IQuery` (markers)
    - `ICommandHandler<TCommand>`, `ICommandHandler<TCommand, TValue>` (returns `IResult<TValue>`)
    - `IQueryHandler<TQuery, TResult>`
    - `ICommandBus` (default: `IocCommandBus`), `IQueryProcessor` (default: `IocQueryProcessor`)
  - Decorators & logging:
    - `ICommandDecorator<TCommand>`, `IQueryDecorator<TQuery, TResult>`
    - `LoggingCommandDecorator<TCommand>`, `LoggingQueryDecorator<TQuery, TResult>` with source-generated `CqrsLog`
  - DI helpers (in `CqrsServiceCollectionExtensions`):
    - `RegisterCommandHandler<TCommand, THandler>()`, `RegisterQueryHandler<TQuery, TResult, THandler>()`
    - `RegisterDefaultCommandBus()`, `RegisterDefaultQueryProcessor()`
    - `RegisterModuleCommandHandlers<TModule>()`, `RegisterModuleQueryHandlers<TModule>()`
    - `AddCqrs(...)` overloads with optional decorators or ordered decorator pipelines
- **DDD (Entities, domain events, repositories)**
  - Entities:
    - `IEntity<TId>` – minimal contract with `Id`.
    - `Entity<TId>` – base class with value-based equality on `Id`, null-safe `==`/`!=` operators.
  - Domain events:
    - `IDomainEvent` (marker), `IDomainEventHandler<TDomainEvent>`
    - `IDomainEventBus` with default IoC-based `IocDomainEventBus`
  - Repository & unit of work:
    - `IRepository` – marker interface for typed repositories
    - `IUnitOfWork` – `Commit()` / `CommitAsync(...)`
  - DI helpers (in `DomainServiceCollectionExtensions`):
    - `RegisterDomainEventHandler<TDomainEvent, TDomainEventHandler>()`
    - `RegisterDefaultDomainEventBus()`
    - `RegisterModuleDomainEventHandlers<TModule>()`
- **Factories & utilities**
  - `GuidFactory` – RFC v7-style sequential GUIDs with tests validating version and
    uniqueness.

### Design characteristics

- **Explicit**: no exceptions for control flow; success/failure is part of the type.
- **Allocation-aware**: readonly structs, span usage where appropriate, cloned arrays only
  when needed.
- **Metadata-rich**: errors and results can carry immutable metadata dictionaries.
- **Framework-agnostic**: minimal BCL-first dependencies; optional integration via
  `Microsoft.Extensions.*` and Scrutor.
- **C# 14-ready**:
  - Plays well with extension blocks / extension members.
  - Benefits from first-class `Span<T>` support and other C# 14 language features.

---

## Usage Examples

### 1. Simple success/failure flow

```csharp
using BigOX.Results;

public static class UserParser
{
    public static Result<int> ParseUserId(string input)
    {
        if (!int.TryParse(input, out var id))
        {
            return Error.Create("userId_not_numeric");
        }

        if (id <= 0)
        {
            return Error.Create("userId_must_be_positive");
        }

        return Result<int>.Success(id);
    }
}
```

Consuming code:

```csharp
var result = UserParser.ParseUserId("7");

var output = result.Match(
    onSuccess: id => $"User ID: {id}",
    onFailure: errors => $"Failed: {string.Join(", ", errors.Select(e => e.Code ?? e.Message))}"
);
```

### 2. Composing operations with `Bind`

```csharp
using BigOX.Results;

public static class Registration
{
    public static Result<string> RegisterUser(string rawUserId)
    {
        return ParseUserId(rawUserId)
            .Bind(LoadUser)
            .Bind(SendWelcomeEmail);
    }

    static Result<int> ParseUserId(string input) =>
        UserParser.ParseUserId(input);

    static Result<User> LoadUser(int id)
    {
        // Pretend this is a database call
        return Result<User>.Success(new User(id));
    }

    static Result<string> SendWelcomeEmail(User user)
    {
        // Pretend this sends an email
        return Result<string>.Success($"Email sent to user {user.Id}");
    }

    public sealed record User(int Id);
}
```

This shows:

- Flat, pipeline-style composition.
- Automatic propagation of failures using the same `Error` model.

### 3. Switching to fully generic `Result<TValue, TError>`

For advanced scenarios you can use the generic form:

```csharp
using BigOX.Results;

public static class Orders
{
    public static Result<Order, Error> LoadOrder(int id)
    {
        // Simulate a failed dependency:
        var user = Result<User, Error>.Failure(Error.Create("user_not_found"));

        // Propagate as failure for a different value type:
        return user.AsFailure<Order>();
    }

    public sealed record User(int Id);
    public sealed record Order(int Id);
}
```

Here you control the error type (`TError`) and still have familiar APIs like
`IsSuccess`, `IsFailure`, `Map`, `Bind`, and `Match`.

### 4. Sequential GUIDs with `GuidFactory`

```csharp
using BigOX.Results;

var guid = GuidFactory.NewSequentialGuid();

Console.WriteLine(guid);

// Batch
foreach (var g in GuidFactory.NewSequentialGuids(10))
{
    Console.WriteLine(g);
}
```

Properties: non-empty, unique per batch, and version nibble is `7`
(time-ordered GUID).

Use these for DB-friendly clustered indices with better locality than `Guid.NewGuid()`.

### 5. CQRS quickstart: commands, queries, DI, and decorators

```csharp
using BigOX.Cqrs;
using BigOX.Results;
using BigOX.Cqrs.Logging;
using Microsoft.Extensions.DependencyInjection;

// Define a command
public sealed record CreateUser(string Email) : ICommand;

// Command handler (no return value)
public sealed class CreateUserHandler : ICommandHandler<CreateUser>
{
    public Task Handle(CreateUser command, CancellationToken ct)
    {
        // persist user, publish events, etc.
        return Task.CompletedTask;
    }
}

// Command handler with value result
public sealed class CreateUserReturningIdHandler : ICommandHandler<CreateUser, Guid>
{
    public Task<IResult<Guid>> Handle(CreateUser command, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        return Task.FromResult<IResult<Guid>>(Result<Guid>.Success(id));
    }
}

// Define a query
public sealed record GetUser(Guid Id) : IQuery;

// Query handler returns a plain TResult
public sealed class GetUserHandler : IQueryHandler<GetUser, UserDto>
{
    public Task<UserDto> Read(GetUser query, CancellationToken ct)
        => Task.FromResult(new UserDto(query.Id, "user@example.com"));
}

public sealed record UserDto(Guid Id, string Email);

var services = new ServiceCollection();

// Register handlers
services
    .RegisterCommandHandler<CreateUser, CreateUserHandler>()
    .RegisterQueryHandler<GetUser, UserDto, GetUserHandler>()
    .RegisterDefaultCommandBus()
    .RegisterDefaultQueryProcessor();

// Optionally add logging decorators for all handlers
services.AddCqrs(
    infrastructureLifetime: ServiceLifetime.Scoped,
    commandHandlerDecoratorType: typeof(LoggingCommandDecorator<>),
    queryHandlerDecoratorType: typeof(LoggingQueryDecorator<,>));

await using var provider = services.BuildServiceProvider();

// Use the bus/processor
var bus = provider.GetRequiredService<ICommandBus>();
await bus.Send(new CreateUser("user@example.com"));

var processor = provider.GetRequiredService<IQueryProcessor>();
var dto = await processor.ProcessQuery<GetUser, UserDto>(new GetUser(Guid.NewGuid()));
```

Notes:

- Commands may optionally return a value via `ICommandHandler<TCommand, TValue>` where the value is wrapped in `IResult<TValue>`.
- Queries return a plain `TResult` from `IQueryHandler<TQuery, TResult>`.
- Use `RegisterModuleCommandHandlers<TModule>()` / `RegisterModuleQueryHandlers<TModule>()` to scan and register handlers in an assembly (Scrutor-based).
- `AddCqrs(...)` overloads allow decorating all handlers with one or more decorators (e.g., logging, validation, metrics).

### 6. DDD quickstart: entities and domain events

```csharp
using BigOX.Domain;
using Microsoft.Extensions.DependencyInjection;

// Define an entity using the base equality semantics
public sealed class User : Entity<Guid>
{
    public User(Guid id) { Id = id; }
}

// Define a domain event
public sealed record UserRegistered(Guid UserId, string Email) : IDomainEvent;

// Handle the domain event
public sealed class SendWelcomeEmail : IDomainEventHandler<UserRegistered>
{
    public Task Handle(UserRegistered @event, CancellationToken ct = default)
    {
        // send email, enqueue job, etc.
        return Task.CompletedTask;
    }
}

// Wire up DI
var services = new ServiceCollection()
    .RegisterDefaultDomainEventBus() // IDomainEventBus -> IocDomainEventBus
    .RegisterDomainEventHandler<UserRegistered, SendWelcomeEmail>();

await using var provider = services.BuildServiceProvider();
var eventBus = provider.GetRequiredService<IDomainEventBus>();

// Publish the event (all handlers for the event type will be invoked)
await eventBus.Publish(new UserRegistered(Guid.NewGuid(), "user@example.com"));
```

Tips:

- Use `RegisterModuleDomainEventHandlers<TModule>()` to scan and register handlers in an assembly.
- `IRepository` and `IUnitOfWork` are provided as minimal contracts; implement them in your data layer with intent-revealing operations and transactional boundaries.

---

## Compatibility

BigO X targets `.NET 10` and leverages its LTS guarantees.

From the NuGet compatibility matrix:

- **Included TFM**
  - `net10.0`
- **Computed compatible TFMs** (via .NET 10):
  - `net10.0-android`
  - `net10.0-browser`
  - `net10.0-ios`
  - `net10.0-maccatalyst`
  - `net10.0-macos`
  - `net10.0-tvos`
  - `net10.0-windows`

### Support matrix

| Aspect          | Status                         | Notes                                           |
| --------------- | ------------------------------ | ----------------------------------------------- |
| .NET            | ✅ `.NET 10` only               | Uses .NET 10 SDK features.                      |
| OS / Runtime    | ✅ Cross-platform via `net10.0` | Works wherever .NET 10 does.                    |
| Nullable        | ✅ Fully annotated              | Warnings treated seriously.                     |
| Trimming        | ⚠️ Likely OK for core types     | Verify in your app; see notes below.            |
| AOT / NativeAOT | ⚠️ Expected to work with care   | Avoid reflection-heavy patterns around Scrutor. |
| Unity / older   | ❌ Not supported                | Targeting .NET 10 and C# 14 only.               |

**Trimming & AOT:** the core result types are simple generics with no heavy reflection. However, the package references `Microsoft.Extensions.*` and Scrutor, which may require additional trimming configuration in some hosting environments.

---

## Performance & Benchmarks

BigO X is designed to work with the runtime rather than against it:

- Uses **readonly structs** and **minimal allocations** where appropriate.
- Favours **span-friendly APIs**; .NET 10 and C# 14 introduce better first-class support
  for `Span<T>` / `ReadOnlySpan<T>` and related conversions.
- Benefits from .NET 10 JIT improvements in inlining, devirtualization and stack
  allocations.

At the time of writing, there is **no public BenchmarkDotNet suite** in this repository.

If you care about specific scenarios (e.g. result mapping in tight loops), consider:

```csharp
// Pseudo-code outline for your own benchmarks
[MemoryDiagnoser]
public class ResultBenchmarks
{
    [Benchmark]
    public Result<int> SuccessThenMap()
    {
        var r = Result<int>.Success(5);
        return r.Map(x => x * 2);
    }
}
```

Contributions adding a `BigOX.Benchmarks` project and documented results are welcome.

---

## Configuration & Extensibility

### Error model extensibility

The error stack is intentionally small:

- `IError` – abstraction over message, code, exception, kind, metadata.
- `Error` – default implementation with helpers like `Error.Create` and
  `Error.Unexpected`.
- `ErrorKind` – lightweight discriminator with common built-ins.

Patterns that scale well:

- Use **`ErrorKind`** values (e.g. `"validation"`, `"infrastructure"`, `"security"`)
  as your taxonomy.
- Use **metadata** for correlation IDs, external error codes, payload fragments, etc.

### C# 14 extension members around `Result<T>`

C# 14 adds **extension blocks** and **extension properties** so you can group instance
and static extensions for BigO X types:

```csharp
using BigOX.Results;

public static class ResultExtensions
{
    // Extension block over Result<T>
    extension<T>(Result<T> result)
    {
        public bool IsOk => result.IsSuccess(out _);

        public bool IsFailureOfType(string? code) =>
            result.IsFailure(out var errors) &&
            errors!.Any(e => e.Code == code);
    }
}
```

Usage:

```csharp
var r = ParseUserId("7");

if (r.IsOk)
{
    // ...
}
```

This keeps your domain-specific logic as a thin layer around BigO X.

### DI & configuration

The package references the standard `Microsoft.Extensions.Configuration`,
`Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Logging.Abstractions`,
`Microsoft.Extensions.Options` and **Scrutor**.

Typical patterns:

```csharp
using BigOX.Results;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

var services = new ServiceCollection();

// Scan application assemblies (Scrutor)
services.Scan(scan => scan
    .FromCallingAssembly()
    .AddClasses()
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// BigO X types are plain C#; no global state or static service locators.
```

BigO X itself doesn’t force any hosting model; you can use it in ASP.NET Core, worker
services, console apps, or background jobs.

---

## Versioning & Roadmap

### Versioning

- Current NuGet version: **10.0.0** (initial .NET 10 release).
- Package ID: `BigOX`.
- License: **MIT**.

Intended policy:

- **MAJOR** – breaking changes to the public API.
- **MINOR** – new functionality added in a backward-compatible way.
- **PATCH** – bug fixes and small, safe improvements.

See:

- NuGet version history:
  https://www.nuget.org/packages/BigOX#versions-tab
- Git commits and tags:
  https://github.com/omarbesiso/BigOX/commits/main

### Roadmap

Planned and proposed work is tracked via GitHub Issues:

- Open issues: https://github.com/omarbesiso/BigOX/issues
- Feature requests: label `enhancement`
- Bug reports: label `bug`

---

## Contributing

Contributions are welcome.

1. Ensure you have the **.NET 10 SDK** and **C# 14** enabled.

2. Clone the repo:

   ```bash
   git clone https://github.com/omarbesiso/BigOX.git
   cd BigOX
   ```

3. Build and run tests:

   ```bash
   dotnet test
   ```

4. Follow these guidelines:

   - Keep **public API minimal and well-documented**.
   - Prefer **immutable** types and **readonly structs** for results.
   - Avoid adding dependencies beyond the existing `Microsoft.Extensions.*` and Scrutor
     without a strong justification.
   - Add or update tests for every change that affects behavior.

5. Open a PR with a clear description and link to the relevant issue (if any).

There is currently no dedicated `CONTRIBUTING.md`; these guidelines serve as the
canonical reference until one is added.

---

## Security

- For **non-sensitive bugs**, use GitHub Issues:
  https://github.com/omarbesiso/BigOX/issues
- For **potential vulnerabilities**, use the private contact channel listed on the
  NuGet page under “Contact owners”.

General guidance:

- BigO X does **not** handle secrets or cryptographic primitives itself.
- It does not introduce network access or I/O; it stays in-process and in-memory.
- Security impact typically comes from how you log/serialize errors and metadata;
  avoid logging sensitive payloads directly.

If a dedicated `SECURITY.md` is later added, that document becomes the source of truth.

---

## License

- **License:** [MIT](LICENSE)
- **Owner:** Omar Besiso (BigO) (© 2025).

You are free to use BigO X in commercial and open-source projects under the terms of the MIT license.

---

## FAQ

### Why another `Result` library?

BigO X is tuned for:

- **.NET 10 / C# 14 only** – it assumes modern language/runtime features.
- **Explicit error modelling** (`IError`, `Error`, `ErrorKind`) with metadata.
- **Performance** – readonly structs, allocation awareness, and span-friendly design.
- **Small surface** – a focused, coherent set of primitives, not a grab-bag.

If you’re on earlier TFMs or prefer a different style, you can continue to use other BigO.* packages or third-party libraries.

### Does BigO X throw exceptions?

The design goal is **no exceptions for control flow**; callers should observe failures
via `Result` and `Result<T>`/`Result<TValue, TError>`.

Exceptions may still appear for:

- Programming errors (null arguments, out-of-range values).
- Environment failures thrown by dependencies (e.g. configuration providers).

### Can I use this in ASP.NET Core / workers / Blazor / MAUI?

Yes, as long as you’re targeting **.NET 10**:

- BigO X is framework-agnostic; it’s just .NET 10 code.
- NuGet’s compatibility matrix shows support for `net10.0` and computed variants such
  as `net10.0-android` and `net10.0-browser`.
- You can integrate with the `Microsoft.Extensions.*` stack in hosted services.

### Does BigO X support .NET 6/7/8/9?

No. BigO X **targets `.NET 10` only**. 

### Is BigO X trimming/AOT friendly?

The **core result types** (`Result<T>`, `Result`, `Result<TValue, TError>`, `Error`,
`ErrorKind`) are simple generics without reflection and should behave well under
trimming and NativeAOT.

However:

- The package depends on Scrutor and `Microsoft.Extensions.*`.
- Scrutor relies on assembly scanning and may need additional trimming configuration in
  some hosting models.

If you plan to ship NativeAOT, run your linker/analyzer and add any required
`DynamicDependency`/descriptor hints around your DI configuration.

------

