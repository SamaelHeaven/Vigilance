using Vigilance.Core;

namespace Vigilance.UI;

public sealed class Attributes : Dictionary<string, object>, IDeepCloneable
{
    public Attributes()
        : base(StringComparer.OrdinalIgnoreCase) { }

    object IDeepCloneable.DeepClone()
    {
        var result = new Attributes();
        foreach (var (key, value) in this)
            result[key] = value switch
            {
                IDeepCloneable deepCloneable => deepCloneable.DeepClone(),
                IShallowCloneable shallowCloneable => shallowCloneable.ShallowClone(),
                _ => value,
            };
        return result;
    }
}
