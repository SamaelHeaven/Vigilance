using Box2D.NET;

namespace Vigilance.Physics;

public enum ShapeType : sbyte
{
    Circle = B2ShapeType.b2_circleShape,
    Capsule = B2ShapeType.b2_capsuleShape,
    Segment = B2ShapeType.b2_segmentShape,
    Polygon = B2ShapeType.b2_polygonShape,
    ChainSegment = B2ShapeType.b2_chainSegmentShape,
}
