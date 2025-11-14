using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Validation utilities specialised for property setters / initialisers.
/// </summary>
[DebuggerStepThrough]
[StackTraceHidden]
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures a property value is not <see langword="null" />.
    /// </summary>
    /// <typeparam name="T">Type of the property value.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="propertyName">
    ///     Name of the property being validated, captured automatically via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">Optional custom message.</param>
    /// <returns>The non-null property value.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static T NotNull<T>(
        [NotNull] [DoesNotReturnIf(true)] T? value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.NotNull(value, propertyName, exceptionMessage);
    }

    /// <summary>
    ///     Ensures that a property collection is not <see langword="null" />.
    /// </summary>
    /// <typeparam name="T">Element type of the collection.</typeparam>
    /// <param name="collection">The property value to validate.</param>
    /// <param name="propertyName">
    ///     Name of the property being validated, captured automatically via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message. When omitted, a default message is generated.
    /// </param>
    /// <returns>The non-null <paramref name="collection" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="collection" /> is <see langword="null" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> NotNull<T>(
        [NotNull] [DoesNotReturnIf(true)] IEnumerable<T>? collection,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.NotNull(collection, propertyName, exceptionMessage);
    }
}