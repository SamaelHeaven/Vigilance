namespace Vigilance.FlexLayout;

public partial class Node
{
    internal readonly List<Node> Children = [];
    internal readonly Flex.Layout NodeLayout = new();
    internal readonly Style NodeStyle = new();
    internal readonly Value[] ResolvedDimensions = [Flex.ValueUndefined, Flex.ValueUndefined];
    internal BaselineFunc? BaselineFunc;
    internal int LineIndex;
    internal MeasureFunc? MeasureFunc;
    internal Node? NextChild;
    internal NodeType NodeType = NodeType.Default;
    internal Node? Parent = null;
    public int ChildrenCount => Children.Count;
    public bool IsDirty { get; internal set; }
}
