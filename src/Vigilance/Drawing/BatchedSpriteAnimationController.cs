namespace Vigilance.Drawing;

[ValueWrapper(typeof(ValueAnimationController<,>), typeParams: [typeof(string), typeof(ValueBatchedSpriteAnimation)])]
public partial struct ValueBatchedSpriteAnimationController
    : IAnimation,
        IValueDictionaryView<string, ValueBatchedSpriteAnimation>,
        IReadOnlyDictionary<string, ValueBatchedSpriteAnimation>;

[ValueWrapper(typeof(ValueAnimationController<,>), typeParams: [typeof(string), typeof(BatchedSpriteAnimation)])]
public sealed partial class BatchedSpriteAnimationController
    : IAnimation,
        IValueDictionaryView<string, BatchedSpriteAnimation>,
        IReadOnlyDictionary<string, BatchedSpriteAnimation>,
        IShallowCloneable;

[ValueWrapper(typeof(ValueAnimationController<,>), typeParams: [null, typeof(ValueBatchedSpriteAnimation)])]
public partial struct ValueBatchedSpriteAnimationController<TKey>
    : IAnimation,
        IValueDictionaryView<TKey, ValueBatchedSpriteAnimation>,
        IReadOnlyDictionary<TKey, ValueBatchedSpriteAnimation>
    where TKey : notnull;

[ValueWrapper(typeof(ValueAnimationController<,>), typeParams: [null, typeof(BatchedSpriteAnimation)])]
public sealed partial class BatchedSpriteAnimationController<TKey>
    : IAnimation,
        IValueDictionaryView<TKey, BatchedSpriteAnimation>,
        IReadOnlyDictionary<TKey, BatchedSpriteAnimation>,
        IShallowCloneable
    where TKey : notnull;
