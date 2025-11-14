using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a property value satisfies a user-supplied predicate.
    /// </summary>
    /// <typeparam name="T">The compile-time type of the property value.</typeparam>
    /// <param name="value">The property value to validate.</param>
    /// <param name="predicate">
    ///     A user-defined condition that <paramref name="value" /> must satisfy.
    /// </param>
    /// <param name="propertyName">
    ///     The name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> fails the predicate.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it satisfies <paramref name="predicate" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="predicate" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> does **not** satisfy <paramref name="predicate" />.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private int _stock;
    /// 
    /// public int Stock
    /// {
    ///     get => _stock;
    ///     set => _stock = PropertyGuard.Requires(value, n => n >= 0);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Requires<T>(
        T value,
        Predicate<T>? predicate,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.Requires(value, predicate, propertyName, exceptionMessage);
    }
}