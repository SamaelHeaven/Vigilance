using System.Numerics;

namespace Vigilance.Math;

public static class MatrixExtensions
{
    extension(in Matrix3x2 matrix)
    {
        public Vector2 GetTranslation()
        {
            return new Vector2(matrix.M31, matrix.M32);
        }

        public Vector2 GetScale()
        {
            return new Vector2(
                MathF.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12),
                MathF.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22)
            );
        }

        public float GetRotation()
        {
            return MathF.Atan2(matrix.M21, matrix.M11).RadToDeg();
        }

        // ReSharper disable once InconsistentNaming
        public Matrix4x4 ToMatrix4x4()
        {
            return new Matrix4x4(
                matrix.M11,
                matrix.M12,
                0f,
                0f,
                matrix.M21,
                matrix.M22,
                0f,
                0f,
                0f,
                0f,
                1f,
                0f,
                matrix.M31,
                matrix.M32,
                0f,
                1f
            );
        }
    }
}
