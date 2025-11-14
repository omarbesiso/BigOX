using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BigOX.Extensions;

/// <summary>
///     Provides extension methods for enumerations, including methods to convert enumerations to dictionaries
///     and retrieve descriptions or display names of enumeration values.
/// </summary>
public static class EnumExtensions
{
    // Cache mapping: Enum Type -> (Enum Name -> Description)
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, string>> NameToDescriptionCache =
        new();

    // Cache mapping: Enum Type -> (Description -> Enum Name) for ToDictionary usage
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, string>> DescriptionToNameCache =
        new();

    // Cache mapping: Enum Type -> (Enum Name -> Display Name)
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, string>> NameToDisplayCache = new();

    /// <summary>
    ///     Creates a dictionary of the names and values of an enumeration with optional descriptions.
    /// </summary>
    /// <typeparam name="T">The enumeration type to convert.</typeparam>
    /// <returns>
    ///     A dictionary with keys representing the enumeration descriptions (or names if no description is available)
    ///     and values representing the enumeration values as strings.
    /// </returns>
    public static Dictionary<string, string> ToDictionary<T>() where T : Enum
    {
        var enumType = typeof(T);

        // Build description -> name map once per enum type (validating duplicates)
        var descriptionToName = DescriptionToNameCache.GetOrAdd(enumType, BuildDescriptionToNameMap);

        // Return a mutable copy (callers might mutate; keep cache immutable)
        return new Dictionary<string, string>(descriptionToName);
    }

    // Build map: Enum Name -> Description (or name if no description)
    private static IReadOnlyDictionary<string, string> BuildNameToDescriptionMap(Type enumType)
    {
        var values = Enum.GetValues(enumType);
        var dict = new Dictionary<string, string>(values.Length);
        foreach (var raw in values)
        {
            var name = raw.ToString()!; // Enum.ToString never null
            var description = GetDescriptionAttribute(enumType, name) ?? name;
            dict[name] = description;
        }

        return dict;
    }

    // Build map: Description -> Enum Name (validates duplicates)
    private static IReadOnlyDictionary<string, string> BuildDescriptionToNameMap(Type enumType)
    {
        // Ensure name->description map exists so attribute parsing occurs at most once
        var nameToDescription = NameToDescriptionCache.GetOrAdd(enumType, BuildNameToDescriptionMap);
        var dict = new Dictionary<string, string>(nameToDescription.Count);
        foreach (var kvp in nameToDescription)
        {
            if (!dict.TryAdd(kvp.Value, kvp.Key))
            {
                throw new InvalidOperationException(
                    $"Duplicate description '{kvp.Value}' found in enum '{enumType.Name}'.");
            }
        }

        return dict;
    }

    // Build map: Enum Name -> Display Name (empty string if not provided)
    private static IReadOnlyDictionary<string, string> BuildNameToDisplayMap(Type enumType)
    {
        var values = Enum.GetValues(enumType);
        var dict = new Dictionary<string, string>(values.Length);
        foreach (var raw in values)
        {
            var name = raw.ToString()!;
            var display = GetDisplayAttributeName(enumType, name) ?? string.Empty;
            dict[name] = display;
        }

        return dict;
    }

    private static string? GetDescriptionAttribute(Type enumType, string enumName)
    {
        var fieldInfo = enumType.GetField(enumName, BindingFlags.Public | BindingFlags.Static);
        return fieldInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }

    private static string? GetDisplayAttributeName(Type enumType, string enumName)
    {
        var fieldInfo = enumType.GetField(enumName, BindingFlags.Public | BindingFlags.Static);
        return fieldInfo?.GetCustomAttribute<DisplayAttribute>()?.GetName();
    }

    /// <summary>
    ///     Provides extension methods for enumeration members.
    /// </summary>
    /// <param name="value">The enumeration member whose description is to be retrieved.</param>
    extension(Enum value)
    {
        /// <summary>
        ///     Retrieves the description of an enumeration member from its <see cref="DescriptionAttribute" /> or
        ///     returns the enumeration member's name if no description is available.
        /// </summary>
        /// <returns>
        ///     The description from the <see cref="DescriptionAttribute" /> or the enumeration member's name if no
        ///     description is available.
        /// </returns>
        public string GetEnumDescription()
        {
            var enumType = value.GetType();
            var name = value.ToString();
            var nameToDescription = NameToDescriptionCache.GetOrAdd(enumType, BuildNameToDescriptionMap);
            // ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault
            return nameToDescription.TryGetValue(name, out var description) ? description : name;
        }

        /// <summary>
        ///     Retrieves the display name of an enumeration member from its <see cref="DisplayAttribute" /> or returns an empty
        ///     string if no display name is available.
        /// </summary>
        /// <returns>
        ///     The display name from the <see cref="DisplayAttribute" /> or an empty string if no display name is available.
        /// </returns>
        public string GetEnumDisplay()
        {
            var enumType = value.GetType();
            var name = value.ToString();
            var nameToDisplay = NameToDisplayCache.GetOrAdd(enumType, BuildNameToDisplayMap);
            return nameToDisplay.TryGetValue(name, out var display) ? display : string.Empty;
        }
    }
}