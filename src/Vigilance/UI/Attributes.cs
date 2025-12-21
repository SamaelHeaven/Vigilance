using Vigilance.Core;

namespace Vigilance.UI;

public sealed class Attributes() : Dictionary<string, object>, IFullCloneable
{
    public Attributes(params ReadOnlySpan<(string, object)> attributes)
        : this()
    {
        foreach (var (key, value) in attributes)
            Add(key, value);
    }

    public Attributes(params ReadOnlySpan<KeyValuePair<string, object>> attributes)
        : this()
    {
        foreach (var (key, value) in attributes)
            Add(key, value);
    }

    public Attributes(IEnumerable<(string, object)> attributes)
        : this(attributes.AsSpan()) { }

    public Attributes(IEnumerable<KeyValuePair<string, object>> attributes)
        : this(attributes.AsSpan()) { }

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

    object IShallowCloneable.ShallowClone()
    {
        return new Attributes(this);
    }
}
