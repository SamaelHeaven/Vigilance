#pragma warning disable CS9084

using System.Runtime.InteropServices;
using Vigilance.Core;

namespace Vigilance.Drawing;

public unsafe struct AnimationController
{
    private readonly Dictionary<string, Animation> _animations;
    private string? _currentAnimation = null;

    public ref Animation Animation => ref CollectionsMarshal.GetValueRefOrNullRef(_animations, _currentAnimation!);

    public AnimationController(IReadOnlyDictionary<string, Animation> animations)
    {
        if (animations.Count == 0)
            throw new ArgumentException("AnimationController must have at least one animation.");
        _animations = animations.ToDictionary();
        _currentAnimation = animations.First().Key;
    }

    public bool Has(string animation)
    {
        return _animations.ContainsKey(animation);
    }

    public ref Animation Get(string animation)
    {
        return ref CollectionsMarshal.GetValueRefOrNullRef(_animations, animation);
    }

    public ref AnimationController Use(string animation, bool resetOthers = true)
    {
        if (!_animations.ContainsKey(animation))
            return ref this;
        _currentAnimation = animation;
        if (!resetOthers)
            return ref this;
        foreach (var key in _animations.Keys.Where(key => key != animation))
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(_animations, key);
            value.Reset();
        }

        return ref this;
    }

    public ref AnimationController Each(RefAction<Animation> action)
    {
        foreach (var key in _animations.Keys)
            action.Invoke(ref CollectionsMarshal.GetValueRefOrNullRef(_animations, key));
        return ref this;
    }
}
