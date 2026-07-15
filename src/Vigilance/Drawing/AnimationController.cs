using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Collections;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Drawing;

public class AnimationController<TAnimation> : AnimationController<string, TAnimation>
    where TAnimation : IAnimation
{
    [OverloadResolutionPriority(1)]
    public AnimationController(params ReadOnlySpan<(string, TAnimation)> animations)
        : base(animations) { }

    [OverloadResolutionPriority(1)]
    public AnimationController(params ReadOnlySpan<KeyValuePair<string, TAnimation>> animations)
        : base(animations) { }

    public AnimationController(IEnumerable<(string, TAnimation)> animations)
        : base(animations) { }

    public AnimationController(IEnumerable<KeyValuePair<string, TAnimation>> animations)
        : base(animations) { }
}

public class AnimationController<TKey, TAnimation>
    : IAnimation,
        IValueDictionaryView<TKey, TAnimation>,
        IReadOnlyDictionary<TKey, TAnimation>,
        IShallowCloneable
    where TAnimation : IAnimation
    where TKey : notnull
{
    private readonly ValueDictionary<TKey, TAnimation> _animations;

    private TKey _current;

    [OverloadResolutionPriority(1)]
    public AnimationController(params ReadOnlySpan<(TKey, TAnimation)> animations)
    {
        if (animations.Length == 0)
            throw new ArgumentException($"{nameof(AnimationController<,>)} must have at least one animation.");
        _animations = animations.AsValueEnumerable().ToValueDictionary();
        _current = animations[0].Item1;
        Animation = _animations[Current];
    }

    [OverloadResolutionPriority(1)]
    public AnimationController(params ReadOnlySpan<KeyValuePair<TKey, TAnimation>> animations)
    {
        if (animations.Length == 0)
            throw new ArgumentException($"{nameof(AnimationController<,>)} must have at least one animation.");
        _animations = animations.AsValueEnumerable().ToValueDictionary();
        _current = animations[0].Key;
        Animation = _animations[Current];
    }

    public AnimationController(IEnumerable<(TKey, TAnimation)> animations)
        : this(animations.AsSpan()) { }

    public AnimationController(IEnumerable<KeyValuePair<TKey, TAnimation>> animations)
        : this(animations.AsSpan()) { }

    public TKey Current
    {
        get => _current;
        private set
        {
            if (EqualityComparer<TKey>.Default.Equals(_current, value))
                return;
            _current = value;
            Animation = _animations[Current];
        }
    }

    public TAnimation Animation { get; private set; }

    public ValueDictionary<TKey, TAnimation>.KeyCollection Keys => _animations.Keys;
    public ValueDictionary<TKey, TAnimation>.ValueCollection Values => _animations.Values;

    public TAnimation this[in TKey animation] => _animations[animation];

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

    bool IReadOnlyDictionary<TKey, TAnimation>.ContainsKey(TKey key)
    {
        return ContainsKey(key);
    }

    bool IReadOnlyDictionary<TKey, TAnimation>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TAnimation value)
    {
        return TryGetValue(key, out value);
    }

    TAnimation IReadOnlyDictionary<TKey, TAnimation>.this[TKey key] => this[key];
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TAnimation>.Keys => _animations.Keys;
    IEnumerable<TAnimation> IReadOnlyDictionary<TKey, TAnimation>.Values => _animations.Values;

    public int Count => _animations.Count;

    public ValueDictionary<TKey, TAnimation>.Enumerator GetEnumerator()
    {
        return _animations.GetEnumerator();
    }

    public ValueEnumerable<
        ValueDictionary<TKey, TAnimation>.Enumerator,
        KeyValuePair<TKey, TAnimation>
    > AsValueEnumerable()
    {
        return _animations.AsValueEnumerable();
    }

    public bool ContainsKey(in TKey key)
    {
        return _animations.ContainsKey(key);
    }

    public bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TAnimation value)
    {
        return _animations.TryGetValue(key, out value);
    }

    public bool IsUsing(in TKey animation)
    {
        return EqualityComparer<TKey>.Default.Equals(Current, animation);
    }

    public void Use(in TKey animation, bool resetOthers = true)
    {
        if (!_animations.ContainsKey(animation))
            throw new KeyNotFoundException(animation.ToString());
        Current = animation;
        if (!resetOthers)
            return;
        foreach (
            var value in _animations
                .AsValueEnumerable()
                .Cross(animation.AsValueSingleton())
                .Where(pair => !EqualityComparer<TKey>.Default.Equals(pair.Left.Key, pair.Right))
                .Select(pair => pair.Left.Value)
        )
            value.Reset();
    }
}
