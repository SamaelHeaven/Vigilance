namespace Vigilance.Physics;

[Flags]
public enum DebugDrawFlags : ushort
{
    None = 0,
    Shapes = 1 << 0,
    Joints = 1 << 1,
    JointExtras = 1 << 2,
    Bounds = 1 << 3,
    Mass = 1 << 4,
    BodyNames = 1 << 5,
    ContactPoints = 1 << 6,
    GraphColors = 1 << 7,
    ContactFeatures = 1 << 8,
    ContactNormals = 1 << 9,
    ContactForces = 1 << 10,
    FrictionForces = 1 << 11,
    Islands = 1 << 12,

    Default = Shapes | Joints | ContactPoints,

    All =
        Shapes
        | Joints
        | JointExtras
        | Bounds
        | Mass
        | BodyNames
        | ContactPoints
        | GraphColors
        | ContactFeatures
        | ContactNormals
        | ContactForces
        | FrictionForces
        | Islands,
}
