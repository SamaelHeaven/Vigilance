using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(object))]
    private static readonly MethodInfo MemberwiseCloneMethod = typeof(object).GetMethod(
        "MemberwiseClone",
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    public static object MemberwiseClone(object obj)
    {
        return MemberwiseCloneMethod.Invoke(obj, null)!;
    }
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
