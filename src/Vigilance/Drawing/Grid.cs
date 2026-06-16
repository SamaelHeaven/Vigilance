using Raylib_cs;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed class Grid : Drawable<Grid>, IFullCloneable
{
    public Grid() { }

    public Grid(Color color)
    {
        Color = color;
    }

    public Grid(float cellSize)
    {
        CellSize = cellSize;
    }

    public Grid(float cellSize, Color color)
        : this(cellSize)
    {
        Color = color;
    }

    public float CellSize { get; set; }
    public float Thick { get; set; } = Drawing.DefaultStrokeWidth == 0 ? 1 : Drawing.DefaultStrokeWidth;
    public Color Color { get; set; } = Drawing.DefaultFill;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }

    public override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawGrid(transform, this);
    }
}

public static class GridExtensions
{
    extension(Graphics graphics)
    {
        public void DrawGrid(
            float x,
            float y,
            float width,
            float height,
            float cellSize,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            graphics.DrawGrid(new Vector2(x, y), new Vector2(width, height), cellSize, color, thick, camera);
        }

        public void DrawGrid(
            in Box box,
            float cellSize,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            graphics.DrawGrid(box.Position, box.Size, cellSize, color, thick, camera);
        }

        public void DrawGrid(
            Vector2 position,
            Vector2 size,
            float cellSize,
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
                || (graphics.Culling() && !graphics.IsBoxInBounds(position, size, camera, thickValue * 0.5f))
            )
                return;
            cellSize = cellSize.Max(1);
            graphics.BeginDrawing(camera);
            for (var x = position.X; x <= position.X + size.X; x += cellSize)
                Raylib.DrawLineEx(
                    new Vector2(x, position.Y),
                    new Vector2(x, position.Y + size.Y),
                    thickValue,
                    colorValue.RColor
                );
            for (var y = position.Y; y <= position.Y + size.Y; y += cellSize)
                Raylib.DrawLineEx(
                    new Vector2(position.X, y),
                    new Vector2(position.X + size.X, y),
                    thickValue,
                    colorValue.RColor
                );
            graphics.EndDrawing();
        }

        public void DrawGrid(Grid grid)
        {
            graphics.DrawGrid(new Transform(), grid);
        }

        public void DrawGrid(float x, float y, float width, float height, Grid grid)
        {
            graphics.DrawGrid(new Vector2(x, y), new Vector2(width, height), grid);
        }

        public void DrawGrid(Vector2 position, Vector2 size, Grid grid)
        {
            graphics.DrawGrid(new Transform(position + size * 0.5f, size), grid);
        }

        public void DrawGrid(in Box box, Grid grid)
        {
            graphics.DrawGrid(box.Position, box.Size, grid);
        }

        public void DrawGrid(Transform transform, Grid grid)
        {
            grid.OnBeginDrawing?.Invoke(transform, grid, graphics);
            transform += grid.Transform;
            var camera = grid.Camera.Get();
            var color = grid.Color;
            var cellSize = grid.CellSize;
            var thick = grid.Thick;
            var position = transform.Position;
            var scale = transform.Scale.Abs();
            graphics.PushMatrix();
            graphics.Pivot(transform, true);
            graphics.DrawGrid(position, scale, cellSize, color, thick, camera);
            graphics.PopMatrix();
            grid.OnEndDrawing?.Invoke(transform, grid, graphics);
        }
    }
}
