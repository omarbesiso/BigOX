using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using BigOX.Validation;

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="string" /> objects.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Extension methods for the <see cref="string" /> type.
    /// </summary>
    /// <param name="value">The value string from which the digits are to be extended.</param>
    extension(string? value)
    {
        /// <summary>
        ///     Indicates whether the string is a valid GUID.
        /// </summary>
        public bool IsGuid => !string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out _);

        /// <summary>
        ///     Indicates whether the string is a valid email address.
        /// </summary>
        public bool IsValidEmail => !string.IsNullOrWhiteSpace(value) && MailAddress.TryCreate(value, out _);

        /// <summary>
        ///     Indicates whether the string is a valid website URL.
        /// </summary>
        public bool IsValidWebsiteUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return false;
                }

                return Uri.TryCreate(value, UriKind.Absolute, out var uriResult)
                       && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            }
        }

        /// <summary>
        ///     Indicates whether the string consists only of white-space characters.
        /// </summary>
        public bool IsWhiteSpace
        {
            get
            {
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }

                foreach (var ch in value.AsSpan())
                {
                    if (!char.IsWhiteSpace(ch))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        ///     Extracts all the digits from the given value string.
        /// </summary>
        /// <returns>
        ///     Returns a string containing only the digits from the value. If the value is <c>null</c>, empty, or consists
        ///     only of white-space characters, an empty string is returned.
        /// </returns>
        /// <example>
        ///     <code><![CDATA[
        /// string text = "Hello123World";
        /// string result = text.ExtractDigits();
        /// Console.WriteLine(result); // Outputs: "123"
        /// ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ExtractDigits()
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var src = value.AsSpan();

            // First pass: count digits
            var len = 0;
            foreach (var ch in src)
            {
                if (char.IsDigit(ch))
                {
                    len++;
                }
            }

            if (len == 0)
            {
                return string.Empty;
            }

            // Second pass: write digits only
            return string.Create(len, src, static (span, source) =>
            {
                var i = 0;
                foreach (var ch in source)
                {
                    if (char.IsDigit(ch))
                    {
                        span[i++] = ch;
                    }
                }
            });
        }

        /// <summary>
        ///     Reduces the length of the given <see cref="string" /> to the specified maximum length using the underlying
        ///     <see cref="StringBuilder" /> method.
        /// </summary>
        /// <param name="maxLength">The maximum length of the resulting <see cref="string" />.</param>
        /// <returns>A <see cref="string" /> with a length reduced to the specified maximum length.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="maxLength" /> is less than 0.</exception>
        /// <example>
        ///     <code><![CDATA[
        /// string originalString = "Hello, World!";
        /// string result = originalString.ReduceToLength(5);
        /// Console.WriteLine(result); // Output: Hello
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     The <see cref="ReduceToLength" /> method reduces the length of the string value to the specified
        ///     <paramref name="maxLength" />. If the string value is already shorter than or equal to the
        ///     <paramref name="maxLength" />, the method returns the original string value without any changes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReduceToLength(int maxLength)
        {
            Guard.NotNull(value);
            Guard.Minimum(maxLength, 0);

            return value.Length <= maxLength ? value : value[..maxLength];
        }

        /// <summary>
        ///     Ensures the given <see cref="string" /> starts with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to check and add if needed.</param>
        /// <param name="stringComparison">
        ///     An optional <see cref="StringComparison" /> enumeration value that determines how the
        ///     comparison is performed. The default is <see cref="StringComparison.InvariantCulture" />.
        /// </param>
        /// <returns>A <see cref="string" /> that starts with the specified prefix.</returns>
        /// <example>
        ///     <code><![CDATA[
        /// string originalString = "World";
        /// string result = originalString.EnsureStartsWith("Hello, ");
        /// Console.WriteLine(result); // Output: Hello, World
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     The <see cref="EnsureStartsWith" /> method checks if the string value starts with the specified
        ///     <paramref name="prefix" />. If it doesn't, the method adds the <paramref name="prefix" /> to the beginning of the
        ///     string value. If the string value already starts with the <paramref name="prefix" />, the
        ///     method returns the original string value without any changes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string EnsureStartsWith(string prefix,
            StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            Guard.NotNull(value);
            Guard.NotNull(prefix);

            return value.StartsWith(prefix, stringComparison) ? value : string.Concat(prefix, value);
        }

        /// <summary>
        ///     Ensures the given <see cref="string" /> ends with the specified suffix.
        /// </summary>
        /// <param name="suffix">The suffix to check and add if needed.</param>
        /// <param name="stringComparison">
        ///     An optional <see cref="StringComparison" /> enumeration value that determines how the
        ///     comparison is performed. The default is <see cref="StringComparison.InvariantCulture" />.
        /// </param>
        /// <returns>A <see cref="string" /> that ends with the specified suffix.</returns>
        /// <example>
        ///     <code><![CDATA[
        /// string originalString = "Hello";
        /// string result = originalString.EnsureEndsWith(", World");
        /// Console.WriteLine(result); // Output: Hello, World
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     The <see cref="EnsureEndsWith" /> method checks if the string value ends with the specified
        ///     <paramref name="suffix" />. If it doesn't, the method appends the <paramref name="suffix" /> to the
        ///     string value. If the string value already ends with the <paramref name="suffix" />, the
        ///     method returns the original string value without any changes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string EnsureEndsWith(string suffix,
            StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            Guard.NotNull(value);
            Guard.NotNull(suffix);

            return value.EndsWith(suffix, stringComparison) ? value : string.Concat(value, suffix);
        }

        /// <summary>
        ///     Removes all whitespace characters from the specified string.
        /// </summary>
        /// <returns>A new string with all whitespace characters removed from the value string.</returns>
        /// <remarks>
        ///     This method iterates through each character in the value string and appends only non-whitespace characters to a new
        ///     string.
        ///     It's useful in scenarios where whitespace is not required or should be ignored, such as processing user value or
        ///     preparing strings for comparison.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        ///     string value = "  Hello  World  ";
        ///     string result = RemoveWhitespace(value);
        ///     // result is "HelloWorld"
        ///     ]]></code>
        /// </example>
        public string? RemoveWhitespace()
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var src = value.AsSpan();

            // Fast check: count whitespace
            var whitespaceCount = 0;
            foreach (var c in src)
            {
                if (char.IsWhiteSpace(c))
                {
                    whitespaceCount++;
                }
            }

            if (whitespaceCount == 0)
            {
                return value;
            }

            var resultLength = src.Length - whitespaceCount;
            return string.Create(resultLength, src, static (span, source) =>
            {
                var i = 0;
                foreach (var c in source)
                {
                    if (!char.IsWhiteSpace(c))
                    {
                        span[i++] = c;
                    }
                }
            });
        }

        /// <summary>
        ///     Converts the specified <see cref="string" /> into a <see cref="StringBuilder" />.
        /// </summary>
        /// <returns>A <see cref="StringBuilder" /> containing the contents of the string value.</returns>
        /// <example>
        ///     <code><![CDATA[
        /// string originalString = "Hello, world!";
        /// StringBuilder stringBuilder = originalString.ToStringBuilder();
        /// stringBuilder.Append(" This is a StringBuilder.");
        /// Console.WriteLine(stringBuilder); // Output: Hello, world! This is a StringBuilder.
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     The <see cref="ToStringBuilder" /> method creates a new <see cref="StringBuilder" /> instance with the contents of
        ///     the specified <see cref="string" />. If the input string value is <c>null</c>, the resulting
        ///     <see cref="StringBuilder" /> will be empty.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder ToStringBuilder()
        {
            return new StringBuilder(value);
        }

        /// <summary>
        ///     Appends a specified character to the given <see cref="string" /> until it reaches the specified target length.
        /// </summary>
        /// <param name="targetLength">The target length of the resulting <see cref="string" />.</param>
        /// <param name="charToAppend">The character to append to the string value.</param>
        /// <returns>A <see cref="string" /> with the specified character appended to reach the target length.</returns>
        /// <example>
        ///     <code><![CDATA[
        /// string originalString = "Hello";
        /// string result = originalString.AppendCharToLength(10, '-');
        /// Console.WriteLine(result); // Output: Hello-----
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     The <see cref="AppendCharToLength" /> method appends the specified <paramref name="charToAppend" /> to the
        ///     string value until the resulting <see cref="string" /> reaches the specified
        ///     <paramref name="targetLength" />. If the string value is already longer than or equal to the
        ///     <paramref name="targetLength" />, the method returns the original string value without any changes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string AppendCharToLength(int targetLength,
            char charToAppend)
        {
            var source = value ?? string.Empty;
            if (targetLength <= source.Length)
            {
                return source;
            }

            var needed = targetLength - source.Length;
            return string.Create(targetLength, (source, charToAppend, needed), static (span, state) =>
            {
                var (src, ch, pad) = state;
                src.AsSpan().CopyTo(span);
                span.Slice(src.Length, pad).Fill(ch);
            });
        }

        /// <summary>
        ///     Determines whether the specified <see cref="string" /> can be parsed as a valid <see cref="DateTime" />.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the string value can be parsed as a valid <see cref="DateTime" />; otherwise,
        ///     <c>false</c>.
        /// </returns>
        /// <example>
        ///     <code><![CDATA[
        /// string validDate = "2022-01-01";
        /// string invalidDate = "invalid-date";
        /// bool validResult = validDate.IsDateTime(); // true
        /// bool invalidResult = invalidDate.IsDateTime(); // false
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     The <see cref="IsDateTime" /> method checks if the provided string value is not <c>null</c> or empty
        ///     and can be successfully parsed as a <see cref="DateTime" /> using the
        ///     <see cref="DateTime.TryParse(string, out DateTime)" /> method.
        ///     Note that this method returns <c>false</c> if the value is <c>null</c> or an empty string.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDateTime()
        {
            return !string.IsNullOrEmpty(value) && DateTime.TryParse(value, out _);
        }

        /// <summary>
        ///     Limits the length of the text to the specified maximum length.
        ///     If the source text is shorter than or equal to the maximum length, the entire source text is returned.
        ///     Otherwise, a substring of the source text is returned with a length equal to the specified maximum length.
        /// </summary>
        /// <param name="maxLength">The maximum length for the returned string.</param>
        /// <returns>
        ///     A string that represents the limited length of the source text.
        ///     If string value is null, the method returns null.
        ///     If string value is shorter than or equal to <paramref name="maxLength" />, the method returns
        ///     string value.
        ///     Otherwise, the method returns a substring of string value with length equal to
        ///     <paramref name="maxLength" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="maxLength" /> is less than 0.
        /// </exception>
        public string? LimitLength(int maxLength)
        {
            Guard.Minimum(maxLength, 0);

            if (value is null || value.Length <= maxLength)
            {
                return value;
            }

            return value[..maxLength];
        }
    }
}