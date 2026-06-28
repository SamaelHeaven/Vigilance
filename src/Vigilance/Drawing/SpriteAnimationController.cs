using System.Diagnostics.CodeAnalysis;
using Vigilance.Collections;
using ZLinq;

namespace Vigilance.Drawing;

public sealed class SpriteAnimationController
    : IValueDictionaryView<string, SpriteAnimation>,
        IReadOnlyDictionary<string, SpriteAnimation>
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
    public ValueDictionary<string, SpriteAnimation>.KeyCollection Keys => _animations.Keys;
    public ValueDictionary<string, SpriteAnimation>.ValueCollection Values => _animations.Values;

    public bool ContainsKey(string key)
    {
        return _animations.ContainsKey(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out SpriteAnimation value)
    {
        return _animations.TryGetValue(key, out value);
    }

    public SpriteAnimation this[string animation] => _animations[animation];
    IEnumerable<string> IReadOnlyDictionary<string, SpriteAnimation>.Keys => _animations.Keys;
    IEnumerable<SpriteAnimation> IReadOnlyDictionary<string, SpriteAnimation>.Values => _animations.Values;

    public int Count => _animations.Count;

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

    public void Use(string animation, bool resetOthers = true)
    {
        if (!_animations.ContainsKey(animation))
            throw new KeyNotFoundException(animation);
        Current = animation;
        if (!resetOthers)
            return;
        foreach (
            var value in _animations.AsValueEnumerable().Where(pair => pair.Key != animation).Select(pair => pair.Value)
        )
            value.Reset();
    }
}
