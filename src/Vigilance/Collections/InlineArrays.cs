using System.Runtime.CompilerServices;

namespace Vigilance.Collections;

[InlineArray(128)]
public struct InlineArray128<T>
{
    private T _element0;
}

[InlineArray(256)]
public struct InlineArray256<T>
{
    private T _element0;
}
