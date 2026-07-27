using Raylib_cs;

namespace Vigilance.Core;

public static class RaylibExtensions
{
    extension(Raylib)
    {
        public static void DrawRectangleRoundedLinesExShapes(
            Raylib_cs.Rectangle rec,
            float roundness,
            int segments,
            float lineThick,
            Raylib_cs.Color color
        )
        {
            if (lineThick < 0)
                lineThick = 0;

            switch (roundness)
            {
                // Not a rounded rectangle
                case <= 0.0f:
                    Raylib.DrawRectangleLinesEx(
                        new Raylib_cs.Rectangle(
                            rec.X - lineThick,
                            rec.Y - lineThick,
                            rec.Width + 2 * lineThick,
                            rec.Height + 2 * lineThick
                        ),
                        lineThick,
                        color
                    );
                    return;
                case >= 1.0f:
                    roundness = 1.0f;
                    break;
            }

            // Calculate corner radius
            var radius = rec.Width > rec.Height ? rec.Height * roundness / 2 : rec.Width * roundness / 2;
            if (radius <= 0.0f)
                return;

            // Calculate number of segments to use for the corners
            if (segments < 4)
            {
                // Calculate the maximum angle between segments based on the error rate (usually 0.5f)
                const float smoothCircleErrorRate = 0.5f;
                var th = MathF.Acos(2 * MathF.Pow(1 - smoothCircleErrorRate / radius, 2) - 1);
                segments = (int)(MathF.Ceiling(2 * MathF.PI / th) / 2.0f);
                if (segments <= 0)
                    segments = 4;
            }

            var stepLength = 90.0f / segments;
            var outerRadius = radius + lineThick;

            /*
            Quick sketch to make sense of all of this,
            marks the 16 + 4(corner centers P16-19) points used

                   P0 ================== P1
                  // P8                P9 \\
                 //                        \\
             P7 // P15                  P10 \\ P2
               ||   *P16             P17*    ||
               ||                            ||
               || P14                   P11  ||
             P6 \\  *P19             P18*   // P3
                 \\                        //
                  \\ P13              P12 //
                   P5 ================== P4
            */
            ReadOnlySpan<Vector2> point =
            [
                new(rec.X + radius + 0.5f, rec.Y - lineThick + 0.5f),
                new(rec.X + rec.Width - radius - 0.5f, rec.Y - lineThick + 0.5f),
                new(rec.X + rec.Width + lineThick - 0.5f, rec.Y + radius + 0.5f), // P0, P1, P2
                new(rec.X + rec.Width + lineThick - 0.5f, rec.Y + rec.Height - radius - 0.5f),
                new(rec.X + rec.Width - radius - 0.5f, rec.Y + rec.Height + lineThick - 0.5f), // P3, P4
                new(rec.X + radius + 0.5f, rec.Y + rec.Height + lineThick - 0.5f),
                new(rec.X - lineThick + 0.5f, rec.Y + rec.Height - radius - 0.5f),
                new(rec.X - lineThick + 0.5f, rec.Y + radius + 0.5f), // P5, P6, P7
                new(rec.X + radius + 0.5f, rec.Y + 0.5f),
                new(rec.X + rec.Width - radius - 0.5f, rec.Y + 0.5f), // P8, P9
                new(rec.X + rec.Width - 0.5f, rec.Y + radius + 0.5f),
                new(rec.X + rec.Width - 0.5f, rec.Y + rec.Height - radius - 0.5f), // P10, P11
                new(rec.X + rec.Width - radius - 0.5f, rec.Y + rec.Height - 0.5f),
                new(rec.X + radius + 0.5f, rec.Y + rec.Height - 0.5f), // P12, P13
                new(rec.X + 0.5f, rec.Y + rec.Height - radius - 0.5f),
                new(rec.X + 0.5f, rec.Y + radius + 0.5f), // P14, P15
            ];

            ReadOnlySpan<Vector2> centers =
            [
                new(rec.X + radius + 0.5f, rec.Y + radius + 0.5f),
                new(rec.X + rec.Width - radius - 0.5f, rec.Y + radius + 0.5f), // P16, P17
                new(rec.X + rec.Width - radius - 0.5f, rec.Y + rec.Height - radius - 0.5f),
                new(rec.X + radius + 0.5f, rec.Y + rec.Height - radius - 0.5f), // P18, P19
            ];

            ReadOnlySpan<float> angles = [180.0f, 270.0f, 0.0f, 90.0f];

            var texShapes = Raylib.GetShapesTexture();
            Rlgl.SetTexture(Raylib.GetShapesTexture().Id);
            var shapeRect = Raylib.GetShapesTextureRectangle();

            Rlgl.Begin((int)DrawMode.Quads);

            // Draw all the 4 corners first: Upper Left Corner, Upper Right Corner, Lower Right Corner, Lower Left Corner
            for (var k = 0; k < 4; ++k) // Hope the compiler is smart enough to unroll this loop
            {
                var angle = angles[k];
                var center = centers[k];
                for (var i = 0; i < segments; i++)
                {
                    Rlgl.Color4ub(color.R, color.G, color.B, color.A);

                    Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
                    Rlgl.Vertex2f(
                        center.X + MathF.Cos(MathF.PI / 180 * angle) * radius,
                        center.Y + MathF.Sin(MathF.PI / 180 * angle) * radius
                    );

                    Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
                    Rlgl.Vertex2f(
                        center.X + MathF.Cos(MathF.PI / 180 * (angle + stepLength)) * radius,
                        center.Y + MathF.Sin(MathF.PI / 180 * (angle + stepLength)) * radius
                    );

                    Rlgl.TexCoord2f(
                        (shapeRect.X + shapeRect.Width) / texShapes.Width,
                        (shapeRect.Y + shapeRect.Height) / texShapes.Height
                    );
                    Rlgl.Vertex2f(
                        center.X + MathF.Cos(MathF.PI / 180 * (angle + stepLength)) * outerRadius,
                        center.Y + MathF.Sin(MathF.PI / 180 * (angle + stepLength)) * outerRadius
                    );

                    Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
                    Rlgl.Vertex2f(
                        center.X + MathF.Cos(MathF.PI / 180 * angle) * outerRadius,
                        center.Y + MathF.Sin(MathF.PI / 180 * angle) * outerRadius
                    );

                    angle += stepLength;
                }
            }

            // Upper rectangle
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);
            Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
            Rlgl.Vertex2f(point[0].X, point[0].Y);
            Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
            Rlgl.Vertex2f(point[8].X, point[8].Y);
            Rlgl.TexCoord2f(
                (shapeRect.X + shapeRect.Width) / texShapes.Width,
                (shapeRect.Y + shapeRect.Height) / texShapes.Height
            );
            Rlgl.Vertex2f(point[9].X, point[9].Y);
            Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
            Rlgl.Vertex2f(point[1].X, point[1].Y);

            // Right rectangle
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);
            Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
            Rlgl.Vertex2f(point[2].X, point[2].Y);
            Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
            Rlgl.Vertex2f(point[10].X, point[10].Y);
            Rlgl.TexCoord2f(
                (shapeRect.X + shapeRect.Width) / texShapes.Width,
                (shapeRect.Y + shapeRect.Height) / texShapes.Height
            );
            Rlgl.Vertex2f(point[11].X, point[11].Y);
            Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
            Rlgl.Vertex2f(point[3].X, point[3].Y);

            // Lower rectangle
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);
            Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
            Rlgl.Vertex2f(point[13].X, point[13].Y);
            Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
            Rlgl.Vertex2f(point[5].X, point[5].Y);
            Rlgl.TexCoord2f(
                (shapeRect.X + shapeRect.Width) / texShapes.Width,
                (shapeRect.Y + shapeRect.Height) / texShapes.Height
            );
            Rlgl.Vertex2f(point[4].X, point[4].Y);
            Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
            Rlgl.Vertex2f(point[12].X, point[12].Y);

            // Left rectangle
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);
            Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
            Rlgl.Vertex2f(point[15].X, point[15].Y);
            Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
            Rlgl.Vertex2f(point[7].X, point[7].Y);
            Rlgl.TexCoord2f(
                (shapeRect.X + shapeRect.Width) / texShapes.Width,
                (shapeRect.Y + shapeRect.Height) / texShapes.Height
            );
            Rlgl.Vertex2f(point[6].X, point[6].Y);
            Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
            Rlgl.Vertex2f(point[14].X, point[14].Y);

            Rlgl.End();
            Rlgl.SetTexture(0);

            Rlgl.Begin((int)DrawMode.Triangles);

            // Draw all the 4 corners first: Upper Left Corner, Upper Right Corner, Lower Right Corner, Lower Left Corner
            for (var k = 0; k < 4; ++k) // Hope the compiler is smart enough to unroll this loop
            {
                var angle = angles[k];
                var center = centers[k];

                for (var i = 0; i < segments; i++)
                {
                    Rlgl.Color4ub(color.R, color.G, color.B, color.A);

                    Rlgl.Vertex2f(
                        center.X + MathF.Cos(MathF.PI / 180 * angle) * radius,
                        center.Y + MathF.Sin(MathF.PI / 180 * angle) * radius
                    );
                    Rlgl.Vertex2f(
                        center.X + MathF.Cos(MathF.PI / 180 * (angle + stepLength)) * radius,
                        center.Y + MathF.Sin(MathF.PI / 180 * (angle + stepLength)) * radius
                    );
                    Rlgl.Vertex2f(
                        center.X + MathF.Cos(MathF.PI / 180 * angle) * outerRadius,
                        center.Y + MathF.Sin(MathF.PI / 180 * angle) * outerRadius
                    );

                    Rlgl.Vertex2f(
                        center.X + MathF.Cos(MathF.PI / 180 * (angle + stepLength)) * radius,
                        center.Y + MathF.Sin(MathF.PI / 180 * (angle + stepLength)) * radius
                    );
                    Rlgl.Vertex2f(
                        center.X + MathF.Cos(MathF.PI / 180 * (angle + stepLength)) * outerRadius,
                        center.Y + MathF.Sin(MathF.PI / 180 * (angle + stepLength)) * outerRadius
                    );
                    Rlgl.Vertex2f(
                        center.X + MathF.Cos(MathF.PI / 180 * angle) * outerRadius,
                        center.Y + MathF.Sin(MathF.PI / 180 * angle) * outerRadius
                    );

                    angle += stepLength;
                }
            }

            // Upper rectangle
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);
            Rlgl.Vertex2f(point[0].X, point[0].Y);
            Rlgl.Vertex2f(point[8].X, point[8].Y);
            Rlgl.Vertex2f(point[9].X, point[9].Y);
            Rlgl.Vertex2f(point[1].X, point[1].Y);
            Rlgl.Vertex2f(point[0].X, point[0].Y);
            Rlgl.Vertex2f(point[9].X, point[9].Y);

            // Right rectangle
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);
            Rlgl.Vertex2f(point[10].X, point[10].Y);
            Rlgl.Vertex2f(point[11].X, point[11].Y);
            Rlgl.Vertex2f(point[3].X, point[3].Y);
            Rlgl.Vertex2f(point[2].X, point[2].Y);
            Rlgl.Vertex2f(point[10].X, point[10].Y);
            Rlgl.Vertex2f(point[3].X, point[3].Y);

            // Lower rectangle
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);
            Rlgl.Vertex2f(point[13].X, point[13].Y);
            Rlgl.Vertex2f(point[5].X, point[5].Y);
            Rlgl.Vertex2f(point[4].X, point[4].Y);
            Rlgl.Vertex2f(point[12].X, point[12].Y);
            Rlgl.Vertex2f(point[13].X, point[13].Y);
            Rlgl.Vertex2f(point[4].X, point[4].Y);

            // Left rectangle
            Rlgl.Color4ub(color.R, color.G, color.B, color.A);
            Rlgl.Vertex2f(point[7].X, point[7].Y);
            Rlgl.Vertex2f(point[6].X, point[6].Y);
            Rlgl.Vertex2f(point[14].X, point[14].Y);
            Rlgl.Vertex2f(point[15].X, point[15].Y);
            Rlgl.Vertex2f(point[7].X, point[7].Y);
            Rlgl.Vertex2f(point[14].X, point[14].Y);
            Rlgl.End();
        }
    }
}
