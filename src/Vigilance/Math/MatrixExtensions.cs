using System.Runtime.CompilerServices;

namespace Vigilance.Math;

public static class MatrixExtensions
{
    extension(in Matrix3x2 matrix)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetTranslation()
        {
            return new Vector2(matrix.M31, matrix.M32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetScale()
        {
            return new Vector2(
                MathF.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12),
                MathF.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetRotation()
        {
            return MathF.Atan2(matrix.M21, matrix.M11).RadToDeg();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once InconsistentNaming
        public Matrix4x4 ToMatrix4x4()
        {
            return new Matrix4x4(matrix);
        }
    }
}
