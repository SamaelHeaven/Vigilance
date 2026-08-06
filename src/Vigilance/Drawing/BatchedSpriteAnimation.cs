namespace Vigilance.Drawing;

[ValueWrapper(typeof(ValueAnimation<BatchedSpriteAnimationFrame>))]
public partial struct ValueBatchedSpriteAnimation : IAnimation, IArrayView<BatchedSpriteAnimationFrame>;

[ValueWrapper(typeof(ValueAnimation<BatchedSpriteAnimationFrame>))]
public sealed partial class BatchedSpriteAnimation
    : IAnimation,
        IArrayView<BatchedSpriteAnimationFrame>,
        IShallowCloneable;
