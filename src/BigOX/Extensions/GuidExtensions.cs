using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="Guid" /> objects.
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    ///     Provides extension methods for the <see cref="Guid" /> struct.
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
        /// bool isEmpty = emptyGuid.IsEmpty();
        /// 
        /// // isEmpty is true.
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     This extension method is useful for quickly checking if a given <see cref="Guid" /> value is equal to
        ///     <see cref="Guid.Empty" />. The method is marked with the <see cref="MethodImplAttribute" /> and the
        ///     <see cref="MethodImplOptions.AggressiveInlining" /> option, allowing the JIT compiler to inline the method's body
        ///     at the call site for improved performance.
        /// </remarks>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            return value == Guid.Empty;
        }

        /// <summary>
        ///     Determines if the specified <see cref="Guid" /> value is not empty.
        /// </summary>
        /// <returns><c>true</c> if the <see cref="Guid" /> value is not empty; otherwise, <c>false</c>.</returns>
        /// <example>
        ///     <code><![CDATA[
        /// Guid nonEmptyGuid = Guid.NewGuid();
        /// bool isNotEmpty = nonEmptyGuid.IsNotEmpty();
        /// 
        /// // isNotEmpty is true.
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     This extension method is useful for quickly checking if a given <see cref="Guid" /> value is not equal to
        ///     <see cref="Guid.Empty" />. The method is marked with the <see cref="MethodImplAttribute" /> and the
        ///     <see cref="MethodImplOptions.AggressiveInlining" /> option, allowing the JIT compiler to inline the method's body
        ///     at the call site for improved performance.
        /// </remarks>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotEmpty()
        {
            return value != Guid.Empty;
        }
    }
}