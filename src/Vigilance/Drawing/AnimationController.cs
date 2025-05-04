using System.Collections;

namespace Vigilance.Drawing;

public sealed class AnimationController : IEnumerable<KeyValuePair<string, Animation>>
{
    private readonly Dictionary<string, Animation> _animations;
    private string _currentAnimation;

    public AnimationController(IReadOnlyDictionary<string, Animation> animations)
    {
        if (animations.Count == 0)
            throw new ArgumentException("AnimationController must have at least one animation.");
        _animations = animations.ToDictionary();
        _currentAnimation = animations.First().Key;
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
            return;
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
