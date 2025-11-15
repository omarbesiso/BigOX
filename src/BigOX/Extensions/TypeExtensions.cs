using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq; // Added for First LINQ usage
using BigOX.Validation;

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="Type" /> objects.
/// </summary>
public static class TypeExtensions
{
    private static readonly MethodInfo DefaultValueGenericMethod = typeof(TypeExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m.Name == nameof(DefaultValue)
                    && m.IsGenericMethodDefinition
                    && m.GetGenericArguments().Length == 1
                    && m.GetParameters().Length == 0);
    // Replaced ambiguous GetMethod call with filtered selection of generic method definition.
    private static readonly ConcurrentDictionary<Type, object?> DefaultValues = new();

    private static readonly Dictionary<Type, string> TypeAlias = new()
    {
        { typeof(bool), "bool" },
        { typeof(bool?), "bool?" },
        { typeof(byte), "byte" },
        { typeof(byte?), "byte?" },
        { typeof(char), "char" },
        { typeof(char?), "char?" },
        { typeof(decimal), "decimal" },
        { typeof(decimal?), "decimal?" },
        { typeof(double), "double" },
        { typeof(double?), "double?" },
        { typeof(float), "float" },
        { typeof(float?), "float?" },
        { typeof(int), "int" },
        { typeof(int?), "int?" },
        { typeof(long), "long" },
        { typeof(long?), "long?" },
        { typeof(object), "object" },
        { typeof(sbyte), "sbyte" },
        { typeof(sbyte?), "sbyte?" },
        { typeof(short), "short" },
        { typeof(short?), "short?" },
        { typeof(string), "string" },
        { typeof(uint), "uint" },
        { typeof(uint?), "uint?" },
        { typeof(ulong), "ulong" },
        { typeof(ulong?), "ulong?" },
        { typeof(Guid), "Guid" },
        { typeof(Guid?), "Guid?" },
        { typeof(DateTime), "DateTime" },
        { typeof(DateTime?), "DateTime?" },
        { typeof(DateOnly), "DateOnly" },
        { typeof(DateOnly?), "DateOnly?" },
        { typeof(TimeOnly), "TimeOnly" },
        { typeof(TimeOnly?), "TimeOnly?" },
        { typeof(void), "void" }
    };

    /// <summary>
    ///     Returns the default value for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to get the default value for.</typeparam>
    /// <returns>The default value for the specified type.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? DefaultValue<T>()
    {
        return default;
    }

    /// <summary>
    ///     Determines whether the type of the specified source object is a nullable type.
    /// </summary>
    /// <typeparam name="T">The type of the source object.</typeparam>
    /// <param name="source">The source object to check for nullability.</param>
    /// <returns><c>true</c> if the type of the source object is a nullable type; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <typeparamref name="T" /> is a value type and
    ///     <paramref name="source" /> is <c>null</c>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOfNullableType<T>(this T source)
    {
        if (source == null && typeof(T).IsValueType)
        {
            throw new ArgumentNullException(nameof(source), "Source cannot be null when T is a value type.");
        }

        return typeof(T).IsNullable();
    }

    /// <summary>
    ///     Determines whether the specified type <typeparamref name="T" /> is a nullable type.
    /// </summary>
    /// <typeparam name="T">The type to check for nullability.</typeparam>
    /// <returns><c>true</c> if the specified type <typeparamref name="T" /> is a nullable type; otherwise, <c>false</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOfNullableType<T>()
    {
        return typeof(T).IsNullable();
    }

    /// <summary>
    ///     Gets the <see cref="TypeCode" /> of the specified type.
    /// </summary>
    /// <param name="type">The type to get the type code for.</param>
    /// <param name="includeNullableTypes">
    ///     Indicates whether nullable types should be treated as their underlying types. If this is <c>true</c>,
    ///     the type code for a nullable type is the type code of its underlying type. Otherwise, it is
    ///     <see cref="TypeCode.Object" />.
    /// </param>
    /// <returns>The <see cref="TypeCode" /> of the specified type.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TypeCode GetTypeCode(Type type, bool includeNullableTypes)
    {
        Guard.NotNull(type);
        var typeToCheck = includeNullableTypes && type.IsNullable() ? Nullable.GetUnderlyingType(type)! : type;
        return Type.GetTypeCode(typeToCheck);
    }

    /// <summary>
    ///     Provides extension methods for the <see cref="Type" /> class.
    /// </summary>
    /// <param name="type">The type to extend.</param>
    extension(Type type)
    {
        /// <summary>
        ///     Determines whether the type is a numeric type, optionally including nullable numeric types.
        /// </summary>
        /// <param name="includeNullableTypes">
        ///     Indicates whether nullable numeric types should be considered numeric. The default is <c>true</c>.
        /// </param>
        /// <returns><c>true</c> if the type is numeric; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the type is <c>null</c>.</exception>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNumeric(bool includeNullableTypes = true)
        {
            Guard.NotNull(type);
            if (type.IsArray || type.IsEnum)
            {
                return false;
            }

            var typeCode = GetTypeCode(type, includeNullableTypes);
            return typeCode switch
            {
                TypeCode.Byte or TypeCode.Decimal or TypeCode.Double or TypeCode.Int16 or TypeCode.Int32
                    or TypeCode.Int64
                    or TypeCode.SByte or TypeCode.Single or TypeCode.UInt16 or TypeCode.UInt32
                    or TypeCode.UInt64 => true,
                _ => false
            };
        }

        /// <summary>
        ///     Determines whether the specified <see cref="Type" /> represents an open generic type.
        /// </summary>
        /// <returns><c>true</c> if the specified type is an open generic type; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the type is <c>null</c>.</exception>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOpenGeneric()
        {
            Guard.NotNull(type);
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        /// <summary>
        ///     Determines whether the specified <see cref="Type" /> has a specified attribute type.
        /// </summary>
        /// <param name="attributeType">The attribute type to look for.</param>
        /// <returns><c>true</c> if the specified type has the attribute type; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the type or <paramref name="attributeType" /> is <c>null</c>.
        /// </exception>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAttribute(Type attributeType)
        {
            Guard.NotNull(type);
            Guard.NotNull(attributeType);
            return Attribute.IsDefined(type, attributeType, true);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="Type" /> has an attribute of type <typeparamref name="T" /> that
        ///     satisfies the specified predicate.
        /// </summary>
        /// <typeparam name="T">The attribute type to look for.</typeparam>
        /// <param name="predicate">A function to test each attribute for a condition.</param>
        /// <returns>
        ///     <c>true</c> if the specified type has an attribute of type <typeparamref name="T" /> that satisfies the
        ///     predicate; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the type or <paramref name="predicate" /> is <c>null</c>.
        /// </exception>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAttribute<T>(Func<T, bool> predicate) where T : Attribute
        {
            Guard.NotNull(type);
            Guard.NotNull(predicate);
            return type.GetTypeInfo().GetCustomAttributes<T>(true).Any(predicate);
        }

        /// <summary>
        ///     Returns the default value for the type using cached reflection to avoid repeated generic method construction.
        /// </summary>
        /// <returns>The default value for the specified type.</returns>
        public object? DefaultValue()
        {
            Guard.NotNull(type);

            return DefaultValues.GetOrAdd(type, static t =>
            {
                try
                {
                    if (t == typeof(void))
                    {
                        return null;
                    }

                    return DefaultValueGenericMethod.MakeGenericMethod(t).Invoke(null, null);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error obtaining default value for type {t.FullName}", ex);
                }
            });
        }

        /// <summary>
        ///     Asynchronously returns the default value for the type. The task completes synchronously to avoid Task.Run overhead.
        /// </summary>
        /// <returns>A task that produces the default value for the specified type.</returns>
        public ValueTask<object?> DefaultValueAsync()
        {
            Guard.NotNull(type);
            return ValueTask.FromResult(DefaultValue(type));
        }

        /// <summary>
        ///     Gets the name or an alias for the specified type.
        /// </summary>
        /// <returns>The name or alias for the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the type is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetTypeAsString()
        {
            Guard.NotNull(type);
            return TypeAlias.TryGetValue(type, out var result) ? result : type.Name;
        }

        /// <summary>
        ///     Determines whether the specified type is nullable.
        /// </summary>
        /// <returns><c>true</c> if the specified type is nullable; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the type is <c>null</c>.</exception>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNullable()
        {
            Guard.NotNull(type);
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}