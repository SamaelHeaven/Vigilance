using Raylib_cs;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed class RectangleGradient : Drawable<RectangleGradient>, IFullCloneable
{
    public Color TopLeftFill { get; set; } = Drawing.DefaultFill;
    public Color BottomLeftFill { get; set; } = Drawing.DefaultFill;
    public Color BottomRightFill { get; set; } = Drawing.DefaultFill;
    public Color TopRightFill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public Color Fill
    {
        get => TopLeftFill.Blend(BottomLeftFill).Blend(BottomRightFill).Blend(TopRightFill);
        set
        {
            TopLeftFill = value;
            BottomLeftFill = value;
            BottomRightFill = value;
            TopRightFill = value;
        }
    }

    public Color TopFill
    {
        get => TopLeftFill.Blend(TopRightFill);
        set
        {
            TopLeftFill = value;
            TopRightFill = value;
        }
    }

    public Color BottomFill
    {
        get => BottomLeftFill.Blend(BottomRightFill);
        set
        {
            BottomLeftFill = value;
            BottomRightFill = value;
        }
    }

    public Color LeftFill
    {
        get => TopLeftFill.Blend(BottomLeftFill);
        set
        {
            TopLeftFill = value;
            BottomLeftFill = value;
        }
    }

    public Color RightFill
    {
        get => TopRightFill.Blend(BottomRightFill);
        set
        {
            TopRightFill = value;
            BottomRightFill = value;
        }
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform), nameof(Fill)), true);
    }

    protected override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawRectangleGradient(transform, this);
    }
}

public static class RectangleGradientExtensions
{
    extension(Graphics graphics)
    {
        public void FillRectangleGradient(
            float x,
            float y,
            float width,
            float height,
            Color? topLeftColor = null,
            Color? bottomLeftColor = null,
            Color? bottomRightColor = null,
            Color? topRightColor = null,
            Camera? camera = null
        )
        {
            graphics.FillRectangleGradient(
                new Vector2(x, y),
                new Vector2(width, height),
                topLeftColor,
                bottomLeftColor,
                bottomRightColor,
                topRightColor,
                camera
            );
        }

        public void FillRectangleGradient(
            in Box box,
            Color? topLeftColor = null,
            Color? bottomLeftColor = null,
            Color? bottomRightColor = null,
            Color? topRightColor = null,
            Camera? camera = null
        )
        {
            graphics.FillRectangleGradient(
                box.Position,
                box.Size,
                topLeftColor,
                bottomLeftColor,
                bottomRightColor,
                topRightColor,
                camera
            );
        }

        public void FillRectangleGradient(
            Vector2 position,
            Vector2 size,
            Color? topLeftColor = null,
            Color? bottomLeftColor = null,
            Color? bottomRightColor = null,
            Color? topRightColor = null,
            Camera? camera = null
        )
        {
            var topLeftColorValue = topLeftColor ?? Drawing.DefaultFill.Or(Color.White);
            var bottomLeftColorValue = bottomLeftColor ?? Drawing.DefaultFill.Or(Color.White);
            var bottomRightColorValue = bottomRightColor ?? Drawing.DefaultFill.Or(Color.White);
            var topRightColorValue = topRightColor ?? Drawing.DefaultFill.Or(Color.White);
            if (
                (
                    topLeftColorValue == Color.Transparent
                    && bottomLeftColorValue == Color.Transparent
                    && bottomRightColorValue == Color.Transparent
                    && topRightColorValue == Color.Transparent
                ) || (graphics.Culling() && !graphics.IsBoxInBounds(position, size, camera))
            )
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawRectangleGradientEx(
                new Raylib_cs.Rectangle(position, size),
                topLeftColorValue.RColor,
                bottomLeftColorValue.RColor,
                bottomRightColorValue.RColor,
                topRightColorValue.RColor
            );
            graphics.EndDrawing();
        }

        public void DrawRectangleGradient(RectangleGradient rectangle)
        {
            graphics.DrawRectangleGradient(new Transform(), rectangle);
        }

        public void DrawRectangleGradient(float x, float y, float width, float height, RectangleGradient rectangle)
        {
            graphics.DrawRectangleGradient(new Vector2(x, y), new Vector2(width, height), rectangle);
        }

        public void DrawRectangleGradient(Vector2 position, Vector2 size, RectangleGradient rectangle)
        {
            graphics.DrawRectangleGradient(new Transform(position + size * 0.5f, size), rectangle);
        }

        public void DrawRectangleGradient(in Box box, RectangleGradient rectangle)
        {
            graphics.DrawRectangleGradient(box.Position, box.Size, rectangle);
        }

        public void DrawRectangleGradient(Transform transform, RectangleGradient rectangle)
        {
            rectangle.OnBeginDrawing?.Invoke(transform, rectangle, graphics);
            transform += rectangle.Transform;
            var camera = rectangle.Camera.Get();
            var topLeftFill = rectangle.TopLeftFill;
            var bottomLeftFill = rectangle.BottomLeftFill;
            var bottomRightFill = rectangle.BottomRightFill;
            var topRightFill = rectangle.TopRightFill;
            var stroke = rectangle.Stroke;
            var position = transform.Position;
            var scale = transform.Scale.Abs();
            var strokeWidth = rectangle.StrokeWidth.Clamp(0, scale.Min() * 0.5f);
            var order = rectangle.DrawOrder;
            graphics.PushMatrix();
            graphics.Pivot(transform, true);
            if (order == DrawOrder.StrokeThenFill)
            {
                graphics.StrokeRectangle(position, scale, stroke, strokeWidth, camera);
                graphics.FillRectangleGradient(
                    position + strokeWidth,
                    scale - strokeWidth * 2,
                    topLeftFill,
                    bottomLeftFill,
                    bottomRightFill,
                    topRightFill,
                    camera
                );
            }
            else
            {
                graphics.FillRectangleGradient(
                    position + strokeWidth,
                    scale - strokeWidth * 2,
                    topLeftFill,
                    bottomLeftFill,
                    bottomRightFill,
                    topRightFill,
                    camera
                );
                graphics.StrokeRectangle(position, scale, stroke, strokeWidth, camera);
            }

            graphics.PopMatrix();
            rectangle.OnEndDrawing?.Invoke(transform, rectangle, graphics);
        }
    }
}
