using System.Diagnostics.CodeAnalysis;
using Vigilance.Collections;
using ZLinq;

namespace Vigilance.Drawing;

public sealed class SpriteAnimationController : IValueDictionaryView<string, SpriteAnimation>
{
    private readonly ValueDictionary<string, SpriteAnimation> _animations;

    public SpriteAnimationController(params ReadOnlySpan<(string, SpriteAnimation)> animations)
    {
        if (animations.Length == 0)
            throw new ArgumentException($"{nameof(SpriteAnimationController)} must have at least one animation.");
        _animations = animations.AsValueEnumerable().ToValueDictionary();
        Current = animations[0].Item1;
    }

    public SpriteAnimationController(params ReadOnlySpan<KeyValuePair<string, SpriteAnimation>> animations)
    {
        if (animations.Length == 0)
            throw new ArgumentException($"{nameof(SpriteAnimationController)} must have at least one animation.");
        _animations = animations.AsValueEnumerable().ToValueDictionary();
        Current = animations[0].Key;
    }

    public SpriteAnimationController(IEnumerable<(string, SpriteAnimation)> animations)
        : this(animations.AsSpan()) { }

    public SpriteAnimationController(IEnumerable<KeyValuePair<string, SpriteAnimation>> animations)
        : this(animations.AsSpan()) { }

    public string Current
    {
        get;
        private set
        {
            if (field == value)
                return;
            field = value;
            Animation = _animations[Current];
        }
    }

    public SpriteAnimation Animation { get; private set; } = null!;

    public SpriteAnimation this[string animation] => _animations[animation];

    public ValueDictionary<string, SpriteAnimation>.Enumerator GetEnumerator()
    {
        return _animations.GetEnumerator();
    }

    public ValueEnumerable<
        StructEnumerator<ValueDictionary<string, SpriteAnimation>.Enumerator, KeyValuePair<string, SpriteAnimation>>,
        KeyValuePair<string, SpriteAnimation>
    > AsValueEnumerable()
    {
        return new StructEnumerator<
            ValueDictionary<string, SpriteAnimation>.Enumerator,
            KeyValuePair<string, SpriteAnimation>
        >(GetEnumerator());
    }

    public bool IsUsing(string animation)
    {
        return Current == animation;
    }

    public bool Has(string animation)
    {
        return _animations.ContainsKey(animation);
    }

    public SpriteAnimation Get(string animation)
    {
        return _animations[animation];
    }

    public bool TryGet(string animation, [MaybeNullWhen(false)] out SpriteAnimation animationOut)
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
