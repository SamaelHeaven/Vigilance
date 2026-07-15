using Box2D.NET;

namespace Vigilance.Physics;

public enum BodyType : sbyte
{
    Static = B2BodyType.b2_staticBody,
    Kinematic = B2BodyType.b2_kinematicBody,
    Dynamic = B2BodyType.b2_dynamicBody,
}
