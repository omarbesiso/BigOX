# BigO X

[![NuGet Version](https://img.shields.io/nuget/v/BigOX?logo=nuget&label=NuGet)](https://www.nuget.org/packages/BigOX)
[![NuGet Downloads](https://img.shields.io/nuget/dt/BigOX?logo=nuget&label=Downloads)](https://www.nuget.org/packages/BigOX)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/omarbesiso/BigOX/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![C#](https://img.shields.io/badge/C%23-14-239120)](https://learn.microsoft.com/dotnet/csharp/)

**BigO X** is a small, opinionated, allocation-aware toolkit for **.NET 10 / C# 14** back-end code:
typed **results & errors**, **CQRS** plumbing, **DDD** building blocks, a framework-agnostic
**authorization** engine, **guard-clause** validation, and a large set of pragmatic **extension methods** —
all in one dependency-light assembly.

I'm Omar Besiso (BigO). I built this to be the shared foundation under my own services: explicit failures,
no magic, and primitives I can reason about. It's MIT-licensed, so help yourself.

---

## Table of contents

- [Why BigO X?](#why-bigo-x)
- [Install](#install)
- [60-second quickstart](#60-second-quickstart)
- [Feature tour](#feature-tour)
  - [BigOX.Results](#bigoxresults)
  - [BigOX.Cqrs](#bigoxcqrs)
  - [BigOX.Domain](#bigoxdomain)
  - [BigOX.Security](#bigoxsecurity)
  - [BigOX.Validation](#bigoxvalidation)
  - [BigOX.Extensions](#bigoxextensions)
  - [BigOX.Types](#bigoxtypes)
  - [BigOX.Factories](#bigoxfactories)
  - [BigOX.DependencyInjection](#bigoxdependencyinjection)
- [Compatibility](#compatibility)
- [Performance & design principles](#performance--design-principles)
- [Configuration & extensibility](#configuration--extensibility)
- [Versioning policy](#versioning-policy)
- [Contributing](#contributing)
- [Security](#security)
- [License](#license)
- [FAQ](#faq)

---

## Why BigO X?

Most real systems need more than `bool` and exceptions:

- **Explicit success/failure** that lives in the type, not in hidden control flow.
- **Typed errors** carrying a message, a machine code, an optional exception, a *kind*, and metadata.
- **Composable pipelines** (`Map`/`Bind`/`Match`/`Tap`/`Ensure`) instead of nested `try`/`catch`.
- **Performance you can feel**: `readonly struct` results, span-friendly guards, no LINQ on hot paths.
- **A cohesive spine** for services — CQRS dispatch, domain events, specifications, and authorization —
  without dragging in a heavyweight framework.

BigO X targets **.NET 10 only** and leans into **C# 14** (extension blocks, first-class spans). If you're on
an older TFM, this isn't for you — and that's on purpose.

---

## Install

BigO X ships as a single NuGet package. Install snippets are intentionally **unpinned** so they can't go stale.

```bash
dotnet add package BigOX
```

```powershell
Install-Package BigOX
```

```xml
<!-- PackageReference -->
<ItemGroup>
  <PackageReference Include="BigOX" />
</ItemGroup>
```

```xml
<!-- Central Package Management (Directory.Packages.props) -->
<ItemGroup>
  <PackageVersion Include="BigOX" Version="10.5.0" />
</ItemGroup>
```

The package targets `net10.0`, embeds Source Link, and ships a `.snupkg` symbol package for step-through
debugging.

---

## 60-second quickstart

```csharp
using BigOX.Results;

// A function that can fail — the failure is part of the return type.
static Result<int> ParseQuantity(string input)
{
    if (!int.TryParse(input, out var qty))
    {
        return Error.Validation("Quantity is not a number.", code: "qty_not_numeric");
    }

    return qty > 0
        ? Result<int>.Success(qty)
        : Error.Validation("Quantity must be positive.", code: "qty_not_positive");
}

// Compose without try/catch. Failures short-circuit automatically.
var message = ParseQuantity("7")
    .Map(qty => qty * 2)
    .Tap(total => Console.WriteLine($"Reserving {total} units"))
    .Match(
        onSuccess: total => $"Reserved {total} units",
        onFailure: errors => $"Rejected: {errors[0].Code}");

Console.WriteLine(message); // Reserved 14 units
```

Notice: an `Error` converts implicitly to a failed `Result<int>`, `Map` only runs on success, `Tap` performs
a side effect and passes the result through unchanged, and `Match` collapses both branches into one value.

---

## Feature tour

Everything lives in one assembly under the `BigOX.*` namespaces. Each section below is verified against the
current source.

### BigOX.Results

The core primitive. Three result flavors share one engine: `Result<T>` (value) and unit `Result` are thin
`readonly struct` wrappers that delegate to the fully generic `Result<TValue, TError> where TError : IError`.

**Statuses.** Every result is `Success`, `Failure`, or `Uninitialized` (the default-struct state). Combinators
throw `InvalidOperationException` on an uninitialized result rather than silently treating it as success or
failure. A failure always carries at least one error — building one from an empty sequence throws
`ArgumentException`.

#### The error model

| Type | Kind | What it is |
| --- | --- | --- |
| `IError` | interface | `ErrorMessage`, `Code`, `Exception?`, `Kind`, `Metadata`. |
| `Error` | `sealed record` | Default `IError`. Immutable; built via static factories. `Code` falls back to `Kind.Value` when omitted. |
| `ErrorKind` | `readonly record struct` | String-backed discriminator; equality on `Value`. |

`ErrorKind` ships a standard taxonomy as static readonly values — `Default`, `Unexpected`, `Validation`,
`NotFound`, `Conflict`, `Unauthorized`, `Forbidden` — and `ErrorKind.FromString("…")` for custom kinds.

`Error` factories (each takes `message`, then optional `code`, `exception`, `metadata`):

| Factory | Resulting `Kind` |
| --- | --- |
| `Error.Create(message, code?, kind?, exception?, metadata?)` | `kind ?? ErrorKind.Default` |
| `Error.Unexpected(message, …)` | `Unexpected` |
| `Error.Validation(message, …)` | `Validation` |
| `Error.NotFound(message, …)` | `NotFound` |
| `Error.Conflict(message, …)` | `Conflict` |
| `Error.Unauthorized(message, …)` | `Unauthorized` |
| `Error.Forbidden(message, …)` | `Forbidden` |

```csharp
var err = Error.NotFound("User 42 does not exist.", code: "user_missing",
    metadata: new Dictionary<string, object?> { ["userId"] = 42 });

Console.WriteLine(err.Kind);      // NotFound
Console.WriteLine(err.Code);      // user_missing
Console.WriteLine(err.Metadata["userId"]); // 42
```

#### Combinators

| Member | On | Behavior |
| --- | --- | --- |
| `Map(Func<T,TNext>)` | `Result<T>`, `Result<TValue,TError>` | Transforms the success value; preserves errors, message, metadata. |
| `Bind(Func<T,Result<…>>)` | `Result<T>`, `Result<TValue,TError>` | Monadic chain of result-returning steps. |
| `Match(onSuccess, onFailure)` | all three | Collapses both branches to one value. Unit `Result`'s `onSuccess` takes no argument. |
| `Tap(Action<T>)` | `Result<T>`, `Result<TValue,TError>` | Runs a side effect on success; returns the original result. |
| `TapError(Action<IReadOnlyList<…>>)` | `Result<T>`, `Result<TValue,TError>` | Side effect on failure; returns the original. |
| `Ensure(predicate, error)` | `Result<T>`, `Result<TValue,TError>` | Converts a success that fails `predicate` into a failure carrying `error`. |
| `MapError(Func<TError,TNextError>)` | `Result<TValue,TError>` only | Re-types every error, preserving value/message/metadata. |
| `AsFailure<TNext>()` | `Result<TValue,TError>` only | Re-types a failure to a new value type. |
| `MapAsync` / `BindAsync` | `Result<T>`, `Result<TValue,TError>` | `Task`-returning async `Map`/`Bind` (all use `ConfigureAwait(false)`). |

All combinators null-check their delegates (`ArgumentNullException`). The unit `Result` deliberately omits
`Map`/`Bind`/`Tap`/`Ensure` — it exposes `Match`, `Deconstruct(out bool, out errors)`, and
`TryGetErrors(out errors)`.

```csharp
using BigOX.Results;

async Task<Result<string>> LoadNameAsync(int id) =>
    await Result<int>.Success(id)
        .Ensure(x => x > 0, Error.Validation("Id must be positive."))
        .MapAsync(async x => await FetchNameFromDbAsync(x)); // Task<string>

// Inspect a failure without a full Match:
var r = Result.Failure(Error.Conflict("Already exists."));
if (r.TryGetErrors(out var errors))
{
    Console.WriteLine(errors[0].Kind); // Conflict
}
```

#### Choosing a value type

```csharp
// Result<T>: value + default Error type — the common case.
Result<Order> a = Result<Order>.Success(order);

// Result: no value payload (a "unit" result) — commands, void-ish operations.
Result b = Result.Success("saved");

// Result<TValue, TError>: bring your own IError type.
public sealed record ValidationError(string ErrorMessage) : IError { /* … */ }
Result<Order, ValidationError> c = Result<Order, ValidationError>.Failure(new ValidationError("bad"));
```

`ResultExtensions` adds `IsSuccess`/`IsFailure` convenience flags plus `ErrorsByCode(code)` and
`ErrorsByKind(kind)` filters over any `IResult`.

---

### BigOX.Cqrs

A minimal command/query bus with decorator-based cross-cutting concerns.

**Contracts.** `ICommand` / `IQuery` are markers. Handlers implement `ICommandHandler<TCommand>` (returns
`Task`), `ICommandHandler<TCommand, TValue>` (returns `Task<IResult<TValue>>`), or
`IQueryHandler<TQuery, TResult>` (returns `Task<TResult>`). Dispatch goes through `ICommandBus` and
`IQueryProcessor`; the default `Ioc*` implementations resolve handlers from the container.

**Decorators.** Cross-cutting behavior is layered as decorators (`ICommandDecorator<>` / `IQueryDecorator<,>`).
Built in: `LoggingCommandDecorator<>` / `LoggingQueryDecorator<,>` (source-generated logging + timing), the
`TransactionCommandDecoratorBase<>` / `DefaultTransactionCommandDecorator<>` transaction scope, and the
authorization decorators that bridge into `BigOX.Security`.

**Registration helpers** (on `IServiceCollection`):

| Helper | Default lifetime | Purpose |
| --- | --- | --- |
| `RegisterCommandHandler<TCommand, THandler>()` | Transient | Register one command handler. |
| `RegisterQueryHandler<TQuery, TResult, THandler>()` | Transient | Register one query handler. |
| `RegisterDefaultCommandBus()` | Singleton | Wire `ICommandBus` → `IocCommandBus`. |
| `RegisterDefaultQueryProcessor()` | Singleton | Wire `IQueryProcessor` → `IocQueryProcessor`. |
| `RegisterModuleCommandHandlers<TModule>()` | Transient | Scan a module's assembly for command handlers. |
| `RegisterModuleQueryHandlers<TModule>()` | Transient | Scan a module's assembly for query handlers. |
| `AddCqrs(infrastructureLifetime = Scoped, commandHandlerDecoratorType?, queryHandlerDecoratorType?)` | Scoped infra | Register bus + processor and optionally apply one decorator each. |
| `AddCqrs(infrastructureLifetime, IEnumerable<Type> commandDecorators, IEnumerable<Type> queryDecorators)` | — | Apply an **ordered decorator pipeline**. |

```csharp
using BigOX.Cqrs;
using BigOX.Cqrs.Logging;
using BigOX.Results;
using Microsoft.Extensions.DependencyInjection;

public sealed record CreateUser(string Email) : ICommand;

public sealed class CreateUserHandler : ICommandHandler<CreateUser>
{
    public Task Handle(CreateUser command, CancellationToken ct = default)
        => Task.CompletedTask; // persist the user, publish events, …
}

public sealed record GetUser(Guid Id) : IQuery;

public sealed class GetUserHandler : IQueryHandler<GetUser, string>
{
    public Task<string> Read(GetUser query, CancellationToken ct = default)
        => Task.FromResult($"user:{query.Id}");
}

var services = new ServiceCollection()
    .AddLogging()
    .RegisterCommandHandler<CreateUser, CreateUserHandler>()   // register handlers first…
    .RegisterQueryHandler<GetUser, string, GetUserHandler>();

// …then wire infrastructure and decorate every handler with logging.
services.AddCqrs(
    infrastructureLifetime: ServiceLifetime.Scoped,
    commandHandlerDecoratorType: typeof(LoggingCommandDecorator<>),
    queryHandlerDecoratorType: typeof(LoggingQueryDecorator<,>));

await using var provider = services.BuildServiceProvider();

var bus = provider.GetRequiredService<ICommandBus>();
await bus.Send(new CreateUser("a@b.com"));

var processor = provider.GetRequiredService<IQueryProcessor>();
var name = await processor.ProcessQuery<GetUser, string>(new GetUser(Guid.NewGuid()));
```

**Value-returning commands.** A command can also return a value: implement
`ICommandHandler<TCommand, TValue>` (returns `Task<IResult<TValue>>`), register it directly
(`services.AddScoped<ICommandHandler<CreateUser, Guid>, CreateUserHandler>()`), and dispatch with
`bus.Send<CreateUser, Guid>(...)`. `RegisterCommandHandler<TCommand, THandler>` targets the no-value
`ICommandHandler<TCommand>`.

**Ordered pipeline semantics.** In `AddCqrs(lifetime, [A, B, C], …)` each decorator wraps the previous one, so
the resolved graph is `C(B(A(handler)))`. At runtime the **last** type in the list executes **first**
(outermost) and the **first** type executes **last**, nearest the handler. Decoration is skipped unless a
matching handler is already registered — always register handlers before calling `AddCqrs`.

---

### BigOX.Domain

DDD building blocks.

- **`Entity<TId>`** (`where TId : struct, IEquatable<TId>`) — identity-based equality with null-safe `==`/`!=`.
  Two entities are equal only when they share a runtime type and a **non-default** `Id`; transient entities
  (default id) are never equal, and cross-type comparison is always unequal.
- **Domain events** — `IDomainEvent` (marker), `IDomainEventHandler<TDomainEvent>`, and `IDomainEventBus`
  whose default implementation dispatches to every registered handler.
- **`Specification<T>`** — supply a `ToExpression()` predicate; `IsSatisfiedBy(candidate)` evaluates it
  in-memory. The compiled delegate is **cached on first use** for the instance's lifetime (a benign
  compile-twice race is accepted; override `IsSatisfiedBy` if you need a dynamic expression).
- **`IRepository`** and **`IUnitOfWork`** (`Commit()` / `CommitAsync(...)`) — marker/coordination contracts
  you implement in your data layer.

Registration helpers on `IServiceCollection`: `RegisterDomainEventHandler<TEvent, THandler>()` (Transient),
`RegisterDefaultDomainEventBus()` (Singleton), `RegisterModuleDomainEventHandlers<TModule>()` (Scoped).

```csharp
using BigOX.Domain;
using Microsoft.Extensions.DependencyInjection;

public sealed class User : Entity<Guid>
{
    public User(Guid id) => Id = id;
}

public sealed record UserRegistered(Guid UserId) : IDomainEvent;

public sealed class SendWelcome : IDomainEventHandler<UserRegistered>
{
    public Task Handle(UserRegistered @event, CancellationToken ct = default) => Task.CompletedTask;
}

var provider = new ServiceCollection()
    .RegisterDefaultDomainEventBus()
    .RegisterDomainEventHandler<UserRegistered, SendWelcome>()
    .BuildServiceProvider();

await provider.GetRequiredService<IDomainEventBus>()
    .Publish(new UserRegistered(Guid.NewGuid()));

// A reusable specification:
public sealed class ActiveUsers : Specification<User>
{
    public override System.Linq.Expressions.Expression<Func<User, bool>> ToExpression()
        => u => u.Id != Guid.Empty;
}
```

---

### BigOX.Security

A framework-agnostic authorization engine that the CQRS decorators plug into.

You implement `IAuthorizationRule<TArgs>` — its `IsAuthorizedAsync(args, ct)` returns a
`ValueTask<AuthorizationResult>`. Return `AuthorizationResult.Failure("…")` (optionally with a stable
`code`) for **expected** denials and reserve exceptions for infrastructure failures. `IAuthorizationManager`
resolves every rule registered for the argument type, aggregates results into an
`AuthorizationEvaluationResult`, and (via `AuthorizeAsync`) throws `System.Security.SecurityException` when
any rule fails.

`AuthorizationOptions` controls behavior:

| Option | Default | Meaning |
| --- | --- | --- |
| `NoRulesBehavior` | `Error` | What to do when no rule is registered: `Allow`, `Deny`, or `Error` (throws). |
| `EvaluateRulesInParallel` | `false` | When `true` and >1 rule exists, rules run concurrently; failures are still collected **in registration order**. |

```csharp
using BigOX.Security;
using Microsoft.Extensions.DependencyInjection;

public sealed record DeleteOrder(Guid OrderId, bool IsAdmin);

public sealed class MustBeAdmin : IAuthorizationRule<DeleteOrder>
{
    public ValueTask<AuthorizationResult> IsAuthorizedAsync(DeleteOrder args, CancellationToken ct = default)
        => new(args.IsAdmin
            ? AuthorizationResult.Success()
            : AuthorizationResult.Failure("Admin role required.", code: "not_admin"));
}

var provider = new ServiceCollection()
    .AddAuthorizationSecurity(ServiceLifetime.Scoped, o =>
    {
        o.NoRulesBehavior = AuthorizationNoRulesBehavior.Deny;
        o.EvaluateRulesInParallel = true;
    })
    .AddScoped<IAuthorizationRule<DeleteOrder>, MustBeAdmin>()
    .BuildServiceProvider();

using var scope = provider.CreateScope();
var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

var evaluation = await auth.EvaluateAsync(new DeleteOrder(Guid.NewGuid(), IsAdmin: false));
Console.WriteLine(evaluation.IsSuccessful);          // False
Console.WriteLine(evaluation.Failures[0].Code);      // not_admin
// Or enforce and throw on failure:
await auth.AuthorizeAsync(new DeleteOrder(Guid.NewGuid(), IsAdmin: true));
```

The failure `Code` set on `AuthorizationResult.Failure(message, code)` flows through to
`AuthorizationFailure.Code`, so callers can branch on stable policy codes.

---

### BigOX.Validation

`Guard` throws the `ArgumentException` family for pre-condition checks; `PropertyGuard` mirrors every check
but captures the member name via `[CallerMemberName]` for property setters. Every check returns the validated
value for fluent chaining, and each has a `[CallerArgumentExpression]` `paramName` plus an optional
`exceptionMessage` override.

The string format/length guards accept `null` (returned unchanged) and carry
`[return: NotNullIfNotNull(nameof(value))]`, so `string x = Guard.Url(nonNull);` flows as non-null.

#### `Guard` / `PropertyGuard` catalog

| Check | Throws | When |
| --- | --- | --- |
| `NotNull<T>(value)` | `ArgumentNullException` | `value` is null. |
| `NotNull<T>(collection)` | `ArgumentNullException` | collection is null. |
| `NotNullOrEmpty(string?)` | `ArgumentNullException` / `ArgumentException` | null / empty. |
| `NotNullOrEmpty<T>(collection)` | `ArgumentNullException` / `ArgumentException` | null / no elements (lazy sources checked on first enumeration). |
| `NotNullOrEmpty(Guid?)` | `ArgumentNullException` / `ArgumentException` | null / `Guid.Empty`. |
| `NotNullOrWhiteSpace(string?)` | `ArgumentNullException` / `ArgumentException` | null / empty or whitespace. |
| `NotEmpty(Guid)` | `ArgumentException` | `Guid.Empty`. |
| `NotDefault<T>(value)` `where T : struct` | `ArgumentException` | equal to `default(T)`. |
| `Positive<T>` `where T : INumber<T>` | `ArgumentOutOfRangeException` | `value <= 0`. |
| `NonNegative<T>` | `ArgumentOutOfRangeException` | `value < 0`. |
| `NonZero<T>` | `ArgumentException` | `value == 0`. |
| `Minimum<T>(value, min)` `where T : IComparable<T>` | `ArgumentOutOfRangeException` | `value < min`. |
| `Maximum<T>(value, max)` | `ArgumentOutOfRangeException` | `value > max`. |
| `WithinRange<T>(value, min, max)` | `ArgumentException` / `ArgumentOutOfRangeException` | `min > max` / out of range. |
| `InFuture(DateTime, timeZone? = null)` | `ArgumentException` | not strictly in the future (UTC when no zone). |
| `InPast(DateTime, timeZone? = null)` | `ArgumentException` | in the future. |
| `Requires<T>(value, predicate)` | `ArgumentNullException` / `ArgumentException` | null predicate / predicate returns false. |
| `EmailAddress(string?)` | `ArgumentException` | non-null and not a valid email. |
| `MatchesRegex(string?, string pattern)` | `ArgumentNullException` / `ArgumentException` | null / empty pattern, or no match. |
| `MatchesRegex(string?, Regex regex)` | `ArgumentNullException` / `ArgumentException` / `RegexMatchTimeoutException` | null regex / no match / regex timeout. |
| `MatchesRegex(string?, string pattern, TimeSpan matchTimeout)` | `ArgumentNullException` / `ArgumentException` / `ArgumentOutOfRangeException` / `RegexMatchTimeoutException` | as above + invalid timeout / timeout elapses. |
| `Url(string?)` | `ArgumentException` | non-null and not an absolute `http`/`https` URL. |
| `Url(string?, string[] allowedSchemes)` | `ArgumentNullException` / `ArgumentException` | null schemes / empty schemes or scheme not allowed. |
| `MaxLength(string?, int)` | `ArgumentOutOfRangeException` / `ArgumentException` | negative limit / too long. |
| `MinLength(string?, int)` | `ArgumentOutOfRangeException` / `ArgumentException` | negative limit / too short. |
| `ExactLength(string?, int)` | `ArgumentOutOfRangeException` / `ArgumentException` | negative length / length differs. |
| `LengthWithinRange(string?, int min, int max)` | `ArgumentException` | config error (`max <= 0`, `min < 0`, `min > max`) or out of range. |

`Guard` additionally provides `ReadOnlySpan<char>` overloads of `MaxLength`, `MinLength`, `ExactLength`, and
`LengthWithinRange`. Spans can't be null, so these validate length only (same exception types as the string
overloads) and return the span. `PropertyGuard` mirrors the string checks but has no span overloads.

```csharp
using BigOX.Validation;

public sealed class Customer
{
    private string? _email;
    public string? Email
    {
        get => _email;
        set => _email = PropertyGuard.EmailAddress(value); // throws ArgumentException on invalid, allows null
    }

    public Customer(int age, string website)
    {
        Age = Guard.NonNegative(age);
        Website = Guard.Url(website, ["https"]); // only https accepted
    }

    public int Age { get; }
    public string Website { get; }
}
```

---

### BigOX.Extensions

Pragmatic, allocation-conscious extension methods on common BCL types. Most are C# 14 `extension(...)` block
members; a few are extension **properties**. Highlights per type:

**`ArrayExtensions`** — `ClearRange(index, length)`, `Clear(Range)` (span-based zeroing).

**`BooleanExtensions`** — `ToCustomString(trueValue = "True", falseValue = "False")`, `ToByte()`, `ToInt32()`.

**`ByteExtensions`** — `ToMemoryStream()` over `ReadOnlyMemory<byte>` (copy) or `byte[]` (zero-copy, optional
`writable`, optional `index`/`count` slice).

**`CollectionExtensions`** — `Shuffle` (Fisher–Yates), `AddUnique`, `RemoveWhere(predicate)`, `AddIf`,
`ContainsAny(params/span)`, `AddUniqueRange`.

**`ComparableExtensions`** — `IsBetween(lower, upper, inclusive = true)`, `Limit(max)`, `Limit(min, max)`.

**`DateOnlyExtensions`** — `PreviousDay`/`NextDay`, `GetDatesInRange`, `IsBetween`, `ToDateTime`, `Age`,
`AddWeeks`, `DaysInMonth`, `GetFirst/LastDateOfMonth`, `GetFirst/LastDateOfWeek`, `GetNumberOfDays`,
`IsAfter`/`IsBefore`/`IsToday`, `IsLeapDay`/`IsLeapYear`.

**`DateTimeExtensions`** — `ToDateOnly`/`ToTimeOnly`, `Age`, `AddWeeks`, month/week helpers, `SetTime` (three
overloads), `NextDay`/`PreviousDay`, `Elapsed`, `GetTimestamp` (ISO-8601 round-trip), `GetDatesInRange`, plus
`static GetNumberOfDaysInYear(year, culture?)`.

**`DayOfWeekExtensions`** — `AddDays(n = 1)` (wraps), `GetNextDays(count = 7)`.

**`DecimalExtensions`** — `ToCurrencyString(culture = "en-US")`, `ToPercentageString(decimals = 2, culture)`,
`RoundTo(decimals)`, `ToWords()`, `IsWholeNumber()`, `Abs()`, and `static ToDouble(this decimal?)`.

**`DictionaryExtensions`** — `ToSortedDictionary()` / `ToSortedDictionary(comparer)`, `RemoveWhere(predicate)`,
`Merge(other, overwriteExisting = true)`.

**`DoubleExtensions`** — `ToDecimal(this double?)` (throws `OverflowException` for `NaN`/`Infinity`/overflow).

**`EnumExtensions`** — `ToDictionary<T>()` (description → name; throws on duplicate descriptions),
`GetEnumDescription()`, `GetEnumDisplay()` (read `[Description]` / `[Display]`, cached per enum type).

**`EnumerableExtensions`** — `IsEmpty` / `IsNotEmpty` / `IsNullOrEmpty` / `IsNotNullOrEmpty` (generic and
non-generic, with nullable-flow attributes) and a streaming `Chunk(chunkSize)`.

> **`Chunk` caveat:** the returned chunks share one underlying enumerator, so consume each chunk fully, in
> order, before advancing — never buffer the outer sequence (`chunks.ToList()`) or read chunks out of order.
> Because `System.Linq.Enumerable` also defines `Chunk`, importing both namespaces yields a CS0121 ambiguity;
> qualify as `EnumerableExtensions.Chunk(source, size)`.

**`GuidExtensions`** — `IsEmpty` / `IsNotEmpty` (extension properties).

**`ReadOnlyDictionaryExtensions`** — `FreezeOrEmpty()` returns an immutable `FrozenDictionary` (or the shared
empty one), preserving the source comparer.

**`StreamExtensions`** — `ToByteArray()` / `ToByteArrayAsync(ct)` (fast-path for `MemoryStream`).

**`StringBuilderExtensions`** — `IsEmpty(countWhiteSpace = false)`, `AppendCharToLength`, `ReduceToLength`,
`Reverse`, `EnsureStartsWith`/`EnsureEndsWith`, `AppendMultiple`, `RemoveAllOccurrences`, `Trim`,
`AppendFormatLine`, `AppendMultipleLines`.

**`StringExtensions`** — properties `IsGuid`, `IsValidEmail`, `IsValidWebsiteUrl`, `IsWhiteSpace`; methods
`ExtractDigits`, `ReduceToLength`, `EnsureStartsWith`/`EnsureEndsWith`, `RemoveWhitespace`, `ToStringBuilder`,
`AppendCharToLength`, `IsDateTime`, `LimitLength`.

**`TimeSpanExtensions`** — `ToTimeOnly()` on `TimeSpan` / `TimeSpan?` (modulo 24h; negatives wrap positive).

**`TypeExtensions`** — `IsNumeric(includeNullableTypes = true)`, `IsOpenGeneric()`, `HasAttribute(...)`,
`DefaultValue()` / `DefaultValueAsync()`, `GetTypeAsString()` (C# aliases), `IsNullable()`,
`IsOfNullableType<T>()`, `static DefaultValue<T>()`.

```csharp
using BigOX.Extensions;

((double?)3.14).ToDecimal();         // extension on double? → decimal?
"  A1B2  ".ExtractDigits();          // "12"
DayOfWeek.Friday.AddDays(3);         // Monday (wraps)
typeof(int?).GetTypeAsString();      // "int?"
EnumerableExtensions.Chunk(new[] { 1, 2, 3, 4, 5 }, 2); // [1,2],[3,4],[5] — qualified to avoid CS0121
```

---

### BigOX.Types

- **`DateRange`** — an inclusive `DateOnly` range with an optional open (infinite) end, serialized by
  `DateRangeConverter` as the canonical string `yyyy-MM-dd|yyyy-MM-dd` (or `…|∞`). Implements `IEquatable`,
  `ISpanFormattable`, `ISpanParsable`. `DateRangeExtensions` adds `Duration`/`TryGetDuration`, `Contains`,
  `Overlaps`, `Intersection`, `GetWeeksInRange`, `EnumerateDays`.
- **`EmailAddress`** — an immutable, normalized value object (lowercase address, title-cased display name;
  equality is case-insensitive on the address only). Implements `IComparable`, `IEquatable`, `IFormattable`,
  `IParsable` **and `ISpanParsable`**, with `<`, `<=`, `>`, `>=` operators. Build via `From`/`Parse`/`TryParse`
  (string or span). `EmailAddressExtensions` adds `ToDisplayString`, `HasDisplayName`, `Username`, `Domain`,
  `Host`, and `ToMailAddress(...)`.
- **`DisposableObject`** — a thread-safe base implementing `IDisposable` + `IAsyncDisposable` with a tri-state
  lifecycle, running cleanup exactly once. Override `DisposeManagedResources` / `DisposeAsyncCore` /
  `DisposeUnmanagedResources`; call `ThrowIfDisposed()` at the top of public members.

```csharp
using BigOX.Types;

var range = DateRange.Create(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
Console.WriteLine(range.Duration());              // 31 (inclusive)
Console.WriteLine(range.ToString());              // 2026-01-01|2026-01-31

var email = EmailAddress.From("USER@Example.com", "  Jane DOE ");
Console.WriteLine(email.Address);                 // user@example.com
Console.WriteLine(email.ToString());              // Jane Doe <user@example.com>
EmailAddress.TryParse("a@b.com".AsSpan(), null, out var parsed); // ISpanParsable
```

---

### BigOX.Factories

- **`GuidFactory`** — sequential (UUID v7) GUIDs: `NewSequentialGuid()`,
  `NewSequentialGuid(DateTimeOffset timestamp)` (embeds a specific time-ordered prefix), the lazy
  `NewSequentialGuids(int count)`, and the allocation-free `NewSequentialGuids(Span<Guid> destination)`.
  Great for DB-friendly clustered keys with better locality than `Guid.NewGuid()`.
- **`TransactionFactory`** — `CreateTransaction(isolationLevel = ReadCommitted,
  transactionScopeOption = Required, transactionScopeAsyncFlowOption = Enabled, timeOut? = null)` builds a
  configured `System.Transactions.TransactionScope`.

```csharp
using BigOX.Factories;

Span<Guid> ids = stackalloc Guid[4];
GuidFactory.NewSequentialGuids(ids); // fills all four, no allocation

using var tx = TransactionFactory.CreateTransaction();
// … work …
tx.Complete();
```

*(A `CultureInfoFactory` exists as an internal helper behind the culture-aware `DecimalExtensions` formatting;
it is not part of the public API.)*

---

### BigOX.DependencyInjection

`IModule` is the unit of composition — a module registers its own services via `Initialize(IServiceCollection)`
and can receive an `IConfiguration`. `ServiceCollectionExtensions` provides:

- `AddModule<TModule>(configuration?)` — initialize a single module (`where TModule : IModule, new()`).
- `AddAllModules(configuration?)` — discover and initialize every `IModule` across loaded assemblies (and DLLs
  in the app base directory); uninstantiable types are skipped.
- `AddTypesFromAssembly<TAssemblyType, TBase>(lifetime = Transient)` — Scrutor-based scan registering classes
  assignable to `TBase` as their implemented interfaces.

```csharp
using BigOX.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public sealed class BillingModule : IModule
{
    public IConfiguration? Configuration { set { } }
    public void Initialize(IServiceCollection services) =>
        services.RegisterCommandHandler<CreateUser, CreateUserHandler>();
}

var services = new ServiceCollection().AddModule<BillingModule>();
```

---

## Compatibility

BigO X targets **`net10.0`** and uses .NET 10 SDK / C# 14 features.

| Aspect | Status | Notes |
| --- | --- | --- |
| .NET | ✅ `.NET 10` only | Uses .NET 10 / C# 14 features. |
| OS / runtime | ✅ Cross-platform | Works wherever `net10.0` runs, incl. computed TFMs (`net10.0-android`, `-browser`, `-ios`, `-windows`, …). |
| Nullable | ✅ Fully annotated | Nullable reference types enabled throughout. |
| Trimming / NativeAOT | ⚠️ Not annotated | The DI registration helpers use assembly scanning and reflection; the library is not currently trim/AOT-annotated. Core result/error types are simple generics, but validate your DI wiring under trimming. |
| .NET 6/7/8/9, Unity | ❌ Not supported | `net10.0` only, by design. |

---

## Performance & design principles

- **Explicit over implicit.** Failures are values, not exceptions. Control flow stays visible.
- **Allocation-aware.** Results are `readonly struct`s; failure propagation reuses the already-validated error
  array instead of re-cloning it; guards use spans and avoid LINQ on hot paths.
- **Inlining where it counts.** Trivial hot helpers are marked `[MethodImpl(AggressiveInlining)]`.
- **Immutable by default.** `Error`, `ErrorKind`, `EmailAddress`, `DateRange`, and result metadata are
  immutable; metadata dictionaries are frozen.
- **Async hygiene.** Every internal `await` uses `ConfigureAwait(false)`.
- **Dependency-light.** BCL + `Microsoft.Extensions.*` + Scrutor only; `System.Text.Json` for serialization.

There is no published BenchmarkDotNet suite yet — contributions of a `BigOX.Benchmarks` project are welcome.

---

## Configuration & extensibility

Extend BigO X types with your own C# 14 extension blocks:

```csharp
using BigOX.Results;

public static class OrderResultExtensions
{
    extension<T>(Result<T> result)
    {
        public bool IsOk => result.IsSuccess(out _);
    }
}
```

Compose your error taxonomy with `ErrorKind.FromString("payment")` and carry correlation IDs / external codes
in error and result `Metadata`. BigO X forces no hosting model — use it in ASP.NET Core, worker services,
console apps, or background jobs, and integrate with the `Microsoft.Extensions.*` stack as needed.

---

## Versioning policy

- **Package ID:** `BigOX` · **License:** MIT.
- **SemVer:** MAJOR = breaking public-API change, MINOR = backward-compatible additions, PATCH = fixes.
- The public API surface is guarded by **Microsoft.CodeAnalysis.PublicApiAnalyzers**
  (`PublicAPI.Shipped.txt`), so accidental breaking changes fail the build.
- Version history: <https://www.nuget.org/packages/BigOX#versions-tab> ·
  commits: <https://github.com/omarbesiso/BigOX/commits/main>

---

## Contributing

Contributions are welcome — see **[CONTRIBUTING.md](CONTRIBUTING.md)** for prerequisites, build/test/coverage
commands, the coding conventions, and the public-API update workflow. In short:

```bash
git clone https://github.com/omarbesiso/BigOX.git
cd BigOX
dotnet test src/BigOX.slnx
```

---

## Security

See **[SECURITY.md](SECURITY.md)**. Report non-sensitive bugs via
[GitHub Issues](https://github.com/omarbesiso/BigOX/issues) and potential vulnerabilities privately through the
**"Contact owners"** channel on the [NuGet page](https://www.nuget.org/packages/BigOX). BigO X handles no
secrets, crypto, or network/I-O of its own; the main consideration is avoiding sensitive data in error
messages and metadata you log or serialize.

---

## License

Licensed under the **[MIT License](LICENSE)** · © Omar Besiso (BigO).
Free for commercial and open-source use.

---

## FAQ

**Why another `Result` library?**
BigO X is `.NET 10 / C# 14`-only, with a typed-error model (`IError`/`Error`/`ErrorKind` + metadata), a full
combinator set, and a cohesive spine (CQRS, DDD, authorization) around it — a focused toolkit, not a grab-bag.

**Does it throw exceptions?**
Not for control flow — observe failures through `Result`. Exceptions are reserved for programming errors
(null arguments, out-of-range values) and infrastructure failures from dependencies.

**Can I use it in ASP.NET Core / workers / Blazor / MAUI?**
Yes, on `net10.0`. It's framework-agnostic and integrates with `Microsoft.Extensions.*`.

**Does it support .NET 6/7/8/9?**
No — `net10.0` only, deliberately.

**Is it trimming / NativeAOT friendly?**
The core result/error types are simple generics, but the DI helpers rely on Scrutor assembly scanning and
reflection and are not currently trim/AOT-annotated. Validate your DI configuration and add preservation hints
if you ship trimmed or NativeAOT.
