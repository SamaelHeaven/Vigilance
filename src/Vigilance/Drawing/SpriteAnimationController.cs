namespace Vigilance.Drawing;

[ValueWrapper(typeof(ValueAnimationController<,>), typeParams: [typeof(string), typeof(ValueSpriteAnimation)])]
public partial struct ValueSpriteAnimationController
    : IAnimation,
        IValueDictionaryView<string, ValueSpriteAnimation>,
        IReadOnlyDictionary<string, ValueSpriteAnimation>;

[ValueWrapper(typeof(ValueAnimationController<,>), typeParams: [typeof(string), typeof(SpriteAnimation)])]
public sealed partial class SpriteAnimationController
    : IAnimation,
        IValueDictionaryView<string, SpriteAnimation>,
        IReadOnlyDictionary<string, SpriteAnimation>,
        IShallowCloneable;

[ValueWrapper(typeof(ValueAnimationController<,>), typeParams: [null, typeof(ValueSpriteAnimation)])]
public partial struct ValueSpriteAnimationController<TKey>
    : IAnimation,
        IValueDictionaryView<TKey, ValueSpriteAnimation>,
        IReadOnlyDictionary<TKey, ValueSpriteAnimation>
    where TKey : notnull;

[ValueWrapper(typeof(ValueAnimationController<,>), typeParams: [null, typeof(SpriteAnimation)])]
public sealed partial class SpriteAnimationController<TKey>
    : IAnimation,
        IValueDictionaryView<TKey, SpriteAnimation>,
        IReadOnlyDictionary<TKey, SpriteAnimation>,
        IShallowCloneable
    where TKey : notnull;
