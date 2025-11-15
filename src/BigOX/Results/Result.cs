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
    ///     Success value (null/default when failure).
    /// </summary>
    public T? Value => _inner.Value;

    /// <summary>
    ///     True when success; outputs the value safely.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSuccess([MaybeNullWhen(false)] out T value)
    {
        return _inner.IsSuccess(out value);
    }

    /// <summary>
    ///     True when failure; outputs the error list safely.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFailure([NotNullWhen(true)] out IReadOnlyList<Error>? errors)
    {
        return _inner.IsFailure(out errors);
    }

    /// <summary>
    ///     Pattern matches on success/failure invoking handlers.
    /// </summary>
    /// <typeparam name="TResult">Return type of handlers.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        return _inner.Match(onSuccess, onFailure);
    }

    /// <summary>
    ///     Maps the success value preserving errors and metadata.
    /// </summary>
    /// <typeparam name="TNext">Mapped value type.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TNext> Map<TNext>(Func<T, TNext> map)
    {
        return new Result<TNext>(_inner.Map(map));
    }

    /// <summary>
    ///     Monadic bind chaining another result-producing function.
    /// </summary>
    /// <typeparam name="TNext">Next value type.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> bind)
    {
        return _inner.IsSuccess(out var v) ? bind(v) : new Result<TNext>(_inner.AsFailure<TNext>());
    }

    /// <summary>
    ///     Creates a success result.
    /// </summary>
    public static Result<T> Success(T value, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new Result<T>(Result<T, Error>.Success(value, message, metadata));
    }

    /// <summary>
    ///     Creates a failure result from a sequence of errors.
    /// </summary>
    public static Result<T> Failure(IEnumerable<Error> errors, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new Result<T>(Result<T, Error>.Failure(errors, message, metadata));
    }

    /// <summary>
    ///     Creates a failure result from a single error.
    /// </summary>
    public static Result<T> Failure(Error error, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new Result<T>(Result<T, Error>.Failure(error, message, metadata));
    }

    /// <summary>
    ///     Implicit conversion from <see cref="Error" /> to a failure result.
    /// </summary>
    public static implicit operator Result<T>(Error error)
    {
        return Failure(error);
    }

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
    ///     Creates a success (no-value) result.
    /// </summary>
    public static Result Success(string? message = null, IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new Result(Result<Unit, Error>.Success(Unit.Value, message, metadata));
    }

    /// <summary>
    ///     Creates a failure result from a sequence of errors.
    /// </summary>
    public static Result Failure(IEnumerable<Error> errors, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new Result(Result<Unit, Error>.Failure(errors, message, metadata));
    }

    /// <summary>
    ///     Creates a failure result from a single error.
    /// </summary>
    public static Result Failure(Error error, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new Result(Result<Unit, Error>.Failure(error, message, metadata));
    }

    /// <summary>
    ///     Implicit conversion from <see cref="Error" /> to a failure result.
    /// </summary>
    public static implicit operator Result(Error error)
    {
        return Failure(error);
    }

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TNext, TError> Map<TNext>(Func<TValue, TNext> map)
    {
        if (map is null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        return _state == 1
            ? Result<TNext, TError>.Success(map(_value!), Message, Metadata)
            : Result<TNext, TError>.Failure(_errors!, Message, Metadata);
    }

    /// <summary>
    ///     Monadic bind chaining another result-producing function.
    /// </summary>
    /// <typeparam name="TNext">Next value type.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TNext, TError> Bind<TNext>(Func<TValue, Result<TNext, TError>> bind)
    {
        if (bind is null)
        {
            throw new ArgumentNullException(nameof(bind));
        }

        return _state == 1 ? bind(_value!) : Result<TNext, TError>.Failure(_errors!, Message, Metadata);
    }

    /// <summary>
    ///     Projects a failure into another value type preserving errors.
    /// </summary>
    /// <typeparam name="TNext">New value type.</typeparam>
    public Result<TNext, TError> AsFailure<TNext>()
    {
        return _state == 2
            ? Result<TNext, TError>.Failure(_errors!, Message, Metadata)
            : throw new InvalidOperationException("AsFailure can only be called on a failure result.");
    }

    /// <summary>
    ///     Creates a success result.
    /// </summary>
    public static Result<TValue, TError> Success(TValue value, string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new Result<TValue, TError>(value, message, metadata);
    }

    /// <summary>
    ///     Creates a failure result from a sequence of errors (optimized cloning path).
    /// </summary>
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
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new Result<TValue, TError>([error], true, message, metadata);
    }

    /// <summary>
    ///     Creates a failure result from a params array.
    /// </summary>
    public static Result<TValue, TError> Failure(string? message = null,
        IReadOnlyDictionary<string, object?>? metadata = null, params TError[] errors)
    {
        return new Result<TValue, TError>(errors ?? throw new ArgumentNullException(nameof(errors)), false, message,
            metadata);
    }

    /// <summary>
    ///     Implicit conversion from error to failure result.
    /// </summary>
    public static implicit operator Result<TValue, TError>(TError error)
    {
        return Failure(error);
    }

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