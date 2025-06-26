using Raylib_cs.BleedingEdge;

namespace Vigilance.Drawing;

public enum ImageFormat
{
    UncompressedGrayscale = PixelFormat.UncompressedGrayscale,
    UncompressedGrayAlpha = PixelFormat.UncompressedGrayAlpha,
    UncompressedR5G6B5 = PixelFormat.UncompressedR5G6B5,
    UncompressedR8G8B8 = PixelFormat.UncompressedR8G8B8,
    UncompressedR5G5B5A1 = PixelFormat.UncompressedR5G5B5A1,
    UncompressedR4G4B4A4 = PixelFormat.UncompressedR4G4B4A4,
    UncompressedR8G8B8A8 = PixelFormat.UncompressedR8G8B8A8,
    UncompressedR32 = PixelFormat.UncompressedR32,
    UncompressedR32G32B32 = PixelFormat.UncompressedR32G32B32,
    UncompressedR32G32B32A32 = PixelFormat.UncompressedR32G32B32A32,
    UncompressedR16 = PixelFormat.UncompressedR16,
    UncompressedR16G16B16 = PixelFormat.UncompressedR16G16B16,
    UncompressedR16G16B16A16 = PixelFormat.UncompressedR16G16B16A16,
    CompressedDxt1Rgb = PixelFormat.CompressedDxt1Rgb,
    CompressedDxt1Rgba = PixelFormat.CompressedDxt1Rgba,
    CompressedDxt3Rgba = PixelFormat.CompressedDxt3Rgba,
    CompressedDxt5Rgba = PixelFormat.CompressedDxt5Rgba,
    CompressedEtc1Rgb = PixelFormat.CompressedEtc1Rgb,
    CompressedEtc2Rgb = PixelFormat.CompressedEtc2Rgb,
    CompressedEtc2EacRgba = PixelFormat.CompressedEtc2EacRgba,
    CompressedPvrtRgb = PixelFormat.CompressedPvrtRgb,
    CompressedPvrtRgba = PixelFormat.CompressedPvrtRgba,
    CompressedAstc4X4Rgba = PixelFormat.CompressedAstc4X4Rgba,
    CompressedAstc8X8Rgba = PixelFormat.CompressedAstc8X8Rgba,
}
