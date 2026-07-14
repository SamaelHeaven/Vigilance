namespace Vigilance.FlexLayout;

// MeasureFunc describes function for measuring
public delegate Size MeasureFunc<TStorage>(
    Node<TStorage> node,
    float width,
    MeasureMode widthMode,
    float height,
    MeasureMode heightMode
)
    where TStorage : IList<Node<TStorage>>;

// BaselineFunc describes function for baseline
public delegate float BaselineFunc<TStorage>(Node<TStorage> node, float width, float height)
    where TStorage : IList<Node<TStorage>>;

public sealed class Size
{
    public float Height;
    public float Width;

    public Size(float w, float h)
    {
        Width = w;
        Height = h;
    }
}

public sealed class Value
{
    public float Number;
    public Unit Unit;

    public Value(float v, Unit u)
    {
        Number = v;
        Unit = u;
    }

    public static Value UndefinedValue => new(float.NaN, Unit.Undefined);

    public static void CopyValue(Value[] dest, Value[] src)
    {
        for (var i = 0; i < src.Length; i++)
        {
            dest[i].Number = src[i].Number;
            dest[i].Unit = src[i].Unit;
        }
    }
}

public static partial class Flex
{
    public static bool FloatsEqual(float a, float b)
    {
        if (FloatIsUndefined(a))
            return FloatIsUndefined(b);
        return System.Math.Abs(a - b) < 0.0001f;
    }

    public static float RoundValueToPixelGrid(float value, float pointScaleFactor, bool forceCeil, bool forceFloor)
    {
        var scaledValue = value * pointScaleFactor;
        var fractial = scaledValue % 1f;
        if (FloatsEqual(fractial, 0))
        {
            scaledValue -= fractial;
        }
        else if (FloatsEqual(fractial, 1) || forceCeil)
        {
            scaledValue = scaledValue - fractial + 1;
        }
        else if (forceFloor)
        {
            scaledValue -= fractial;
        }
        else
        {
            float f = 0;
            if (fractial >= 0.5f)
                f = 1.0f;
            scaledValue = scaledValue - fractial + f;
        }

        return scaledValue / pointScaleFactor;
    }

    public static void NodeCopyStyle<TStorage>(Node<TStorage> dstNode, Node<TStorage> srcNode)
        where TStorage : IList<Node<TStorage>>
    {
        if (!StyleEq(dstNode.NodeStyle, srcNode.NodeStyle))
        {
            Style.Copy(dstNode.NodeStyle, srcNode.NodeStyle);
            NodeMarkDirtyInternal(dstNode);
        }
    }

    public static void CalculateLayout<TStorage>(
        Node<TStorage> node,
        float parentWidth,
        float parentHeight,
        Direction parentDirection
    )
        where TStorage : IList<Node<TStorage>>
    {
        CurrentGenerationCount++;

        ResolveDimensions(node);

        CalcStartWidth(node, parentWidth, out var width, out var widthMeasureMode);
        CalcStartHeight(node, parentWidth, parentHeight, out var height, out var heightMeasureMode);

        if (
            LayoutNodeInternal(
                node,
                width,
                height,
                parentDirection,
                widthMeasureMode,
                heightMeasureMode,
                parentWidth,
                parentHeight,
                true
            )
        )
        {
            NodeSetPosition(node, node.NodeLayout.Direction, parentWidth, parentHeight, parentWidth);
            RoundToPixelGrid(node, 1, 0, 0);
        }
    }
}
