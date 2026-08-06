using Raylib_cs;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

[ValueWrapper(typeof(Drawable<ValueRectangleGradient>), "Drawable")]
public partial struct ValueRectangleGradient : IDrawable
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
        readonly get => TopLeftFill.Blend(BottomLeftFill).Blend(BottomRightFill).Blend(TopRightFill);
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
        readonly get => TopLeftFill.Blend(TopRightFill);
        set
        {
            TopLeftFill = value;
            TopRightFill = value;
        }
    }

    public Color BottomFill
    {
        readonly get => BottomLeftFill.Blend(BottomRightFill);
        set
        {
            BottomLeftFill = value;
            BottomRightFill = value;
        }
    }

    public Color LeftFill
    {
        readonly get => TopLeftFill.Blend(BottomLeftFill);
        set
        {
            TopLeftFill = value;
            BottomLeftFill = value;
        }
    }

    public Color RightFill
    {
        readonly get => TopRightFill.Blend(BottomRightFill);
        set
        {
            TopRightFill = value;
            BottomRightFill = value;
        }
    }

    public override readonly string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform), nameof(Fill)]), true);
    }

    public readonly void Draw(Transform transform, Graphics graphics)
    {
        graphics.DrawRectangleGradient(transform, this);
    }
}

[ValueWrapper(typeof(ValueRectangleGradient))]
public sealed partial class RectangleGradient : IDrawable, IFullCloneable
{
    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform), nameof(Fill)]), true);
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
                topLeftColorValue == Color.Transparent
                    && bottomLeftColorValue == Color.Transparent
                    && bottomRightColorValue == Color.Transparent
                    && topRightColorValue == Color.Transparent
                || graphics.Culling() && !graphics.IsBoxInBounds(position, size, camera)
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

        public void DrawRectangleGradient(in ValueRectangleGradient rectangle)
        {
            graphics.DrawRectangleGradient(new Transform(), rectangle);
        }

        public void DrawRectangleGradient(
            float x,
            float y,
            float width,
            float height,
            in ValueRectangleGradient rectangle
        )
        {
            graphics.DrawRectangleGradient(new Vector2(x, y), new Vector2(width, height), rectangle);
        }

        public void DrawRectangleGradient(Vector2 position, Vector2 size, in ValueRectangleGradient rectangle)
        {
            graphics.DrawRectangleGradient(new Transform(position + size * 0.5f, size), rectangle);
        }

        public void DrawRectangleGradient(in Box box, in ValueRectangleGradient rectangle)
        {
            graphics.DrawRectangleGradient(box.Position, box.Size, rectangle);
        }

        public void DrawRectangleGradient(Transform transform, in ValueRectangleGradient rectangle)
        {
            using var _ = Drawable<ValueRectangleGradient>.EnterDrawing(
                ref transform,
                rectangle.Drawable,
                rectangle,
                graphics
            );
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
        }
    }
}
