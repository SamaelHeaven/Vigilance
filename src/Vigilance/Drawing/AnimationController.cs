using System.Diagnostics.CodeAnalysis;
using Vigilance.Core;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Drawing;

public sealed class AnimationController : IDictionaryView<string, Animation>
{
    private readonly Dictionary<string, Animation> _animations;

    public AnimationController(params ReadOnlySpan<(string, Animation)> animations)
    {
        if (animations.Length == 0)
            throw new ArgumentException("AnimationController must have at least one animation.");
        _animations = animations.AsValueEnumerable().ToDictionary();
        Current = animations[0].Item1;
    }

    public AnimationController(params ReadOnlySpan<KeyValuePair<string, Animation>> animations)
    {
        if (animations.Length == 0)
            throw new ArgumentException("AnimationController must have at least one animation.");
        _animations = animations.AsValueEnumerable().ToDictionary();
        Current = animations[0].Key;
    }

    public AnimationController(IEnumerable<(string, Animation)> animations)
        : this(animations.AsSpan()) { }

    public AnimationController(IEnumerable<KeyValuePair<string, Animation>> animations)
        : this(animations.AsSpan()) { }

    public string Current { get; private set; }

    public Animation Animation => _animations[Current];

    public Animation this[string animation] => _animations[animation];

    public Dictionary<string, Animation>.Enumerator GetEnumerator()
    {
        return _animations.GetEnumerator();
    }

    public ValueEnumerable<FromDictionary<string, Animation>, KeyValuePair<string, Animation>> AsValueEnumerable()
    {
        return _animations.AsValueEnumerable();
    }

    public bool IsUsing(string animation)
    {
        return Current == animation;
    }

    public bool Has(string animation)
    {
        return _animations.ContainsKey(animation);
    }

    public Animation Get(string animation)
    {
        return _animations[animation];
    }

    public bool TryGet(string animation, [MaybeNullWhen(false)] out Animation animationOut)
    {
        return _animations.TryGetValue(animation, out animationOut!);
    }

    public void Use(string animation, bool resetOthers = true)
    {
        if (!_animations.ContainsKey(animation))
            throw new KeyNotFoundException(animation);
        Current = animation;
        if (!resetOthers)
            return;
        foreach (var key in _animations.Keys.AsValueEnumerable().Where(key => key != animation))
        {
            var value = _animations[key];
            value.Reset();
        }
    }
}
