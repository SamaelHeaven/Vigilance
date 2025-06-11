using System.Collections;
using System.Collections.Immutable;

namespace Vigilance.Drawing;

public sealed class AnimationController : IEnumerable<KeyValuePair<string, Animation>>
{
    private readonly ImmutableDictionary<string, Animation> _animations;
    private string _currentAnimation;

    public AnimationController(IEnumerable<(string, Animation)> animations)
        : this(animations.Select(x => new KeyValuePair<string, Animation>(x.Item1, x.Item2))) { }

    public AnimationController(IEnumerable<KeyValuePair<string, Animation>> animations)
    {
        var list = animations as IList<KeyValuePair<string, Animation>> ?? animations.ToList();
        if (list.Count == 0)
            throw new ArgumentException("AnimationController must have at least one animation.");
        _animations = list.ToImmutableDictionary();
        _currentAnimation = list.First().Key;
    }

    public Animation Animation => _animations[_currentAnimation];

    public Animation this[string animation] => _animations[animation];

    public IEnumerator<KeyValuePair<string, Animation>> GetEnumerator()
    {
        return _animations.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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
        _currentAnimation = animation;
        if (!resetOthers)
            return;
        foreach (var key in _animations.Keys.Where(key => key != animation))
        {
            var value = _animations[key];
            value.Reset();
        }
    }
}
