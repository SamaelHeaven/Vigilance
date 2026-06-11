using System.Numerics;

namespace Vigilance.Math;

public static class MatrixExtensions
{
    extension(in Matrix3x2 matrix)
    {
        // ReSharper disable once InconsistentNaming
        public Matrix4x4 ToMatrix4x4()
        {
            return new Matrix4x4(
                matrix.M11, matrix.M12, 0f, 0f,
                matrix.M21, matrix.M22, 0f, 0f,
                0f, 0f, 1f, 0f,
                matrix.M31, matrix.M32, 0f, 1f
            );
        }
    }
}
