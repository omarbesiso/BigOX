using System.Globalization;
using BigOX.Validation;

namespace BigOX.Factories;

/// <summary>
///     Provides factory methods for creating <see cref="CultureInfo" /> instances.
/// </summary>
internal static class CultureInfoFactory
{
    /// <summary>
    ///     Creates a read-only, cached <see cref="CultureInfo" /> object for the specified culture name.
    ///     If the culture name is <c>null</c>, empty, or consists only of white-space characters,
    ///     an <see cref="ArgumentNullException" /> or <see cref="ArgumentException" /> is thrown.
    ///     If the specified culture is not found, a <see cref="CultureNotFoundException" /> is thrown.
    /// </summary>
    /// <param name="cultureName">
    ///     The name of the culture to create. It must be a valid culture name string
    ///     that is neither null nor empty.
    /// </param>
    /// <returns>
    ///     A read-only, cached <see cref="CultureInfo" /> object representing the specified culture.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="cultureName" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown if <paramref name="cultureName" /> is empty or contains only white-space characters.
    /// </exception>
    /// <exception cref="CultureNotFoundException">
    ///     Thrown if the specified culture is not found.
    /// </exception>
    /// <remarks>
    ///     This method is useful for creating a <see cref="CultureInfo" /> object from a culture name string.
    ///     It validates that the culture name is not null, empty, or white-space, and ensures that
    ///     the culture exists. Since <see cref="CultureInfo.GetCultureInfo(string)" /> returns a read-only
    ///     instance, you must clone it if you need to modify its properties.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var culture = CultureInfoFactory.Create("en-US");
    /// // The returned instance is read-only. If you need to modify it, call:
    /// culture = (CultureInfo)culture.Clone();
    /// culture.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
    /// </code>
    /// </example>
    public static CultureInfo Create(string cultureName)
    {
        Guard.NotNullOrWhiteSpace(cultureName);

        // Attempt to retrieve a cached, read-only CultureInfo.
        try
        {
            return CultureInfo.GetCultureInfo(cultureName);
        }
        catch (CultureNotFoundException ex)
        {
            // Rethrow with additional context
            throw new CultureNotFoundException(
                $"The culture specified by '{cultureName}' is not found.",
                ex);
        }
    }
}