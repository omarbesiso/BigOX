using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using BigOX.Factories;

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="decimal" /> objects.
/// </summary>
public static class DecimalExtensions
{
    // Cache number word maps to avoid per-call allocations
    private static readonly string[] UnitsMap =
    {
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven",
        "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"
    };

    private static readonly string[] TensMap =
    {
        "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
    };

    /// <summary>
    ///     Converts a nullable decimal value to a nullable double.
    /// </summary>
    /// <param name="value">The nullable decimal value to convert.</param>
    /// <returns>
    ///     A nullable double representing the converted value if <paramref name="value" /> has a value; otherwise, <c>null</c>
    ///     .
    /// </returns>
    /// <remarks>
    ///     This method converts the given nullable decimal value to a nullable double, which might result in a loss of
    ///     precision
    ///     since a double has less precision compared to a decimal. Use this method when the higher range of a double
    ///     is required over the precision of a decimal.
    /// </remarks>
    /// <example>
    ///     The following example shows how to use the <see cref="ToDouble" /> method to convert a nullable decimal value to a
    ///     nullable double.
    ///     <code>
    /// decimal? value = 1234.56m;
    /// double? doubleValue = value.ToDouble();
    /// Console.WriteLine("Double value: {0}", doubleValue);
    /// 
    /// decimal? nullValue = null;
    /// double? nullDoubleValue = nullValue.ToDouble();
    /// Console.WriteLine("Double value: {0}", nullDoubleValue);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double? ToDouble(this decimal? value)
    {
        return value.HasValue ? (double?)value.Value : null;
    }

    private static string NumberToWords(long number)
    {
        if (number == 0)
        {
            return "zero";
        }

        if (number < 0)
        {
            return "minus " + NumberToWords(Math.Abs(number));
        }

        var sb = new StringBuilder();

        static void AppendChunk(StringBuilder builder, long chunk, string label)
        {
            if (chunk <= 0)
            {
                return;
            }

            builder.Append(NumberToWords(chunk));
            builder.Append(' ');
            builder.Append(label);
            builder.Append(' ');
        }

        AppendChunk(sb, number / 1_000_000, "million");
        number %= 1_000_000;

        AppendChunk(sb, number / 1_000, "thousand");
        number %= 1_000;

        AppendChunk(sb, number / 100, "hundred");
        number %= 100;

        if (number > 0)
        {
            if (sb.Length > 0)
            {
                sb.Append("and ");
            }

            if (number < 20)
            {
                sb.Append(UnitsMap[number]);
            }
            else
            {
                sb.Append(TensMap[number / 10]);
                if (number % 10 > 0)
                {
                    sb.Append('-');
                    sb.Append(UnitsMap[number % 10]);
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    ///     Provides extension methods for the <see cref="decimal" /> type.
    /// </summary>
    /// <param name="value">The decimal value to convert.</param>
    extension(decimal value)
    {
        /// <summary>
        ///     Converts a decimal value to a currency string using a specified culture.
        /// </summary>
        /// <param name="cultureName">The name of the culture to use for the currency string formatting. Default is "en-US".</param>
        /// <returns>A string representing the given decimal value as a currency in the specified culture.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="cultureName" /> parameter is <c>null</c> or
        ///     empty.
        /// </exception>
        /// <exception cref="CultureNotFoundException">
        ///     Thrown when the culture specified by the <paramref name="cultureName" />
        ///     parameter is not found.
        /// </exception>
        /// <example>
        ///     <code>
        /// decimal value = 1234.56m;
        /// string currencyString = value.ToCurrencyString("en-US");
        /// // Output: "$1,234.56"
        /// 
        /// string currencyStringFr = value.ToCurrencyString("fr-FR");
        /// // Output: "1 234,56 €"
        /// </code>
        /// </example>
        public string ToCurrencyString(string cultureName = "en-US")
        {
            var culture = CultureInfoFactory.Create(cultureName);
            return value.ToString("C", culture);
        }

        /// <summary>
        ///     Converts a decimal value to a percentage string.
        /// </summary>
        /// <param name="decimalPlaces">The number of decimal places to include in the percentage string. Default is 2.</param>
        /// <param name="cultureName">The name of the culture to use for the percentage string formatting. Default is "en-US".</param>
        /// <returns>A string representing the given decimal value as a percentage in the specified culture.</returns>
        public string ToPercentageString(int decimalPlaces = 2, string cultureName = "en-US")
        {
            var culture = CultureInfoFactory.Create(cultureName);
            var format = "P" + decimalPlaces;
            return value.ToString(format, culture);
        }

        /// <summary>
        ///     Rounds a decimal value to a specified number of decimal places.
        /// </summary>
        /// <param name="decimalPlaces">The number of decimal places to round to.</param>
        /// <returns>The rounded decimal value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal RoundTo(int decimalPlaces)
        {
            return Math.Round(value, decimalPlaces);
        }

        /// <summary>
        ///     Converts a decimal value to its word representation.
        /// </summary>
        /// <returns>A string representing the given decimal value in words.</returns>
        /// <example>
        ///     <code>
        /// decimal value = 1234.56m;
        /// string words = value.ToWords();
        /// // Output: "one thousand two hundred thirty-four and fifty-six cents"
        /// </code>
        /// </example>
        public string ToWords()
        {
            if (value == 0)
            {
                return "zero";
            }

            var integerPart = (long)Math.Truncate(value);
            var fractionalPart = (long)Math.Round(Math.Abs((value - integerPart) * 100m));

            var words = new StringBuilder();
            words.Append(NumberToWords(integerPart));

            if (fractionalPart <= 0)
            {
                return words.ToString();
            }

            words.Append(" and ");
            words.Append(NumberToWords(fractionalPart));
            words.Append(" cents");

            return words.ToString();
        }

        /// <summary>
        ///     Checks if a decimal value is a whole number.
        /// </summary>
        /// <returns><c>true</c> if the decimal value is a whole number; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWholeNumber()
        {
            return value == Math.Truncate(value);
        }

        /// <summary>
        ///     Gets the absolute value of a decimal.
        /// </summary>
        /// <returns>The absolute value of the decimal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Abs()
        {
            return Math.Abs(value);
        }
    }
}