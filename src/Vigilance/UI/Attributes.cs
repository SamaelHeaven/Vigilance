using Vigilance.Collections;
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
        : this()
    {
        foreach (var (key, value) in attributes.AsFastEnumerable())
            Add(key, value);
    }

    public Attributes(IEnumerable<KeyValuePair<string, object>> attributes)
        : this()
    {
        foreach (var (key, value) in attributes.AsFastEnumerable())
            Add(key, value);
    }

    object IDeepCloneable.DeepClone()
    {
        var result = new Attributes();
        foreach (var (key, value) in this)
            result[key] = Cloner.CloneOrSelf(value);
        return result;
    }

    object IShallowCloneable.ShallowClone()
    {
        return new Attributes(this);
    }
}
