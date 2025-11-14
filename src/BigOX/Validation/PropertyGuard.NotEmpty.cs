using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that the specified property GUID is not <see cref="Guid.Empty" />.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="propertyName">
    ///     The name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the GUID is empty.
    /// </param>
    /// <returns>The original, non-empty <paramref name="value" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> equals <see cref="Guid.Empty" />.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private Guid _tenantId;
    /// 
    /// public Guid TenantId
    /// {
    ///     get => _tenantId;
    ///     set => _tenantId = PropertyGuard.NotEmpty(value);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid NotEmpty(
        Guid value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.NotEmpty(value, propertyName, exceptionMessage);
    }
}