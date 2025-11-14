using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Internals;

/// <summary>
///     Internal helper that centralises <see cref="Exception" /> creation so the JIT can keep
///     caller hot paths inlined while isolating the cold throw paths.
/// </summary>
[DebuggerNonUserCode]
[DebuggerStepThrough]
internal static class ThrowHelper
{
    /// <summary>Throws an <see cref="ArgumentNullException" /> for the supplied parameter.</summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [StackTraceHidden]
    internal static void ThrowArgumentNull(string paramName, string? message = null)
    {
        throw new ArgumentNullException(
            paramName,
            string.IsNullOrWhiteSpace(message)
                ? $"The value of '{paramName}' cannot be null."
                : message);
    }

    /// <summary>Throws an <see cref="ArgumentException" /> for the supplied parameter.</summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [StackTraceHidden]
    internal static void ThrowArgument(string paramName, string? message = null)
    {
        throw new ArgumentException(
            string.IsNullOrWhiteSpace(message)
                ? $"The value of '{paramName}' is invalid."
                : message,
            paramName);
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException" /> for the supplied parameter.</summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [StackTraceHidden]
    internal static void ThrowArgumentOutOfRange(string paramName, string? message = null)
    {
        throw new ArgumentOutOfRangeException(
            paramName,
            string.IsNullOrWhiteSpace(message)
                ? $"The value of '{paramName}' is outside the allowable range."
                : message);
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException" /> that includes the actual value.</summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [StackTraceHidden]
    internal static void ThrowArgumentOutOfRange(
        string paramName,
        object? actualValue,
        string message)
    {
        throw new ArgumentOutOfRangeException(paramName, actualValue, message);
    }
}