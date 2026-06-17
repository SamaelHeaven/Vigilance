using Raylib_cs;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed class Line : Drawable<Line>
{
    public Line() { }

    public Line(Color color)
    {
        Color = color;
    }

    public Line(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }

    public Line(Vector2 start, Vector2 end, Color color)
    {
        Start = start;
        End = end;
        Color = color;
    }

    public Vector2 Start { get; set; } = Vector2.Zero;
    public Vector2 End { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Drawing.DefaultFill;
    public float Thick { get; set; } = Drawing.DefaultStrokeWidth == 0 ? 1 : Drawing.DefaultStrokeWidth;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    protected override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawLine(transform, this);
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
                || (
                    graphics.Culling()
                    && !graphics.IsPolygonInBoundsSpan(new Quad(start, start, end, end), camera, thickValue * 0.5f)
                )
            )
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawLineEx(start, end, thickValue, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void DrawLine(Line line)
        {
            graphics.DrawLine(new Transform(), line);
        }

        public void DrawLine(Transform transform, Line line)
        {
            line.OnBeginDrawing?.Invoke(transform, line, graphics);
            transform += line.Transform;
            var camera = line.Camera.Get();
            var position = transform.Position;
            var start = line.Start + position;
            var end = line.End + position;
            var color = line.Color;
            var thick = line.Thick;
            var scale = transform.Scale.Abs().Min();
            graphics.PushMatrix();
            graphics.Pivot(transform, false);
            graphics.DrawLine(start, end, color, thick * scale, camera);
            graphics.PopMatrix();
            line.OnEndDrawing?.Invoke(transform, line, graphics);
        }
    }
}
