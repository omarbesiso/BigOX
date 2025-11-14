using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a value-type property is **not** equal to its default value
    ///     (<c>default(T)</c>).
    /// </summary>
    /// <typeparam name="T">Any struct-constrained type.</typeparam>
    /// <param name="value">The property value to validate.</param>
    /// <param name="propertyName">
    ///     Property name, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> equals
    ///     <c>default(T)</c>.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it is not
    ///     <c>default(T)</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> equals its type’s default.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private Guid _tenantId;
    /// 
    /// public Guid TenantId
    /// {
    ///     get => _tenantId;
    ///     set => _tenantId = PropertyGuard.NotDefault(value);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotDefault<T>(
        T value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
        where T : struct
    {
        return Guard.NotDefault(value, propertyName, exceptionMessage);
    }
}