using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LibraryCore.Core.ThrowUtilities;

/// <summary>
/// C# 10 feature - inline the throw statements and reduce it down. The expression passed in by the compiler gives you the exact claus that was passed in
/// </summary>
public static class ThrowUtility
{
    //these should be inlined so don't try to clean them up

    [DoesNotReturn]
    private static void ThrowIfFalseException(string? expression) => throw new ArgumentException($"{expression} must be True, but was False");

    [DoesNotReturn]
    private static void ThrowIfTrueException(string? expression) => throw new ArgumentException($"{expression} must be False, but was True");

    public static void ThrowIfFalse(bool value, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        if (!value)
        {
            ThrowIfFalseException(expression);
        }
    }

    public static void ThrowIfTrue(bool value, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        if (value)
        {
            ThrowIfTrueException(expression);
        }
    }

    [DoesNotReturn]
    private static void ThrowEnumerableIsNullOrEmpty(string? expression) => throw new ArgumentOutOfRangeException(expression, "The enumerable is null or empty");

    /// <summary>
    /// Throw if the array is null or empty
    /// </summary>
    /// <param name="value">Source array to check</param>
    /// <param name="expression">expression to set if it results in an error</param>
    public static void ThrowIfNullOrEmpty<T>(IEnumerable<T> value, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        if (value.IsNullOrEmpty())
        {
            ThrowEnumerableIsNullOrEmpty(expression);
        }
    }

}
