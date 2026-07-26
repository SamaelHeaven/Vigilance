using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Core;

public static class Debug
{
    public static bool IsAssertEnabled
    {
        get;
        // ReSharper disable once ValueParameterNotUsed
        set
        {
            // csharpier-ignore
#if VIGILANCE_ASSERTS
            field = value;
#endif
        }
    } =
#if VIGILANCE_ASSERTS
        true;
#else
        false;
#endif

    [Conditional("VIGILANCE_ASSERTS")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Assert(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))] string message = ""
    )
    {
        if (IsAssertEnabled && !condition)
            throw new AssertionException(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EnableAssert()
    {
#if VIGILANCE_ASSERTS
        var previous = IsAssertEnabled;
        IsAssertEnabled = true;
        return previous;
#else
        return false;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DisableAssert()
    {
#if VIGILANCE_ASSERTS
        var previous = IsAssertEnabled;
        IsAssertEnabled = false;
        return previous;
#else
        return false;
#endif
    }
}
