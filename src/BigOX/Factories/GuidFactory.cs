using System.Runtime.CompilerServices;
using BigOX.Validation;

namespace BigOX.Factories;

/// <summary>
///     Contains utility methods for creating <see cref="Guid" /> instances.
/// </summary>
public static class GuidFactory
{
    /// <summary>
    ///     Generates a new sequential <see cref="Guid" />.
    /// </summary>
    /// <returns>A new sequential <see cref="Guid" /> value.</returns>
    /// <example>
    ///     The following code demonstrates how to use the <see cref="NewSequentialGuid" /> method to generate a new sequential
    ///     <see cref="Guid" /> value.
    ///     <code><![CDATA[
    /// var newGuid = GuidFactory.NewSequentialGuid();
    /// Console.WriteLine(newGuid);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // ReSharper disable once MemberCanBePrivate.Global
    public static Guid NewSequentialGuid()
    {
        return Guid.CreateVersion7();
    }

    /// <summary>
    ///     Generates a list of new sequential <see cref="Guid" />.
    /// </summary>
    /// <param name="count">The number of sequential <see cref="Guid" /> values to generate.</param>
    /// <returns>A list of new sequential <see cref="Guid" /> values.</returns>
    /// <example>
    ///     The following code demonstrates how to use the <see cref="NewSequentialGuids(int)" /> method to generate a list
    ///     of new sequential <see cref="Guid" /> values.
    ///     <code><![CDATA[
    /// var newGuids = GuidFactory.NewSequentialGuids(5);
    /// foreach (var guid in newGuids)
    /// {
    ///     Console.WriteLine(guid);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<Guid> NewSequentialGuids(int count)
    {
        Guard.Minimum(count, 1);
        for (var i = 0; i < count; i++)
        {
            yield return NewSequentialGuid(); // Or Guid.CreateVersion7() directly
        }
    }
}