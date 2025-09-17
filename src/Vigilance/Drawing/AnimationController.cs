using System.Collections.Immutable;
using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed class AnimationController
    : IValueEnumerable<ImmutableDictionary<string, Animation>.Enumerator, KeyValuePair<string, Animation>>
{
    private readonly ImmutableDictionary<string, Animation> _animations;

    public AnimationController(IEnumerable<(string, Animation)> animations)
        : this(animations.Select(x => new KeyValuePair<string, Animation>(x.Item1, x.Item2))) { }

    public AnimationController(IEnumerable<KeyValuePair<string, Animation>> animations)
    {
        var list = animations as IReadOnlyList<KeyValuePair<string, Animation>> ?? animations.ToList();
        if (list.Count == 0)
            throw new ArgumentException("AnimationController must have at least one animation.");
        _animations = list.ToImmutableDictionary();
        Current = list[0].Key;
    }

    public string Current { get; private set; }

    public Animation Animation => _animations[Current];

    public Animation this[string animation] => _animations[animation];

    public ImmutableDictionary<string, Animation>.Enumerator GetEnumerator()
    {
        return _animations.GetEnumerator();
    }

    public bool Has(string animation)
    {
        return _animations.ContainsKey(animation);
    }

    public Animation Get(string animation)
    {
        return _animations[animation];
    }

    public bool TryGet(string animation, out Animation animationOut)
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
        foreach (var key in _animations.Keys.Where(key => key != animation))
        {
            var value = _animations[key];
            value.Reset();
        }
    }
}
