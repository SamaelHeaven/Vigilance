namespace Vigilance.Core;

public static class EnumExtensions<T>
    where T : struct, Enum
{
    private static readonly T[] _values = Enum.GetValues<T>();

    public static ReadOnlySpan<T> Values => _values.AsSpan();
}

public static class EnumExtensions
{
    extension<T>(T)
        where T : struct, Enum
    {
        public static ReadOnlySpan<T> Values()
        {
            return EnumExtensions<T>.Values;
        }
    }
}
