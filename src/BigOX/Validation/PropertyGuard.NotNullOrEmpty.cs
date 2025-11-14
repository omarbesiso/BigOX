using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a property string is neither <see langword="null" /> nor empty.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="propertyName">
    ///     The name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the string is <see langword="null" /> or empty.
    /// </param>
    /// <returns>The original, non-<see langword="null" />, non-empty <paramref name="value" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is empty.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private string _title = string.Empty;
    /// 
    /// public string Title
    /// {
    ///     get => _title;
    ///     set => _title = PropertyGuard.NotNullOrEmpty(value);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NotNullOrEmpty(
        [NotNull] string? value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.NotNullOrEmpty(value, propertyName, exceptionMessage);
    }

    /// <summary>
    ///     Ensures that a property collection is not <see langword="null" /> and contains at least one element.
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
    /// <returns>The non-null, non-empty <paramref name="collection" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="collection" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="collection" /> is empty.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> NotNullOrEmpty<T>(
        [NotNull] [DoesNotReturnIf(true)] IEnumerable<T>? collection,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.NotNullOrEmpty(collection, propertyName, exceptionMessage);
    }

    /// <summary>
    ///     Ensures that a property value of type <see cref="Nullable{Guid}" /> is neither
    ///     <see langword="null" /> nor <see cref="Guid.Empty" />.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="propertyName">
    ///     Name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the GUID is <see langword="null" /> or empty.
    /// </param>
    /// <returns>The original, non-null, non-empty <see cref="Guid" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> equals <see cref="Guid.Empty" />.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private Guid? _orderId;
    /// 
    /// public Guid? OrderId
    /// {
    ///     get => _orderId;
    ///     set => _orderId = PropertyGuard.NotNullOrEmpty(value);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid NotNullOrEmpty(
        Guid? value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.NotNullOrEmpty(value, propertyName, exceptionMessage);
    }
}