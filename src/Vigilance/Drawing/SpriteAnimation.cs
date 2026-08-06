namespace Vigilance.Drawing;

[ValueWrapper(typeof(ValueAnimation<SpriteAnimationFrame>))]
public partial struct ValueSpriteAnimation : IAnimation, IArrayView<SpriteAnimationFrame>;

[ValueWrapper(typeof(ValueAnimation<SpriteAnimationFrame>))]
public sealed partial class SpriteAnimation : IAnimation, IArrayView<SpriteAnimationFrame>, IShallowCloneable;
