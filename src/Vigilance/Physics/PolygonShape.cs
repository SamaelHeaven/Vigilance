using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Box2D.NET;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Physics;

public record struct PolygonShape
{
    public InlineList<InlineArray8<Vector2>, Vector2> Vertices { get; set; }
    public InlineList<InlineArray8<Vector2>, Vector2> Normals { get; set; }
    public Vector2 Centroid { get; set; }
    public float Radius { get; set; }

    internal readonly B2Polygon B2Polygon
    {
        get
        {
            var vertices = Vertices;
            var normals = Normals;
            Debug.Assert(vertices.Count == Normals.Count);
            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = World.PixelsToMeters(vertices[i]);
            return new B2Polygon
            {
                vertices = Unsafe.As<InlineList<InlineArray8<Vector2>, Vector2>, B2FixedArray8<B2Vec2>>(ref vertices),
                normals = Unsafe.As<InlineList<InlineArray8<Vector2>, Vector2>, B2FixedArray8<B2Vec2>>(ref normals),
                centroid = World.PixelsToMeters(Centroid).B2Vec2,
                radius = World.PixelsToMeters(Radius),
                count = vertices.Count,
            };
        }
    }

    public static PolygonShape Make(in ReadOnlySpan<Vector2> points, float radius)
    {
        Debug.Assert(points.Length <= 8);
        var b2Hull = new B2Hull();
        var b2Points = MemoryMarshal.Cast<B2Vec2, Vector2>(b2Hull.points.AsSpan());
        for (var i = 0; i < points.Length; i++)
            b2Points[i] = World.PixelsToMeters(points[i]);
        b2Hull.count = points.Length;
        return B2Geometries.b2MakePolygon(b2Hull, World.PixelsToMeters(radius)).ToPolygon();
    }

    public static PolygonShape Make(in ReadOnlySpan<Vector2> points, Vector2 offset, float rotation)
    {
        Debug.Assert(points.Length <= 8);
        var b2Hull = new B2Hull();
        var b2Points = MemoryMarshal.Cast<B2Vec2, Vector2>(b2Hull.points.AsSpan());
        for (var i = 0; i < points.Length; i++)
            b2Points[i] = World.PixelsToMeters(points[i]);
        b2Hull.count = points.Length;
        return B2Geometries
            .b2MakeOffsetPolygon(b2Hull, World.PixelsToMeters(offset).B2Vec2, B2Rot.FromDegrees(rotation))
            .ToPolygon();
    }

    public static PolygonShape MakeRounded(
        in ReadOnlySpan<Vector2> points,
        Vector2 offset,
        float rotation,
        float radius
    )
    {
        Debug.Assert(points.Length <= 8);
        var b2Hull = new B2Hull();
        var b2Points = MemoryMarshal.Cast<B2Vec2, Vector2>(b2Hull.points.AsSpan());
        for (var i = 0; i < points.Length; i++)
            b2Points[i] = World.PixelsToMeters(points[i]);
        b2Hull.count = points.Length;
        return B2Geometries
            .b2MakeOffsetRoundedPolygon(
                b2Hull,
                World.PixelsToMeters(offset).B2Vec2,
                B2Rot.FromDegrees(rotation),
                World.PixelsToMeters(radius)
            )
            .ToPolygon();
    }

    public static PolygonShape MakeSquare(float size)
    {
        return B2Geometries.b2MakeSquare(World.PixelsToMeters(size * 0.5f)).ToPolygon();
    }

    public static PolygonShape MakeBox(Vector2 size)
    {
        return B2Geometries
            .b2MakeBox(World.PixelsToMeters(size.X * 0.5f), World.PixelsToMeters(size.Y * 0.5f))
            .ToPolygon();
    }

    public static PolygonShape MakeBox(Vector2 size, Vector2 offset, float rotation)
    {
        return B2Geometries
            .b2MakeOffsetBox(
                World.PixelsToMeters(size.X * 0.5f),
                World.PixelsToMeters(size.Y * 0.5f),
                World.PixelsToMeters(offset).B2Vec2,
                B2Rot.FromDegrees(rotation)
            )
            .ToPolygon();
    }

    public static PolygonShape MakeRoundedBox(Vector2 size, float radius)
    {
        return B2Geometries
            .b2MakeRoundedBox(
                World.PixelsToMeters(size.X * 0.5f),
                World.PixelsToMeters(size.Y * 0.5f),
                World.PixelsToMeters(radius)
            )
            .ToPolygon();
    }

    public static PolygonShape MakeRoundedBox(Vector2 size, Vector2 offset, float rotation, float radius)
    {
        return B2Geometries
            .b2MakeOffsetRoundedBox(
                World.PixelsToMeters(size.X * 0.5f),
                World.PixelsToMeters(size.Y * 0.5f),
                World.PixelsToMeters(offset).B2Vec2,
                B2Rot.FromDegrees(rotation),
                World.PixelsToMeters(radius)
            )
            .ToPolygon();
    }

    internal readonly B2ShapeProxy MakeProxy()
    {
        var proxy = new B2ShapeProxy { count = Vertices.Count, radius = World.PixelsToMeters(Radius) };
        ref var pts = ref Unsafe.As<B2FixedArray8<B2Vec2>, InlineArray8<Vector2>>(ref proxy.points);
        for (var i = 0; i < Vertices.Count; i++)
            pts[i] = World.PixelsToMeters(Vertices[i]);
        return proxy;
    }
}
