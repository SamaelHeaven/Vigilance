using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Collections;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Drawing;

public class AnimationController<TAnimation>
    : IAnimation,
        IValueDictionaryView<string, TAnimation>,
        IReadOnlyDictionary<string, TAnimation>,
        IShallowCloneable
    where TAnimation : IAnimation
{
    private readonly ValueDictionary<string, TAnimation> _animations;

    [OverloadResolutionPriority(1)]
    public AnimationController(params ReadOnlySpan<(string, TAnimation)> animations)
    {
        if (animations.Length == 0)
            throw new ArgumentException($"{nameof(AnimationController<>)} must have at least one animation.");
        _animations = animations.AsValueEnumerable().ToValueDictionary();
        Current = animations[0].Item1;
    }

    [OverloadResolutionPriority(1)]
    public AnimationController(params ReadOnlySpan<KeyValuePair<string, TAnimation>> animations)
    {
        if (animations.Length == 0)
            throw new ArgumentException($"{nameof(AnimationController<>)} must have at least one animation.");
        _animations = animations.AsValueEnumerable().ToValueDictionary();
        Current = animations[0].Key;
    }

    public AnimationController(IEnumerable<(string, TAnimation)> animations)
        : this(animations.AsSpan()) { }

    public AnimationController(IEnumerable<KeyValuePair<string, TAnimation>> animations)
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

    public TAnimation Animation { get; private set; } = default!;
    public ValueDictionary<string, TAnimation>.KeyCollection Keys => _animations.Keys;
    public ValueDictionary<string, TAnimation>.ValueCollection Values => _animations.Values;

    public void Update(TimeSpan? step = null)
    {
        Animation.Update(step);
    }

    public void Reset()
    {
        Animation.Reset();
    }

    public void Apply(Entity entity)
    {
        Animation.Apply(entity);
    }

    public bool ContainsKey(string key)
    {
        return _animations.ContainsKey(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out TAnimation value)
    {
        return _animations.TryGetValue(key, out value);
    }

    public TAnimation this[string animation] => _animations[animation];
    IEnumerable<string> IReadOnlyDictionary<string, TAnimation>.Keys => _animations.Keys;
    IEnumerable<TAnimation> IReadOnlyDictionary<string, TAnimation>.Values => _animations.Values;

    public int Count => _animations.Count;

    public ValueDictionary<string, TAnimation>.Enumerator GetEnumerator()
    {
        return _animations.GetEnumerator();
    }

    public ValueEnumerable<
        ValueDictionary<string, TAnimation>.Enumerator,
        KeyValuePair<string, TAnimation>
    > AsValueEnumerable()
    {
        return _animations.AsValueEnumerable();
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
