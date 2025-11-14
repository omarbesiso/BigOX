using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a property string is either <see langword="null" /> or a valid e-mail address.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="propertyName">
    ///     Name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> is non-null and invalid.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or a valid e-mail address.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and not a valid e-mail address.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private string? _contactEmail;
    /// 
    /// public string? ContactEmail
    /// {
    ///     get => _contactEmail;
    ///     set => _contactEmail = PropertyGuard.EmailAddress(value);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? EmailAddress(
        string? value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.EmailAddress(value, propertyName, exceptionMessage);
    }
}