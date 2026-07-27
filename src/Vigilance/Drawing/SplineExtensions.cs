using System.Runtime.CompilerServices;
using Raylib_cs;

namespace Vigilance.Drawing;

public static class SplineExtensions
{
    extension(Graphics graphics)
    {
        public void DrawSplineLinear(
            IEnumerable<Vector2> points,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            graphics.DrawSplineLinear(points.AsSpan(), color, thick, camera);
        }

        [OverloadResolutionPriority(1)]
        public unsafe void DrawSplineLinear(
            in ReadOnlySpan<Vector2> points,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (colorValue == Color.Transparent || points.Length < 2 || thickValue <= 0)
                return;
            graphics.BeginDrawing(camera);
            fixed (Vector2* pointsBuffer = points)
            {
                Raylib.DrawSplineLinear(
                    (System.Numerics.Vector2*)pointsBuffer,
                    points.Length,
                    thickValue,
                    colorValue.RColor
                );
            }

            graphics.EndDrawing();
        }

        public void DrawSplineBasis(
            IEnumerable<Vector2> points,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            graphics.DrawSplineBasis(points.AsSpan(), color, thick, camera);
        }

        [OverloadResolutionPriority(1)]
        public unsafe void DrawSplineBasis(
            in ReadOnlySpan<Vector2> points,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (colorValue == Color.Transparent || points.Length < 4 || thickValue <= 0)
                return;
            graphics.BeginDrawing(camera);
            fixed (Vector2* pointsBuffer = points)
            {
                Raylib.DrawSplineBasis(
                    (System.Numerics.Vector2*)pointsBuffer,
                    points.Length,
                    thickValue,
                    colorValue.RColor
                );
            }

            graphics.EndDrawing();
        }

        public void DrawSplineCatmullRom(
            IEnumerable<Vector2> points,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            graphics.DrawSplineCatmullRom(points.AsSpan(), color, thick, camera);
        }

        [OverloadResolutionPriority(1)]
        public unsafe void DrawSplineCatmullRom(
            in ReadOnlySpan<Vector2> points,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (colorValue == Color.Transparent || points.Length < 4 || thickValue <= 0)
                return;
            graphics.BeginDrawing(camera);
            fixed (Vector2* pointsBuffer = points)
            {
                Raylib.DrawSplineCatmullRom(
                    (System.Numerics.Vector2*)pointsBuffer,
                    points.Length,
                    thickValue,
                    colorValue.RColor
                );
            }

            graphics.EndDrawing();
        }

        public void DrawSplineBezierQuadratic(
            IEnumerable<Vector2> points,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            graphics.DrawSplineBezierQuadratic(points.AsSpan(), color, thick, camera);
        }

        [OverloadResolutionPriority(1)]
        public unsafe void DrawSplineBezierQuadratic(
            in ReadOnlySpan<Vector2> points,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (colorValue == Color.Transparent || points.Length < 3 || thickValue <= 0)
                return;
            graphics.BeginDrawing(camera);
            fixed (Vector2* pointsBuffer = points)
            {
                Raylib.DrawSplineBezierQuadratic(
                    (System.Numerics.Vector2*)pointsBuffer,
                    points.Length,
                    thickValue,
                    colorValue.RColor
                );
            }

            graphics.EndDrawing();
        }

        public void DrawSplineBezierCubic(
            IEnumerable<Vector2> points,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            graphics.DrawSplineBezierCubic(points.AsSpan(), color, thick, camera);
        }

        [OverloadResolutionPriority(1)]
        public unsafe void DrawSplineBezierCubic(
            in ReadOnlySpan<Vector2> points,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (colorValue == Color.Transparent || points.Length < 4 || thickValue <= 0)
                return;
            graphics.BeginDrawing(camera);
            fixed (Vector2* pointsBuffer = points)
            {
                Raylib.DrawSplineBezierCubic(
                    (System.Numerics.Vector2*)pointsBuffer,
                    points.Length,
                    thickValue,
                    colorValue.RColor
                );
            }

            graphics.EndDrawing();
        }

        public void DrawSplineSegmentLinear(
            Vector2 p1,
            Vector2 p2,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (colorValue == Color.Transparent || thickValue <= 0)
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawSplineSegmentLinear(p1, p2, thickValue, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void DrawSplineSegmentBasis(
            Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            Vector2 p4,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (colorValue == Color.Transparent || thickValue <= 0)
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawSplineSegmentBasis(p1, p2, p3, p4, thickValue, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void DrawSplineSegmentCatmullRom(
            Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            Vector2 p4,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (colorValue == Color.Transparent || thickValue <= 0)
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawSplineSegmentCatmullRom(p1, p2, p3, p4, thickValue, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void DrawSplineSegmentCatmullRom(
            Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (colorValue == Color.Transparent || thickValue <= 0)
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawSplineSegmentBezierQuadratic(p1, p2, p3, thickValue, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void DrawSplineSegmentBezierCubic(
            Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            Vector2 p4,
            Color? color = null,
            float? thick = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var thickValue = thick ?? Drawing.DefaultStrokeWidth.Or(1);
            if (colorValue == Color.Transparent || thickValue <= 0)
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawSplineSegmentBezierCubic(p1, p2, p3, p4, thickValue, colorValue.RColor);
            graphics.EndDrawing();
        }
    }
}
