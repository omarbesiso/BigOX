using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BigOX.Extensions;

namespace BigOX.Results;

/// <summary>
///     Value-carrying result wrapper with default <see cref="Error" /> error type.
/// </summary>
/// <typeparam name="T">Type of the success value.</typeparam>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct Result<T> : IResult<T>
{
    private readonly Result<T, Error> _inner;

    private Result(Result<T, Error> inner)
    {
        _inner = inner;
    }

    /// <summary>
    ///     Optional human-readable message associated with the result.
    /// </summary>
    public string? Message => _inner.Message;

    /// <summary>
    ///     Gets the current status of the result.
    /// </summary>
    public ResultStatus Status => _inner.Status;

    /// <summary>
    ///     Untyped view of errors (empty when not failure).
    /// </summary>
    IReadOnlyList<IError> IResult.Errors => ((IResult)_inner).Errors;

    /// <summary>
    ///     Immutable result-level metadata bag.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata => _inner.Metadata;

    /// <summary>
    ///     Success value (null/default when not in a success state).
    /// </summary>
    public T? Value => _inner.Value;

    /// <summary>
    ///     True when success; outputs the value safely.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSuccess([MaybeNullWhen(false)] out T value) => _inner.IsSuccess(out value);

    /// <summary>
    ///     True when failure; outputs the error list safely.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFailure([NotNullWhen(true)] out IReadOnlyList<Error>? errors) => _inner.IsFailure(out errors);

    /// <summary>
    ///     Pattern matches on success/failure invoking handlers.
    /// </summary>
    /// <typeparam name="TResult">Return type of handlers.</typeparam>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="onSuccess" /> or <paramref name="onFailure" /> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure) =>
        _inner.Match(onSuccess, onFailure);

    /// <summary>
    ///     Maps the success value preserving errors and metadata.
    /// </summary>
    /// <typeparam name="TNext">Mapped value type.</typeparam>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="map" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TNext> Map<TNext>(Func<T, TNext> map) => new(_inner.Map(map));

    /// <summary>
    ///     Monadic bind chaining another result-producing function.
    /// </summary>
    /// <typeparam name="TNext">Next value type.</typeparam>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bind" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> bind)
    {
        if (bind is null)
        {
            throw new ArgumentNullException(nameof(bind));
        }

        return _inner.IsSuccess(out var v) ? bind(v) : new Result<TNext>(_inner.AsFailure<TNext>());
    }

    /// <summary>
    ///     Invokes <paramref name="action" /> with the success value and returns the original result unchanged.
    /// </summary>
    /// <param name="action">Side-effecting action executed only when the result is a success.</param>
    /// <returns>The original result, enabling fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T> Tap(Action<T> action) => new(_inner.Tap(action));

    /// <summary>
    ///     Invokes <paramref name="action" /> with the error list and returns the original result unchanged.
    /// </summary>
    /// <param name="action">Side-effecting action executed only when the result is a failure.</param>
    /// <returns>The original result, enabling fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T> TapError(Action<IReadOnlyList<Error>> action) => new(_inner.TapError(action));

    /// <summary>
    ///     Ensures the success value satisfies <paramref name="predicate" />, converting the result to a failure
    ///     carrying <paramref name="error" /> when it does not.
    /// </summary>
    /// <param name="predicate">Condition the success value must satisfy.</param>
    /// <param name="error">Error used to build the failure when the predicate is not satisfied.</param>
    /// <returns>
    ///     The original result when it is a success satisfying the predicate or an existing failure; otherwise a new
    ///     failure carrying <paramref name="error" /> together with the original message and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="predicate" /> or <paramref name="error" /> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T> Ensure(Func<T, bool> predicate, Error error) => new(_inner.Ensure(predicate, error));

    /// <summary>
    ///     Asynchronously maps the success value while preserving errors, message and metadata.
    /// </summary>
    /// <typeparam name="TNext">Mapped value type.</typeparam>
    /// <param name="map">Asynchronous projection applied to the success value.</param>
    /// <returns>A task producing the mapped result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="map" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    public async Task<Result<TNext>> MapAsync<TNext>(Func<T, Task<TNext>> map) =>
        new(await _inner.MapAsync(map).ConfigureAwait(false));

    /// <summary>
    ///     Asynchronously binds the success value to another result while preserving failures.
    /// </summary>
    /// <typeparam name="TNext">Next value type.</typeparam>
    /// <param name="bind">Asynchronous result-producing continuation.</param>
    /// <returns>A task producing the bound result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bind" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    public async Task<Result<TNext>> BindAsync<TNext>(Func<T, Task<Result<TNext>>> bind)
    {
        if (bind is null)
        {
            throw new ArgumentNullException(nameof(bind));
        }

        return _inner.IsSuccess(out var v)
            ? await bind(v).ConfigureAwait(false)
            : new Result<TNext>(_inner.AsFailure<TNext>());
    }

    /// <summary>
    ///     Creates a success result.
    /// </summary>
    public static Result<T> Success(T value, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(Result<T, Error>.Success(value, message, metadata));

    /// <summary>
    ///     Creates a failure result from a sequence of errors.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errors" /> is empty.</exception>
    public static Result<T> Failure(IEnumerable<Error> errors, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(Result<T, Error>.Failure(errors, message, metadata));

    /// <summary>
    ///     Creates a failure result from a single error.
    /// </summary>
    public static Result<T> Failure(Error error, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(Result<T, Error>.Failure(error, message, metadata));

    /// <summary>
    ///     Implicit conversion from <see cref="Error" /> to a failure result.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);

    /// <summary>
    ///     Debugger display string.
    /// </summary>
    private string DebuggerDisplay => _inner.Status == ResultStatus.Success
        ? $"Success: {(_inner.Value is null ? "null" : _inner.Value.ToString())}"
        : _inner.Status == ResultStatus.Failure
            ? $"Failure[{((IResult)_inner).Errors.Count}]"
            : "Uninitialized";
}

/// <summary>
///     Result without a value payload (unit) with default <see cref="Error" /> type.
/// </summary>
public readonly record struct Result : IResult
{
    private readonly Result<Unit, Error> _inner;

    private Result(Result<Unit, Error> inner)
    {
        _inner = inner;
    }

    /// <summary>
    ///     Optional human-readable message associated with the result.
    /// </summary>
    public string? Message => _inner.Message;

    /// <summary>
    ///     Gets the current status of the result.
    /// </summary>
    public ResultStatus Status => _inner.Status;

    /// <summary>
    ///     Untyped error list (empty when not failure).
    /// </summary>
    IReadOnlyList<IError> IResult.Errors => ((IResult)_inner).Errors;

    /// <summary>
    ///     Immutable result-level metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata => _inner.Metadata;

    /// <summary>
    ///     Pattern matches on success/failure invoking the corresponding handler.
    /// </summary>
    /// <typeparam name="TResult">Return type of the handlers.</typeparam>
    /// <param name="onSuccess">Invoked when the result is a success.</param>
    /// <param name="onFailure">Invoked with the error list when the result is a failure.</param>
    /// <returns>The value produced by the invoked handler.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="onSuccess" /> or <paramref name="onFailure" /> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        if (onSuccess is null)
        {
            throw new ArgumentNullException(nameof(onSuccess));
        }

        if (onFailure is null)
        {
            throw new ArgumentNullException(nameof(onFailure));
        }

        return _inner.Match(_ => onSuccess(), onFailure);
    }

    /// <summary>
    ///     Deconstructs the result into its success flag and error list.
    /// </summary>
    /// <param name="isSuccess">True when the result is a success.</param>
    /// <param name="errors">The errors when the result is a failure; otherwise null.</param>
    public void Deconstruct(out bool isSuccess, out IReadOnlyList<Error>? errors)
    {
        isSuccess = _inner.IsSuccess(out _);
        errors = _inner.IsFailure(out var e) ? e : null;
    }

    /// <summary>
    ///     Attempts to retrieve the error list, succeeding only when the result is a failure.
    /// </summary>
    /// <param name="errors">The errors when the result is a failure; otherwise null.</param>
    /// <returns>True when the result is a failure; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetErrors([NotNullWhen(true)] out IReadOnlyList<Error>? errors) => _inner.IsFailure(out errors);

    /// <summary>
    ///     Creates a success (no-value) result.
    /// </summary>
    public static Result Success(string? message = null, IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(Result<Unit, Error>.Success(Unit.Value, message, metadata));

    /// <summary>
    ///     Creates a failure result from a sequence of errors.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errors" /> is empty.</exception>
    public static Result Failure(IEnumerable<Error> errors, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(Result<Unit, Error>.Failure(errors, message, metadata));

    /// <summary>
    ///     Creates a failure result from a single error.
    /// </summary>
    public static Result Failure(Error error, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(Result<Unit, Error>.Failure(error, message, metadata));

    /// <summary>
    ///     Implicit conversion from <see cref="Error" /> to a failure result.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);

    private readonly struct Unit
    {
        public static readonly Unit Value = new();
    }
}

/// <summary>
///     Generic result with strongly-typed error items.
/// </summary>
/// <typeparam name="TValue">Type of the success value.</typeparam>
/// <typeparam name="TError">Type of the error items (must implement <see cref="IError" />).</typeparam>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct Result<TValue, TError> : IResult<TValue, TError> where TError : IError
{
    private readonly byte _state; // 0=Uninitialized,1=Success,2=Failure
    private readonly TValue? _value;
    private readonly TError[]? _errors;
    private readonly IReadOnlyList<TError>? _errorsRo;
    private static readonly TError[] EmptyErrorsArray = [];
    private static readonly IReadOnlyList<TError> EmptyErrorsRo = Array.AsReadOnly(EmptyErrorsArray);

    /// <summary>
    ///     Success-state constructor.
    /// </summary>
    private Result(TValue value, string? message, IReadOnlyDictionary<string, object?>? metadata)
    {
        _state = 1;
        _value = value;
        _errors = null;
        _errorsRo = null;
        Message = message;
        Metadata = metadata.FreezeOrEmpty();
    }

    /// <summary>
    ///     Failure-state constructor (clones unless alreadyCloned is true).
    /// </summary>
    private Result(TError[] errors, bool alreadyCloned, string? message, IReadOnlyDictionary<string, object?>? metadata)
    {
        if (errors is null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        if (errors.Length == 0)
        {
            throw new ArgumentException("Failure must contain at least one error.", nameof(errors));
        }

        var cloned = alreadyCloned ? errors : (TError[])errors.Clone();
        _errors = cloned;
        _errorsRo = Array.AsReadOnly(cloned);
        _value = default;
        _state = 2;
        Message = message;
        Metadata = metadata.FreezeOrEmpty();
    }

    /// <summary>
    ///     Optional human-readable message associated with the result.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    ///     Gets result status (success/failure/uninitialized).
    /// </summary>
    public ResultStatus Status => _state switch
    {
        1 => ResultStatus.Success, 2 => ResultStatus.Failure, _ => ResultStatus.Uninitialized
    };

    /// <summary>
    ///     Untyped error list (empty when not failure).
    /// </summary>
    IReadOnlyList<IError> IResult.Errors => (IReadOnlyList<IError>)(_errorsRo ?? EmptyErrorsRo);

    /// <summary>
    ///     Immutable metadata bag.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; }

    /// <summary>
    ///     Success value (default when not success state).
    /// </summary>
    public TValue? Value => _state == 1 ? _value! : default;

    /// <summary>
    ///     Strongly-typed error list (empty when not failure).
    /// </summary>
    public IReadOnlyList<TError> Errors => _errorsRo ?? EmptyErrorsRo;

    /// <summary>
    ///     True when success; outputs the value safely.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSuccess([MaybeNullWhen(false)] out TValue value)
    {
        if (_state == 1)
        {
            value = _value!;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    ///     True when failure; outputs the errors safely.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFailure([NotNullWhen(true)] out IReadOnlyList<TError>? errors)
    {
        if (_state == 2)
        {
            errors = _errorsRo!;
            return true;
        }

        errors = null;
        return false;
    }

    /// <summary>
    ///     First error when in failure state.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when not in failure state.</exception>
    public TError FirstError => _state switch
    {
        2 => _errors![0],
        0 => throw new InvalidOperationException("Result is uninitialized."),
        _ => throw new InvalidOperationException("Result is in a success state.")
    };

    /// <summary>
    ///     Pattern matches invoking success or failure handler.
    /// </summary>
    /// <typeparam name="TResult">Return type.</typeparam>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="onSuccess" /> or <paramref name="onFailure" /> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<IReadOnlyList<TError>, TResult> onFailure)
    {
        if (onSuccess is null)
        {
            throw new ArgumentNullException(nameof(onSuccess));
        }

        if (onFailure is null)
        {
            throw new ArgumentNullException(nameof(onFailure));
        }

        return _state switch
        {
            1 => onSuccess(_value!),
            2 => onFailure(_errorsRo!),
            _ => throw new InvalidOperationException("Result is uninitialized.")
        };
    }

    /// <summary>
    ///     Maps the success value preserving errors/metadata/message.
    /// </summary>
    /// <typeparam name="TNext">Target mapped value type.</typeparam>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="map" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TNext, TError> Map<TNext>(Func<TValue, TNext> map)
    {
        if (map is null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        return _state switch
        {
            1 => Result<TNext, TError>.Success(map(_value!), Message, Metadata),
            2 => Result<TNext, TError>.FromOwnedErrors(_errors!, Message, Metadata),
            _ => throw new InvalidOperationException("Result is uninitialized.")
        };
    }

    /// <summary>
    ///     Monadic bind chaining another result-producing function.
    /// </summary>
    /// <typeparam name="TNext">Next value type.</typeparam>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bind" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TNext, TError> Bind<TNext>(Func<TValue, Result<TNext, TError>> bind)
    {
        if (bind is null)
        {
            throw new ArgumentNullException(nameof(bind));
        }

        return _state switch
        {
            1 => bind(_value!),
            2 => Result<TNext, TError>.FromOwnedErrors(_errors!, Message, Metadata),
            _ => throw new InvalidOperationException("Result is uninitialized.")
        };
    }

    /// <summary>
    ///     Projects a failure into another value type preserving errors.
    /// </summary>
    /// <typeparam name="TNext">New value type.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown when the result is not in a failure state.</exception>
    public Result<TNext, TError> AsFailure<TNext>() =>
        _state == 2
            ? Result<TNext, TError>.FromOwnedErrors(_errors!, Message, Metadata)
            : throw new InvalidOperationException("AsFailure can only be called on a failure result.");

    /// <summary>
    ///     Invokes <paramref name="action" /> with the success value and returns the original result unchanged.
    /// </summary>
    /// <param name="action">Side-effecting action executed only when the result is a success.</param>
    /// <returns>The original result, enabling fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    public Result<TValue, TError> Tap(Action<TValue> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        switch (_state)
        {
            case 1:
                action(_value!);
                return this;
            case 2:
                return this;
            default:
                throw new InvalidOperationException("Result is uninitialized.");
        }
    }

    /// <summary>
    ///     Invokes <paramref name="action" /> with the error list and returns the original result unchanged.
    /// </summary>
    /// <param name="action">Side-effecting action executed only when the result is a failure.</param>
    /// <returns>The original result, enabling fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    public Result<TValue, TError> TapError(Action<IReadOnlyList<TError>> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        switch (_state)
        {
            case 1:
                return this;
            case 2:
                action(_errorsRo!);
                return this;
            default:
                throw new InvalidOperationException("Result is uninitialized.");
        }
    }

    /// <summary>
    ///     Ensures the success value satisfies <paramref name="predicate" />, converting the result to a failure
    ///     carrying <paramref name="error" /> when it does not.
    /// </summary>
    /// <param name="predicate">Condition the success value must satisfy.</param>
    /// <param name="error">Error used to build the failure when the predicate is not satisfied.</param>
    /// <returns>
    ///     The original result when it is a success satisfying the predicate or an existing failure; otherwise a new
    ///     failure carrying <paramref name="error" /> together with the original message and metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="predicate" /> or <paramref name="error" /> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    public Result<TValue, TError> Ensure(Func<TValue, bool> predicate, TError error)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        return _state switch
        {
            1 => predicate(_value!) ? this : Failure(error, Message, Metadata),
            2 => this,
            _ => throw new InvalidOperationException("Result is uninitialized.")
        };
    }

    /// <summary>
    ///     Transforms every error with <paramref name="map" /> while preserving the value (on success), message and
    ///     metadata.
    /// </summary>
    /// <typeparam name="TNextError">Target error type.</typeparam>
    /// <param name="map">Projection applied to each error when the result is a failure.</param>
    /// <returns>
    ///     A success result carrying the same value when this result is a success; otherwise a failure whose errors are
    ///     the projections of this result's errors.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="map" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    public Result<TValue, TNextError> MapError<TNextError>(Func<TError, TNextError> map) where TNextError : IError
    {
        if (map is null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        switch (_state)
        {
            case 1:
                return Result<TValue, TNextError>.Success(_value!, Message, Metadata);
            case 2:
            {
                var source = _errors!;
                var mapped = new TNextError[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    mapped[i] = map(source[i]);
                }

                return Result<TValue, TNextError>.FromOwnedErrors(mapped, Message, Metadata);
            }
            default:
                throw new InvalidOperationException("Result is uninitialized.");
        }
    }

    /// <summary>
    ///     Asynchronously maps the success value while preserving errors, message and metadata.
    /// </summary>
    /// <typeparam name="TNext">Mapped value type.</typeparam>
    /// <param name="map">Asynchronous projection applied to the success value.</param>
    /// <returns>A task producing the mapped result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="map" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    public async Task<Result<TNext, TError>> MapAsync<TNext>(Func<TValue, Task<TNext>> map)
    {
        if (map is null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        switch (_state)
        {
            case 1:
            {
                var next = await map(_value!).ConfigureAwait(false);
                return Result<TNext, TError>.Success(next, Message, Metadata);
            }
            case 2:
                return Result<TNext, TError>.FromOwnedErrors(_errors!, Message, Metadata);
            default:
                throw new InvalidOperationException("Result is uninitialized.");
        }
    }

    /// <summary>
    ///     Asynchronously binds the success value to another result while preserving failures.
    /// </summary>
    /// <typeparam name="TNext">Next value type.</typeparam>
    /// <param name="bind">Asynchronous result-producing continuation.</param>
    /// <returns>A task producing the bound result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bind" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result is uninitialized.</exception>
    public async Task<Result<TNext, TError>> BindAsync<TNext>(Func<TValue, Task<Result<TNext, TError>>> bind)
    {
        if (bind is null)
        {
            throw new ArgumentNullException(nameof(bind));
        }

        switch (_state)
        {
            case 1:
                return await bind(_value!).ConfigureAwait(false);
            case 2:
                return Result<TNext, TError>.FromOwnedErrors(_errors!, Message, Metadata);
            default:
                throw new InvalidOperationException("Result is uninitialized.");
        }
    }

    /// <summary>
    ///     Creates a success result.
    /// </summary>
    public static Result<TValue, TError> Success(TValue value, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new(value, message, metadata);

    /// <summary>
    ///     Creates a failure result from a sequence of errors (optimized cloning path).
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errors" /> is empty.</exception>
    public static Result<TValue, TError> Failure(IEnumerable<TError> errors, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        if (errors is null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        if (errors is TError[] arr)
        {
            if (arr.Length == 0)
            {
                throw new ArgumentException("Failure must contain at least one error.", nameof(errors));
            }

            var cloned = (TError[])arr.Clone();
            return new Result<TValue, TError>(cloned, true, message, metadata);
        }

        if (errors is ICollection<TError> coll)
        {
            if (coll.Count == 0)
            {
                throw new ArgumentException("Failure must contain at least one error.", nameof(errors));
            }

            var buffer = new TError[coll.Count];
            coll.CopyTo(buffer, 0);
            return new Result<TValue, TError>(buffer, true, message, metadata);
        }

        var list = new List<TError>();
        foreach (var e in errors)
        {
            list.Add(e);
        }

        if (list.Count == 0)
        {
            throw new ArgumentException("Failure must contain at least one error.", nameof(errors));
        }

        return new Result<TValue, TError>(list.ToArray(), true, message, metadata);
    }

    /// <summary>
    ///     Creates a failure result from a single error.
    /// </summary>
    public static Result<TValue, TError> Failure(TError error, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null) =>
        new([error], true, message, metadata);

    /// <summary>
    ///     Creates a failure result from a params array.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errors" /> is empty.</exception>
    public static Result<TValue, TError> Failure(string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null, params TError[] errors) =>
        new(errors ?? throw new ArgumentNullException(nameof(errors)), false, message,
            metadata);

    /// <summary>
    ///     Creates a failure result that takes direct ownership of <paramref name="errors" /> without cloning it.
    /// </summary>
    /// <remarks>
    ///     Internal fast path used by the failure-propagating combinators (<see cref="Map{TNext}" />,
    ///     <see cref="Bind{TNext}" />, <see cref="AsFailure{TNext}" />, <see cref="MapError{TNextError}" /> and the async
    ///     variants). Callers pass a private, already-validated, non-empty array that is never mutated after creation;
    ///     because the array is only ever surfaced through the read-only <see cref="Errors" /> view, storing it by
    ///     reference instead of cloning is safe and avoids an allocation per hop.
    /// </remarks>
    /// <param name="errors">A non-null, non-empty array whose ownership transfers to the new result.</param>
    /// <param name="message">Optional message to carry.</param>
    /// <param name="metadata">Optional metadata to carry.</param>
    /// <returns>A failure result wrapping <paramref name="errors" /> directly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errors" /> is empty.</exception>
    internal static Result<TValue, TError> FromOwnedErrors(TError[] errors, string? message,
        IReadOnlyDictionary<string, object?>? metadata) =>
        new(errors, true, message, metadata);

    /// <summary>
    ///     Implicit conversion from error to failure result.
    /// </summary>
    public static implicit operator Result<TValue, TError>(TError error) => Failure(error);

    /// <summary>
    ///     Deconstructs into (isSuccess, value, errors) for pattern matching.
    /// </summary>
    /// <param name="isSuccess">True when success.</param>
    /// <param name="value">Value if success; default otherwise.</param>
    /// <param name="errors">Errors if failure; null otherwise.</param>
    public void Deconstruct(out bool isSuccess, out TValue? value, out IReadOnlyList<TError>? errors)
    {
        isSuccess = _state == 1;
        if (isSuccess)
        {
            value = _value!;
            errors = null;
        }
        else if (_state == 2)
        {
            value = default;
            errors = _errorsRo!;
        }
        else
        {
            value = default;
            errors = null;
        }
    }

    /// <summary>
    ///     Debugger display string.
    /// </summary>
    private string DebuggerDisplay => _state switch
    {
        1 => $"Success: {(_value is null ? "null" : _value.ToString())}",
        2 => $"Failure[{_errors!.Length}] {FirstError.Kind}: {FirstError.Code} - {FirstError.ErrorMessage}",
        _ => "Uninitialized"
    };
}