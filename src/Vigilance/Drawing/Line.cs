using Raylib_cs;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

[ValueWrapper(typeof(Drawable<ValueLine>), "Drawable")]
public partial struct ValueLine : IDrawable
{
    public ValueLine(Color color)
        : this()
    {
        Color = color;
    }

    public ValueLine(Vector2 start, Vector2 end)
        : this()
    {
        Start = start;
        End = end;
    }

    public ValueLine(Vector2 start, Vector2 end, Color color)
        : this()
    {
        Start = start;
        End = end;
        Color = color;
    }

    public Vector2 Start { get; set; } = Vector2.Zero;
    public Vector2 End { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Drawing.DefaultFill;
    public float Thick { get; set; } = Drawing.DefaultStrokeWidth == 0 ? 1 : Drawing.DefaultStrokeWidth;

    public override readonly string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    public readonly void Draw(Transform transform, Graphics graphics)
    {
        graphics.DrawLine(transform, this);
    }
}

[ValueWrapper(typeof(ValueLine))]
public sealed partial class Line : IDrawable, IFullCloneable
{
    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }
}

public static class LineExtensions
{
    extension(Graphics graphics)
    {
        public void DrawLine(
            float startX,
            float startY,
            float endX,
            float endY,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            graphics.DrawLine(new Vector2(startX, startY), new Vector2(endX, endY), color, thick, camera);
        }

        public void DrawLine(
            Vector2 start,
            Vector2 end,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (
                colorValue == Color.Transparent
                || thickValue <= 0
                || graphics.Culling()
                    && !graphics.IsPolygonInBounds(new Quad(start, start, end, end), camera, thickValue * 0.5f)
            )
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawLineEx(start, end, thickValue, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void DrawLine(in ValueLine line)
        {
            graphics.DrawLine(new Transform(), line);
        }

        public void DrawLine(Transform transform, in ValueLine line)
        {
            using var _ = Drawable<ValueLine>.EnterDrawing(ref transform, line.Drawable, line, graphics);
            var camera = line.Camera.Get();
            var position = transform.Position;
            var start = line.Start + position;
            var end = line.End + position;
            var color = line.Color;
            var thick = line.Thick;
            var scale = transform.Scale.Abs().Min();
            graphics.Pivot(transform, false);
            graphics.DrawLine(start, end, color, thick * scale, camera);
        }
    }
}
