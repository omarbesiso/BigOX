namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension members for working with <see cref="Guid" /> objects.
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    ///     Provides extension members for the <see cref="Guid" /> struct.
    /// </summary>
    /// <param name="value">The <see cref="Guid" /> value to check for emptiness.</param>
    extension(Guid value)
    {
        /// <summary>
        ///     Determines if the specified <see cref="Guid" /> value is empty.
        /// </summary>
        /// <returns><c>true</c> if the <see cref="Guid" /> value is empty; otherwise, <c>false</c>.</returns>
        /// <example>
        ///     <code><![CDATA[
        /// Guid emptyGuid = Guid.Empty;
        /// bool isEmpty = emptyGuid.IsEmpty;
        /// 
        /// // isEmpty is true.
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     This extension property is useful for quickly checking if a given <see cref="Guid" /> value is equal to
        ///     <see cref="Guid.Empty" />.
        /// </remarks>
        public bool IsEmpty => value == Guid.Empty;

        /// <summary>
        ///     Determines if the specified <see cref="Guid" /> value is not empty.
        /// </summary>
        /// <returns><c>true</c> if the <see cref="Guid" /> value is not empty; otherwise, <c>false</c>.</returns>
        /// <example>
        ///     <code><![CDATA[
        /// Guid nonEmptyGuid = Guid.NewGuid();
        /// bool isNotEmpty = nonEmptyGuid.IsNotEmpty;
        /// 
        /// // isNotEmpty is true.
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     This extension property is useful for quickly checking if a given <see cref="Guid" /> value is not equal to
        ///     <see cref="Guid.Empty" />.
        /// </remarks>
        public bool IsNotEmpty => value != Guid.Empty;
    }
}