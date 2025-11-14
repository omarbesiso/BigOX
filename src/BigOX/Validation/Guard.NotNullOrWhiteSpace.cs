using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that the specified <paramref name="value" /> is neither
    ///     <see langword="null" />, an empty string (<c>""</c>), nor composed solely of white-space characters.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">
    ///     The name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the string is <see langword="null" />, empty, or white-space.
    ///     If omitted, default messages are generated for each failure case.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it contains at least one non-white-space character.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is empty or consists only of white-space characters.
    /// </exception>
    /// <remarks>
    ///     Use this helper to guarantee that string inputs contain meaningful content before proceeding with
    ///     business logic or persistence.
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// string firstName = GetFirstName();
    /// firstName = Guard.NotNullOrWhiteSpace(firstName);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NotNullOrWhiteSpace(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Null check (fast-fail).
        if (value is null)
        {
            var msg = string.IsNullOrWhiteSpace(exceptionMessage)
                ? $"The value of '{paramName}' cannot be null."
                : exceptionMessage;

            ThrowHelper.ThrowArgumentNull(paramName, msg);
        }

        // Empty or white-space check.
        // ReSharper disable once InvertIf
        if (string.IsNullOrWhiteSpace(value))
        {
            var msg = string.IsNullOrWhiteSpace(exceptionMessage)
                ? $"The value of '{paramName}' cannot be empty or whitespace."
                : exceptionMessage;

            ThrowHelper.ThrowArgument(paramName, msg);
        }

        return value;
    }
}