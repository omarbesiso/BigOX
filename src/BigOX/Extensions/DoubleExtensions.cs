using System.Runtime.CompilerServices;

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="double" /> objects.
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    ///     Provides extension methods for nullable double values.
    /// </summary>
    /// <param name="value">The nullable double value to convert.</param>
    extension(double? value)
    {
        /// <summary>
        ///     Converts a nullable double value to a nullable decimal.
        /// </summary>
        /// <returns>
        ///     A nullable decimal representing the converted value if source has a value; otherwise,
        ///     <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     This method converts the given nullable double value to a nullable decimal. Since a decimal has higher precision
        ///     compared to a double,
        ///     this conversion is safe and does not result in a loss of precision.
        /// </remarks>
        /// <example>
        ///     The following example shows how to use the <see cref="ToDecimal" /> method to convert a nullable double value to a
        ///     nullable decimal.
        ///     <code>
        /// double? value = 1234.56;
        /// decimal? decimalValue = value.ToDecimal();
        /// Console.WriteLine("Decimal value: {0}", decimalValue);
        /// 
        /// double? nullValue = null;
        /// decimal? nullDecimalValue = nullValue.ToDecimal();
        /// Console.WriteLine("Decimal value: {0}", nullDecimalValue);
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? ToDecimal()
        {
            return value.HasValue ? (decimal)value.Value : null;
        }
    }
}