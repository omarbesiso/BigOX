using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that a value-type argument is **not** equal to its default value
    ///     (<c>default(T)</c>).
    ///     Typical examples include uninitialised <see cref="Guid" />s, <see cref="DateTime" />s,
    ///     integral types with sentinel <c>0</c>, or custom structs whose fields are all zero-initialised.
    /// </summary>
    /// <typeparam name="T">
    ///     Any unmanaged or managed struct (<c>where: struct</c>).
    /// </typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">
    ///     Argument name, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> equals
    ///     <c>default(T)</c>. If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it is not
    ///     <c>default(T)</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> equals its type’s default.
    /// </exception>
    /// <remarks>
    ///     Use this helper to catch uninitialised identifiers or timestamps early:
    ///     <code language="csharp"><![CDATA[
    ///     Guid orderId = Guid.Empty;          // BUG
    ///     orderId = Guard.NotDefault(orderId); // throws
    ///     ]]></code>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotDefault<T>(
        T value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
        where T : struct
    {
        if (!EqualityComparer<T>.Default.Equals(value, default))
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' must not be the default for {typeof(T).Name}."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}