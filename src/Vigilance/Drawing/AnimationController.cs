using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Drawing;

[ValueWrapper(typeof(ValueAnimationController<,>), typeParams: [typeof(string), null])]
public partial struct ValueAnimationController<TAnimation>
    : IValueDictionaryView<string, TAnimation>,
        IReadOnlyDictionary<string, TAnimation>
    where TAnimation : IAnimation;

public struct ValueAnimationController<TKey, TAnimation>
    : IAnimation,
        IValueDictionaryView<TKey, TAnimation>,
        IReadOnlyDictionary<TKey, TAnimation>
    where TAnimation : IAnimation
    where TKey : notnull
{
    private readonly ValueDictionary<TKey, TAnimation> _animations;

    [OverloadResolutionPriority(1)]
    public ValueAnimationController(params ReadOnlySpan<(TKey, TAnimation)> animations)
    {
        if (animations.Length == 0)
            throw new ArgumentException($"{nameof(ValueAnimationController<,>)} must have at least one animation.");
        _animations = animations.AsValueEnumerable().ToValueDictionary();
        Current = animations[0].Item1;
    }

    [OverloadResolutionPriority(1)]
    public ValueAnimationController(params ReadOnlySpan<KeyValuePair<TKey, TAnimation>> animations)
    {
        if (animations.Length == 0)
            throw new ArgumentException($"{nameof(ValueAnimationController<,>)} must have at least one animation.");
        _animations = animations.AsValueEnumerable().ToValueDictionary();
        Current = animations[0].Key;
    }

    public ValueAnimationController(IEnumerable<(TKey, TAnimation)> animations)
        : this(animations.AsSpan()) { }

    public ValueAnimationController(IEnumerable<KeyValuePair<TKey, TAnimation>> animations)
        : this(animations.AsSpan()) { }

    public TKey Current { get; private set; }

    public readonly ref TAnimation Animation => ref _animations.GetValueRefOrNullRef(Current);

    readonly IEnumerable<TKey> IReadOnlyDictionary<TKey, TAnimation>.Keys => _animations.Keys;
    readonly IEnumerable<TAnimation> IReadOnlyDictionary<TKey, TAnimation>.Values => _animations.Values;
    public readonly ValueDictionary<TKey, TAnimation>.KeyCollection Keys => _animations.Keys;
    public readonly ValueDictionary<TKey, TAnimation>.ValueCollection Values => _animations.Values;

    [OverloadResolutionPriority(1)]
    public readonly ref TAnimation this[in TKey animation] => ref _animations.GetValueRefOrNullRef(animation);

    public bool IsPaused { get; set; }

    public void Update(TimeSpan? step = null)
    {
        if (IsPaused)
            return;
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

    public readonly bool ContainsKey(TKey key)
    {
        return ContainsKey(key);
    }

    public readonly bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TAnimation value)
    {
        return TryGetValue(key, out value);
    }

    public readonly TAnimation this[TKey key] => this[key];

    public readonly int Count => _animations.Count;

    public readonly ValueDictionary<TKey, TAnimation>.Enumerator GetEnumerator()
    {
        return _animations.GetEnumerator();
    }

    public readonly ValueEnumerable<
        ValueDictionary<TKey, TAnimation>.Enumerator,
        KeyValuePair<TKey, TAnimation>
    > AsValueEnumerable()
    {
        return _animations.AsValueEnumerable();
    }

    [OverloadResolutionPriority(1)]
    public readonly bool ContainsKey(in TKey key)
    {
        return _animations.ContainsKey(key);
    }

    [OverloadResolutionPriority(1)]
    public readonly bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TAnimation value)
    {
        return _animations.TryGetValue(key, out value);
    }

    public readonly bool IsUsing(in TKey animation)
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
            var key in _animations
                .Keys.AsValueEnumerable()
                .Cross(animation.AsValueSingleton())
                .Where(cross => !EqualityComparer<TKey>.Default.Equals(cross.Left, cross.Right))
                .Select(cross => cross.Left)
        )
            this[key].Reset();
    }
}

[ValueWrapper(typeof(ValueAnimationController<,>), typeParams: [typeof(string), null])]
public partial class AnimationController<TAnimation>
    : IValueDictionaryView<string, TAnimation>,
        IReadOnlyDictionary<string, TAnimation>,
        IShallowCloneable
    where TAnimation : IAnimation;

[ValueWrapper(typeof(ValueAnimationController<,>))]
public partial class AnimationController<TKey, TAnimation>
    : IAnimation,
        IValueDictionaryView<TKey, TAnimation>,
        IReadOnlyDictionary<TKey, TAnimation>,
        IShallowCloneable
    where TAnimation : IAnimation
    where TKey : notnull;
