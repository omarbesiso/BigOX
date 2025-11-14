using BigOX.Validation;

namespace BigOX.Extensions;

/// <summary>
///     Provides extension methods for arrays.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    ///     Provides extension methods for arrays of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to extend.</param>
    extension<T>(T[] array)
    {
        /// <summary>
        ///     Clears a range of elements in the array.
        /// </summary>
        /// <param name="index">The starting index of the range to clear.</param>
        /// <param name="length">The number of elements to clear.</param>
        /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index or length are out of range.</exception>
        /// <remarks>
        ///     This method sets the specified range of elements in the array to their default value.
        /// </remarks>
        public void ClearRange(int index, int length)
        {
            Guard.NotNull(array);
            Guard.Minimum(length, 0, exceptionMessage: "Length cannot be negative.");
            Guard.Minimum(index, 0, exceptionMessage: "Index cannot be negative.");
            Guard.Maximum(index + length, array.Length,
                exceptionMessage: "Index and length must refer to a location within the array.");

            array.AsSpan(index, length).Clear();
        }

        /// <summary>
        ///     Clears a range of elements in the array.
        /// </summary>
        /// <param name="range">The range of elements to clear.</param>
        /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the range is invalid.</exception>
        /// <remarks>
        ///     This method sets the specified range of elements in the array to their default value.
        /// </remarks>
        public void Clear(Range range)
        {
            Guard.NotNull(array);

            var (start, length) = range.GetOffsetAndLength(array.Length);
            array.AsSpan(start, length).Clear();
        }
    }
}