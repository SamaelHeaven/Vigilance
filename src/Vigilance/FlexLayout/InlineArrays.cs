using System.Runtime.CompilerServices;

namespace Vigilance.FlexLayout;

[InlineArray(2)]
internal struct ValueBuffer2
{
    private Value _element0;
}

[InlineArray(Constant.EdgeCount)]
internal struct ValueBufferEdge
{
    private Value _element0;
}

[InlineArray(2)]
internal struct FloatBuffer2
{
    private float _element0;
}

[InlineArray(4)]
internal struct FloatBuffer4
{
    private float _element0;
}

[InlineArray(6)]
internal struct FloatBuffer6
{
    private float _element0;
}

[InlineArray(Constant.MaxCachedResultCount)]
internal struct CachedMeasurementBuffer
{
    private Flex.CachedMeasurement _element0;
}
