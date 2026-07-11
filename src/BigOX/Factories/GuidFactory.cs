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
    ///     The following code demonstrates how to use the <see cref="NewSequentialGuid()" /> method to generate a new sequential
    ///     <see cref="Guid" /> value.
    ///     <code><![CDATA[
    /// var newGuid = GuidFactory.NewSequentialGuid();
    /// Console.WriteLine(newGuid);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // ReSharper disable once MemberCanBePrivate.Global
    public static Guid NewSequentialGuid() => Guid.CreateVersion7();

    /// <summary>
    ///     Generates a new sequential <see cref="Guid" /> whose embedded timestamp is <paramref name="timestamp" />.
    /// </summary>
    /// <param name="timestamp">
    ///     The timestamp to embed in the generated value. Must be at or after the Unix epoch
    ///     (<c>1970-01-01T00:00:00Z</c>).
    /// </param>
    /// <returns>A new version-7 <see cref="Guid" /> whose ordering reflects <paramref name="timestamp" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="timestamp" /> is earlier than the Unix epoch.
    /// </exception>
    /// <remarks>
    ///     Two values generated with the same <paramref name="timestamp" /> still differ because the remaining bits are
    ///     random; only the time-ordered prefix is determined by <paramref name="timestamp" />.
    /// </remarks>
    /// <example>
    ///     <code><![CDATA[
    /// var guid = GuidFactory.NewSequentialGuid(DateTimeOffset.UtcNow);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid NewSequentialGuid(DateTimeOffset timestamp) => Guid.CreateVersion7(timestamp);

    /// <summary>
    ///     Generates a lazily-evaluated sequence of new sequential <see cref="Guid" /> values.
    /// </summary>
    /// <param name="count">The number of sequential <see cref="Guid" /> values to generate.</param>
    /// <returns>A lazily-evaluated sequence of new sequential <see cref="Guid" /> values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="count" /> is less than 1 (raised when the sequence is first enumerated).
    /// </exception>
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
            yield return NewSequentialGuid();
        }
    }

    /// <summary>
    ///     Fills <paramref name="destination" /> with newly generated sequential <see cref="Guid" /> values.
    /// </summary>
    /// <param name="destination">
    ///     The span to populate; every slot is overwritten with a new version-7 <see cref="Guid" />. An empty span is a
    ///     no-op.
    /// </param>
    /// <remarks>
    ///     This overload allocates nothing and is the allocation-free counterpart to
    ///     <see cref="NewSequentialGuids(int)" />; the destination is filled in ascending index order.
    /// </remarks>
    /// <example>
    ///     <code><![CDATA[
    /// Span<Guid> ids = stackalloc Guid[4];
    /// GuidFactory.NewSequentialGuids(ids);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NewSequentialGuids(Span<Guid> destination)
    {
        for (var i = 0; i < destination.Length; i++)
        {
            destination[i] = Guid.CreateVersion7();
        }
    }
}