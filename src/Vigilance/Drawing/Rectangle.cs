using Raylib_cs;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed class Rectangle : Drawable<Rectangle>
{
    public Rectangle() { }

    public Rectangle(Color fill)
    {
        Fill = fill;
    }

    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public float Radius { get; set; } = Drawing.DefaultRadius;
    public int Segments { get; set; } = 0;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    public override void Draw(Transform transform, Graphics graphics)
    {
        graphics.DrawRectangle(transform, this);
    }
}

public static class RectangleExtensions
{
    extension(Graphics graphics)
    {
        public void FillRectangle(
            float x,
            float y,
            float width,
            float height,
            Color? color = null,
            Camera? camera = null
        )
        {
            graphics.FillRectangle(new Vector2(x, y), new Vector2(width, height), color, camera);
        }

        public void FillRectangle(in Box box, Color? color = null, Camera? camera = null)
        {
            graphics.FillRectangle(box.Position, box.Size, color, camera);
        }

        public void FillRectangle(Vector2 position, Vector2 size, Color? color = null, Camera? camera = null)
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            if (
                colorValue == Color.Transparent
                || (graphics.Culling() && !graphics.IsBoxInBounds(position, size, camera))
            )
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawRectangleRec(new Raylib_cs.Rectangle(position, size), colorValue.RColor);
            graphics.EndDrawing();
        }

        public void StrokeRectangle(
            float x,
            float y,
            float width,
            float height,
            Color? color = null,
            float? strokeWidth = null,
            Camera? camera = null
        )
        {
            graphics.StrokeRectangle(new Vector2(x, y), new Vector2(width, height), color, strokeWidth, camera);
        }

        public void StrokeRectangle(in Box box, Color? color = null, float? strokeWidth = null, Camera? camera = null)
        {
            graphics.StrokeRectangle(box.Position, box.Size, color, strokeWidth, camera);
        }

        public void StrokeRectangle(
            Vector2 position,
            Vector2 size,
            Color? color = null,
            float? strokeWidth = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
            var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
            if (
                colorValue == Color.Transparent
                || strokeWidthValue <= 0
                || (graphics.Culling() && !graphics.IsBoxInBounds(position, size, camera))
            )
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawRectangleLinesEx(new Raylib_cs.Rectangle(position, size), strokeWidthValue, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void FillRoundedRectangle(
            float x,
            float y,
            float width,
            float height,
            Color? color = null,
            float? radius = null,
            int segments = 0,
            Camera? camera = null
        )
        {
            graphics.FillRoundedRectangle(
                new Vector2(x, y),
                new Vector2(width, height),
                color,
                radius,
                segments,
                camera
            );
        }

        public void FillRoundedRectangle(
            in Box box,
            Color? color = null,
            float? radius = null,
            int segments = 0,
            Camera? camera = null
        )
        {
            graphics.FillRoundedRectangle(box.Position, box.Size, color, radius, segments, camera);
        }

        public void FillRoundedRectangle(
            Vector2 position,
            Vector2 size,
            Color? color = null,
            float? radius = null,
            int segments = 0,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var radiusValue = radius ?? Drawing.DefaultRadius.Or(1);
            if (
                colorValue == Color.Transparent
                || (graphics.Culling() && !graphics.IsBoxInBounds(position, size, camera))
            )
                return;
            var minSize = size.Abs().Min();
            segments = Drawing.CalculateSegments(minSize, 0, 90, segments);
            graphics.BeginDrawing(camera);
            Raylib.DrawRectangleRounded(
                new Raylib_cs.Rectangle(position, size),
                radiusValue <= 0 ? 0 : radiusValue / minSize,
                segments,
                colorValue.RColor
            );
            graphics.EndDrawing();
        }

        public void StrokeRoundedRectangle(
            float x,
            float y,
            float width,
            float height,
            Color? color = null,
            float? radius = null,
            float? strokeWidth = null,
            int segments = 0,
            Camera? camera = null
        )
        {
            graphics.StrokeRoundedRectangle(
                new Vector2(x, y),
                new Vector2(width, height),
                color,
                radius,
                strokeWidth,
                segments,
                camera
            );
        }

        public void StrokeRoundedRectangle(
            in Box box,
            Color? color = null,
            float? radius = null,
            float? strokeWidth = null,
            int segments = 0,
            Camera? camera = null
        )
        {
            graphics.StrokeRoundedRectangle(box.Position, box.Size, color, radius, strokeWidth, segments, camera);
        }

        public void StrokeRoundedRectangle(
            Vector2 position,
            Vector2 size,
            Color? color = null,
            float? radius = null,
            float? strokeWidth = null,
            int segments = 0,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
            var radiusValue = radius ?? Drawing.DefaultRadius.Or(1);
            var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
            if (
                colorValue == Color.Transparent
                || strokeWidthValue <= 0
                || (graphics.Culling() && !graphics.IsBoxInBounds(position, size, camera, strokeWidthValue))
            )
                return;
            position += strokeWidthValue * 0.5f;
            size -= strokeWidthValue;
            var minSize = size.Abs().Min();
            segments = Drawing.CalculateSegments(minSize, 0, 90, segments);
            radiusValue = radiusValue <= 0 ? 0 : radiusValue / minSize;
            graphics.BeginDrawing(camera);
            if (strokeWidthValue > 1f)
                Raylib.DrawRectangleRoundedLinesEx(
                    new Raylib_cs.Rectangle(position, size),
                    radiusValue,
                    segments,
                    strokeWidthValue,
                    colorValue.RColor
                );
            else
                Raylib.DrawRectangleRoundedLinesExShapes(
                    new Raylib_cs.Rectangle(position, size),
                    radiusValue,
                    segments,
                    strokeWidthValue,
                    colorValue.RColor
                );
            graphics.EndDrawing();
        }

        public void DrawRectangle(Rectangle rectangle)
        {
            graphics.DrawRectangle(new Transform(), rectangle);
        }

        public void DrawRectangle(float x, float y, float width, float height, Rectangle rectangle)
        {
            graphics.DrawRectangle(new Vector2(x, y), new Vector2(width, height), rectangle);
        }

        public void DrawRectangle(Vector2 position, Vector2 size, Rectangle rectangle)
        {
            graphics.DrawRectangle(new Transform(position + size * 0.5f, size), rectangle);
        }

        public void DrawRectangle(in Box box, Rectangle rectangle)
        {
            graphics.DrawRectangle(box.Position, box.Size, rectangle);
        }

        public void DrawRectangle(Transform transform, Rectangle rectangle)
        {
            using var _ = Drawable.EnterDrawing(ref transform, rectangle, graphics);
            var camera = rectangle.Camera.Get();
            var fill = rectangle.Fill;
            var stroke = rectangle.Stroke;
            var radius = rectangle.Radius;
            var segments = rectangle.Segments;
            var position = transform.Position;
            var scale = transform.Scale.Abs();
            var strokeWidth = rectangle.StrokeWidth.Clamp(0, scale.Min() * 0.5f);
            var order = rectangle.DrawOrder;
            graphics.Pivot(transform, true);
            if (radius > 0)
            {
                if (order == DrawOrder.StrokeThenFill)
                {
                    graphics.StrokeRoundedRectangle(position, scale, stroke, radius, strokeWidth, segments, camera);
                    graphics.FillRoundedRectangle(
                        position + strokeWidth,
                        scale - strokeWidth * 2,
                        fill,
                        radius,
                        segments,
                        camera
                    );
                }
                else
                {
                    graphics.FillRoundedRectangle(
                        position + strokeWidth,
                        scale - strokeWidth * 2,
                        fill,
                        radius,
                        segments,
                        camera
                    );
                    graphics.StrokeRoundedRectangle(position, scale, stroke, radius, strokeWidth, segments, camera);
                }
            }
            else
            {
                if (order == DrawOrder.StrokeThenFill)
                {
                    graphics.StrokeRectangle(position, scale, stroke, strokeWidth, camera);
                    graphics.FillRectangle(position + strokeWidth, scale - strokeWidth * 2, fill, camera);
                }
                else
                {
                    graphics.FillRectangle(position + strokeWidth, scale - strokeWidth * 2, fill, camera);
                    graphics.StrokeRectangle(position, scale, stroke, strokeWidth, camera);
                }
            }
        }
    }
}
