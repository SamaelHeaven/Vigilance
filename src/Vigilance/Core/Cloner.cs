using System.Runtime.CompilerServices;

namespace Vigilance.Core;

public interface IShallowCloneable
{
    object ShallowClone()
    {
        return Cloner.MemberwiseClone(this);
    }
}

public interface IDeepCloneable
{
    object DeepClone()
    {
        return Cloner.MemberwiseClone(this);
    }
}

public interface IFullCloneable : IShallowCloneable, IDeepCloneable;

public static class Cloner
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "MemberwiseClone")]
    public static extern object MemberwiseClone(object obj);
}

public static class CloneableExtensions
{
    public static T ShallowClone<T>(this T obj)
        where T : IShallowCloneable
    {
        return (T)obj.ShallowClone();
    }

    public static T DeepClone<T>(this T obj)
        where T : IDeepCloneable
    {
        return (T)obj.DeepClone();
    }
}
