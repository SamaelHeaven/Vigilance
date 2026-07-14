using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Vigilance.FlexLayout;

namespace Vigilance.Test;

[TestFixture]
public class TestUnit
{
    private sealed class TestNode : Node<TestNode.Children>
    {
        public TestNode()
            : base([]) { }

        public object? Context { get; set; }

        public sealed class Children : List<Node<Children>>;
    }

    private static void AssertFloatEqual(float expect, float real)
    {
        Assert.AreEqual(expect, real, 0.0001f);
        if (System.Math.Abs(expect - real) > 0.0001f)
            throw new Exception();
    }

    private static void AssertEqual(object? a, object? b)
    {
        Assert.AreEqual(a, b);
        if (a == null && b == null)
            return;

        if (a == null || b == null)
            throw new Exception();

        if (!a.Equals(b))
            throw new Exception();
    }

    private static void AssertTrue(bool value)
    {
        Assert.IsTrue(value);
        if (!value)
            throw new Exception();
    }

    private static void AssertFalse(bool value)
    {
        Assert.IsFalse(value);
        if (value)
            throw new Exception();
    }

    [Test]
    public void TestAbsoluteLayoutWidthHeightStartTop()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Start, 10);
        rootChild0.StyleSetPosition(Edge.Top, 10);
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutStartTopEndBottom()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Start, 10);
        rootChild0.StyleSetPosition(Edge.Top, 10);
        rootChild0.StyleSetPosition(Edge.End, 10);
        rootChild0.StyleSetPosition(Edge.Bottom, 10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutWidthHeightStartTopEndBottom()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Start, 10);
        rootChild0.StyleSetPosition(Edge.Top, 10);
        rootChild0.StyleSetPosition(Edge.End, 10);
        rootChild0.StyleSetPosition(Edge.Bottom, 10);
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestDoNotClampHeightOfAbsoluteNodeToHeightOfItsOverflowHiddenParent()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetOverflow(Overflow.Hidden);
        root.StyleSetWidth(50);
        root.StyleSetHeight(50);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Start, 0);
        rootChild0.StyleSetPosition(Edge.Top, 0);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(100);
        rootChild0Child0.StyleSetHeight(100);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(-50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutWithinBorder()
    {
        var root = new TestNode();
        root.StyleSetMargin(Edge.Left, 10);
        root.StyleSetMargin(Edge.Top, 10);
        root.StyleSetMargin(Edge.Right, 10);
        root.StyleSetMargin(Edge.Bottom, 10);
        root.StyleSetPadding(Edge.Left, 10);
        root.StyleSetPadding(Edge.Top, 10);
        root.StyleSetPadding(Edge.Right, 10);
        root.StyleSetPadding(Edge.Bottom, 10);
        root.StyleSetBorder(Edge.Left, 10);
        root.StyleSetBorder(Edge.Top, 10);
        root.StyleSetBorder(Edge.Right, 10);
        root.StyleSetBorder(Edge.Bottom, 10);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Left, 0);
        rootChild0.StyleSetPosition(Edge.Top, 0);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetPositionType(PositionType.Absolute);
        rootChild1.StyleSetPosition(Edge.Right, 0);
        rootChild1.StyleSetPosition(Edge.Bottom, 0);
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetPositionType(PositionType.Absolute);
        rootChild2.StyleSetPosition(Edge.Left, 0);
        rootChild2.StyleSetPosition(Edge.Top, 0);
        rootChild2.StyleSetMargin(Edge.Left, 10);
        rootChild2.StyleSetMargin(Edge.Top, 10);
        rootChild2.StyleSetMargin(Edge.Right, 10);
        rootChild2.StyleSetMargin(Edge.Bottom, 10);
        rootChild2.StyleSetWidth(50);
        rootChild2.StyleSetHeight(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetPositionType(PositionType.Absolute);
        rootChild3.StyleSetPosition(Edge.Right, 0);
        rootChild3.StyleSetPosition(Edge.Bottom, 0);
        rootChild3.StyleSetMargin(Edge.Left, 10);
        rootChild3.StyleSetMargin(Edge.Top, 10);
        rootChild3.StyleSetMargin(Edge.Right, 10);
        rootChild3.StyleSetMargin(Edge.Bottom, 10);
        rootChild3.StyleSetWidth(50);
        rootChild3.StyleSetHeight(50);
        root.InsertChild(rootChild3, 3);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(10, root.LayoutGetLeft());
        AssertFloatEqual(10, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(40, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(20, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(30, rootChild3.LayoutGetLeft());
        AssertFloatEqual(30, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(10, root.LayoutGetLeft());
        AssertFloatEqual(10, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(40, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(20, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(30, rootChild3.LayoutGetLeft());
        AssertFloatEqual(30, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutAlignItemsAndJustifyContentCenter()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetFlexGrow(1);
        root.StyleSetWidth(110);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(40);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(30, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(30, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutAlignItemsAndJustifyContentFlexEnd()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.End);
        root.StyleSetAlignItems(Align.End);
        root.StyleSetFlexGrow(1);
        root.StyleSetWidth(110);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(40);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(60, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(60, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutJustifyContentCenter()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetFlexGrow(1);
        root.StyleSetWidth(110);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(40);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(30, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(30, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutAlignItemsCenter()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetFlexGrow(1);
        root.StyleSetWidth(110);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(40);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutAlignItemsCenterOnChildOnly()
    {
        var root = new TestNode();
        root.StyleSetFlexGrow(1);
        root.StyleSetWidth(110);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAlignSelf(Align.Center);
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(40);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutAlignItemsAndJustifyContentCenterAndTopPosition()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetFlexGrow(1);
        root.StyleSetWidth(110);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Top, 10);
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(40);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutAlignItemsAndJustifyContentCenterAndBottomPosition()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetFlexGrow(1);
        root.StyleSetWidth(110);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Bottom, 10);
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(40);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(50, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(50, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsoluteLayoutAlignItemsAndJustifyContentCenterAndLeftPosition()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetFlexGrow(1);
        root.StyleSetWidth(110);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Left, 5);
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(40);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(5, rootChild0.LayoutGetLeft());
        AssertFloatEqual(30, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(5, rootChild0.LayoutGetLeft());
        AssertFloatEqual(30, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsolute_layout_align_items_and_justify_content_center_and_right_position()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetFlexGrow(1);
        root.StyleSetWidth(110);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Right, 5);
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(40);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(45, rootChild0.LayoutGetLeft());
        AssertFloatEqual(30, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(110, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(45, rootChild0.LayoutGetLeft());
        AssertFloatEqual(30, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestPosition_root_with_rtl_should_position_withoutdirection()
    {
        var root = new TestNode();
        root.StyleSetPosition(Edge.Left, 72);
        root.StyleSetWidth(52);
        root.StyleSetHeight(52);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(72, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(52, root.LayoutGetWidth());
        AssertFloatEqual(52, root.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(72, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(52, root.LayoutGetWidth());
        AssertFloatEqual(52, root.LayoutGetHeight());
    }

    [Test]
    public void TestAbsolute_layout_percentage_bottom_based_on_parent_height()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPositionPercent(Edge.Top, 50);
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetPositionType(PositionType.Absolute);
        rootChild1.StyleSetPositionPercent(Edge.Bottom, 50);
        rootChild1.StyleSetWidth(10);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetPositionType(PositionType.Absolute);
        rootChild2.StyleSetPositionPercent(Edge.Top, 10);
        rootChild2.StyleSetPositionPercent(Edge.Bottom, 10);
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(100, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(90, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(160, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0.LayoutGetLeft());
        AssertFloatEqual(100, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(90, rootChild1.LayoutGetLeft());
        AssertFloatEqual(90, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(90, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(160, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestAbsolute_layout_in_wrap_reverse_column_container()
    {
        var root = new TestNode();
        root.StyleSetFlexWrap(Wrap.WrapReverse);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetWidth(20);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(20, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(20, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsolute_layout_in_wrap_reverse_row_container()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetFlexWrap(Wrap.WrapReverse);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetWidth(20);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0.LayoutGetTop());
        AssertFloatEqual(20, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0.LayoutGetTop());
        AssertFloatEqual(20, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsolute_layout_in_wrap_reverse_column_container_flex_end()
    {
        var root = new TestNode();
        root.StyleSetFlexWrap(Wrap.WrapReverse);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAlignSelf(Align.End);
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetWidth(20);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(20, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(20, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAbsolute_layout_in_wrap_reverse_row_container_flex_end()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetFlexWrap(Wrap.WrapReverse);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAlignSelf(Align.End);
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetWidth(20);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(20, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(20, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAlignContentFlexStart()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(130);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        rootChild3.StyleSetHeight(10);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        rootChild4.StyleSetHeight(10);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(130, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(10, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        AssertFloatEqual(50, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(20, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(10, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(130, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(80, rootChild2.LayoutGetLeft());
        AssertFloatEqual(10, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        AssertFloatEqual(30, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(80, rootChild4.LayoutGetLeft());
        AssertFloatEqual(20, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(10, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_flex_start_without_height_on_children()
    {
        var root = new TestNode();
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        rootChild3.StyleSetHeight(10);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(10, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(0, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(20, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(0, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(50, rootChild2.LayoutGetLeft());
        AssertFloatEqual(10, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(0, rootChild2.LayoutGetHeight());

        AssertFloatEqual(50, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(20, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(0, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_flex_start_with_flex()
    {
        var root = new TestNode();
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(100);
        root.StyleSetHeight(120);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasisPercent(0);
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetFlexBasisPercent(0);
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetFlexGrow(1);
        rootChild3.StyleSetFlexShrink(1);
        rootChild3.StyleSetFlexBasisPercent(0);
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(120, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(40, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(40, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(80, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(0, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(80, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(120, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(0, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(120, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(40, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(40, rootChild1.LayoutGetHeight());

        AssertFloatEqual(50, rootChild2.LayoutGetLeft());
        AssertFloatEqual(80, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(0, rootChild2.LayoutGetHeight());

        AssertFloatEqual(50, rootChild3.LayoutGetLeft());
        AssertFloatEqual(80, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(120, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(0, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_flex_end()
    {
        var root = new TestNode();
        root.StyleSetAlignContent(Align.End);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        rootChild3.StyleSetHeight(10);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        rootChild4.StyleSetHeight(10);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(30, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(40, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(10, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(50, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        AssertFloatEqual(50, rootChild3.LayoutGetLeft());
        AssertFloatEqual(30, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(40, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(10, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch()
    {
        var root = new TestNode();
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(0, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(0, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(0, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(0, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(100, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        AssertFloatEqual(100, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(0, rootChild2.LayoutGetHeight());

        AssertFloatEqual(100, rootChild3.LayoutGetLeft());
        AssertFloatEqual(0, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(0, rootChild3.LayoutGetHeight());

        AssertFloatEqual(100, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(0, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_spacebetween()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.SpaceBetween);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(130);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        rootChild3.StyleSetHeight(10);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        rootChild4.StyleSetHeight(10);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(130, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(45, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        AssertFloatEqual(50, rootChild3.LayoutGetLeft());
        AssertFloatEqual(45, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(90, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(10, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(130, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(80, rootChild2.LayoutGetLeft());
        AssertFloatEqual(45, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        AssertFloatEqual(30, rootChild3.LayoutGetLeft());
        AssertFloatEqual(45, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(80, rootChild4.LayoutGetLeft());
        AssertFloatEqual(90, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(10, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_spacearound()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.SpaceAround);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(140);
        root.StyleSetHeight(120);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        rootChild3.StyleSetHeight(10);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        rootChild4.StyleSetHeight(10);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(140, root.LayoutGetWidth());
        AssertFloatEqual(120, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(15, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(15, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(55, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        AssertFloatEqual(50, rootChild3.LayoutGetLeft());
        AssertFloatEqual(55, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(95, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(10, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(140, root.LayoutGetWidth());
        AssertFloatEqual(120, root.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0.LayoutGetLeft());
        AssertFloatEqual(15, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(15, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(90, rootChild2.LayoutGetLeft());
        AssertFloatEqual(55, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        AssertFloatEqual(40, rootChild3.LayoutGetLeft());
        AssertFloatEqual(55, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(90, rootChild4.LayoutGetLeft());
        AssertFloatEqual(95, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(10, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(100, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(50, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(50, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(100, rootChild3.LayoutGetLeft());
        AssertFloatEqual(50, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(50, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_row_with_children()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexGrow(1);
        rootChild0Child0.StyleSetFlexShrink(1);
        rootChild0Child0.StyleSetFlexBasisPercent(0);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(100, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(50, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(50, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(100, rootChild3.LayoutGetLeft());
        AssertFloatEqual(50, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(50, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_row_with_flex()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetFlexShrink(1);
        rootChild1.StyleSetFlexBasisPercent(0);
        rootChild1.StyleSetWidth(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetFlexGrow(1);
        rootChild3.StyleSetFlexShrink(1);
        rootChild3.StyleSetFlexBasisPercent(0);
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(50, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        AssertFloatEqual(100, rootChild3.LayoutGetLeft());
        AssertFloatEqual(0, rootChild3.LayoutGetTop());
        AssertFloatEqual(0, rootChild3.LayoutGetWidth());
        AssertFloatEqual(100, rootChild3.LayoutGetHeight());

        AssertFloatEqual(100, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(100, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(100, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(50, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        AssertFloatEqual(50, rootChild3.LayoutGetLeft());
        AssertFloatEqual(0, rootChild3.LayoutGetTop());
        AssertFloatEqual(0, rootChild3.LayoutGetWidth());
        AssertFloatEqual(100, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(100, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_row_with_flex_no_shrink()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetFlexShrink(1);
        rootChild1.StyleSetFlexBasisPercent(0);
        rootChild1.StyleSetWidth(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetFlexGrow(1);
        rootChild3.StyleSetFlexBasisPercent(0);
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(50, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        AssertFloatEqual(100, rootChild3.LayoutGetLeft());
        AssertFloatEqual(0, rootChild3.LayoutGetTop());
        AssertFloatEqual(0, rootChild3.LayoutGetWidth());
        AssertFloatEqual(100, rootChild3.LayoutGetHeight());

        AssertFloatEqual(100, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(100, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(100, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(50, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        AssertFloatEqual(50, rootChild3.LayoutGetLeft());
        AssertFloatEqual(0, rootChild3.LayoutGetTop());
        AssertFloatEqual(0, rootChild3.LayoutGetWidth());
        AssertFloatEqual(100, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(100, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_row_with_margin()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetMargin(Edge.Left, 10);
        rootChild1.StyleSetMargin(Edge.Top, 10);
        rootChild1.StyleSetMargin(Edge.Right, 10);
        rootChild1.StyleSetMargin(Edge.Bottom, 10);
        rootChild1.StyleSetWidth(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetMargin(Edge.Left, 10);
        rootChild3.StyleSetMargin(Edge.Top, 10);
        rootChild3.StyleSetMargin(Edge.Right, 10);
        rootChild3.StyleSetMargin(Edge.Bottom, 10);
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        AssertFloatEqual(60, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(40, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(40, rootChild2.LayoutGetHeight());

        AssertFloatEqual(60, rootChild3.LayoutGetLeft());
        AssertFloatEqual(50, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(20, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(80, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(20, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(100, rootChild2.LayoutGetLeft());
        AssertFloatEqual(40, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(40, rootChild2.LayoutGetHeight());

        AssertFloatEqual(40, rootChild3.LayoutGetLeft());
        AssertFloatEqual(50, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(20, rootChild3.LayoutGetHeight());

        AssertFloatEqual(100, rootChild4.LayoutGetLeft());
        AssertFloatEqual(80, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(20, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_row_with_padding()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetPadding(Edge.Left, 10);
        rootChild1.StyleSetPadding(Edge.Top, 10);
        rootChild1.StyleSetPadding(Edge.Right, 10);
        rootChild1.StyleSetPadding(Edge.Bottom, 10);
        rootChild1.StyleSetWidth(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetPadding(Edge.Left, 10);
        rootChild3.StyleSetPadding(Edge.Top, 10);
        rootChild3.StyleSetPadding(Edge.Right, 10);
        rootChild3.StyleSetPadding(Edge.Bottom, 10);
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(100, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(50, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(50, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(100, rootChild3.LayoutGetLeft());
        AssertFloatEqual(50, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(50, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_row_with_single_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_row_with_fixed_height()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(60);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(60, rootChild1.LayoutGetHeight());

        AssertFloatEqual(100, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(80, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(80, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(20, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(80, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(20, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(60, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(80, rootChild2.LayoutGetHeight());

        AssertFloatEqual(100, rootChild3.LayoutGetLeft());
        AssertFloatEqual(80, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(20, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(80, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(20, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_row_with_max_height()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetMaxHeight(20);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(100, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(50, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(50, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(100, rootChild3.LayoutGetLeft());
        AssertFloatEqual(50, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(50, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_row_with_min_height()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetMinHeight(80);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(90, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(90, rootChild1.LayoutGetHeight());

        AssertFloatEqual(100, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(90, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(90, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(90, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(10, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(90, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(90, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(90, rootChild2.LayoutGetHeight());

        AssertFloatEqual(100, rootChild3.LayoutGetLeft());
        AssertFloatEqual(90, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(10, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(90, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(10, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_column()
    {
        var root = new TestNode();
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(100);
        root.StyleSetHeight(150);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexGrow(1);
        rootChild0Child0.StyleSetFlexShrink(1);
        rootChild0Child0.StyleSetFlexBasisPercent(0);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetFlexShrink(1);
        rootChild1.StyleSetFlexBasisPercent(0);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetHeight(50);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetHeight(50);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetHeight(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(150, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(100, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        AssertFloatEqual(50, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(150, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        AssertFloatEqual(50, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        AssertFloatEqual(50, rootChild3.LayoutGetLeft());
        AssertFloatEqual(100, rootChild3.LayoutGetTop());
        AssertFloatEqual(50, rootChild3.LayoutGetWidth());
        AssertFloatEqual(50, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(50, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_content_stretch_is_not_overriding_align_items()
    {
        var root = new TestNode();
        root.StyleSetAlignContent(Align.Stretch);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetAlignContent(Align.Stretch);
        rootChild0.StyleSetAlignItems(Align.Center);
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetHeight(100);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetAlignContent(Align.Stretch);
        rootChild0Child0.StyleSetWidth(10);
        rootChild0Child0.StyleSetHeight(10);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(45, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(45, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_self_center()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAlignSelf(Align.Center);
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(45, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(45, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_self_flex_end()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAlignSelf(Align.End);
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_self_flex_start()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAlignSelf(Align.Start);
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_self_flex_end_override_flex_start()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAlignSelf(Align.End);
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_self_baseline()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAlignSelf(Align.Baseline);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetAlignSelf(Align.Baseline);
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        var rootChild1Child0 = new TestNode();
        rootChild1Child0.StyleSetWidth(50);
        rootChild1Child0.StyleSetHeight(10);
        rootChild1.InsertChild(rootChild1Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(40, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(40, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1Child0.LayoutGetHeight());
    }

    private static Size _measure(
        Node<TestNode.Children> node,
        float width,
        MeasureMode widthMode,
        float height,
        MeasureMode heightMode
    )
    {
        if (widthMode != MeasureMode.Exactly)
            width = 50;
        if (heightMode != MeasureMode.Exactly)
            height = 50;
        return new Size(width, height);
    }

    [Test]
    public void TestAspect_ratio_cross_defined()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_main_defined()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_both_dimensions_defined_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_both_dimensions_defined_column()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_align_stretch()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_flex_grow()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_flex_shrink()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(150);
        rootChild0.StyleSetFlexShrink(1);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_basis()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexBasis(50);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_absolute_layout_width_defined()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Left, 0);
        rootChild0.StyleSetPosition(Edge.Top, 0);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_absolute_layout_height_defined()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Left, 0);
        rootChild0.StyleSetPosition(Edge.Top, 0);
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_with_max_cross_defined()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetMaxWidth(40);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_with_max_main_defined()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetMaxHeight(40);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_with_min_cross_defined()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(30);
        rootChild0.StyleSetMinWidth(40);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0.LayoutGetWidth());
        AssertFloatEqual(30, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_with_min_main_defined()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetMinHeight(40);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_double_cross()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetAspectRatio(2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_half_cross()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(100);
        rootChild0.StyleSetAspectRatio(0.5f);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_double_main()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetAspectRatio(0.5f);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_half_main()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetAspectRatio(2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_with_measure_func()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.SetMeasureFunc(_measure);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_width_height_flex_grow_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_width_height_flex_grow_column()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(200);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_height_as_flex_basis()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(100);
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetAspectRatio(1);
        root.InsertChild(rootChild1, 1);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(75, rootChild0.LayoutGetWidth());
        AssertFloatEqual(75, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(125, rootChild1.LayoutGetWidth());
        AssertFloatEqual(125, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_width_as_flex_basis()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(100);
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetAspectRatio(1);
        root.InsertChild(rootChild1, 1);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(75, rootChild0.LayoutGetWidth());
        AssertFloatEqual(75, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(75, rootChild1.LayoutGetTop());
        AssertFloatEqual(125, rootChild1.LayoutGetWidth());
        AssertFloatEqual(125, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_overrides_flex_grow_row()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetAspectRatio(0.5f);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_overrides_flex_grow_column()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetAspectRatio(2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_left_right_absolute()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Left, 10);
        rootChild0.StyleSetPosition(Edge.Top, 10);
        rootChild0.StyleSetPosition(Edge.Right, 10);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_top_bottom_absolute()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPosition(Edge.Left, 10);
        rootChild0.StyleSetPosition(Edge.Top, 10);
        rootChild0.StyleSetPosition(Edge.Bottom, 10);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_width_overrides_align_stretch_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_height_overrides_align_stretch_column()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetAspectRatio(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_allow_child_overflow_parent_size()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetAspectRatio(4);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_defined_main_with_margin()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(50);
        rootChild0.StyleSetAspectRatio(1);
        rootChild0.StyleSetMargin(Edge.Left, 10);
        rootChild0.StyleSetMargin(Edge.Right, 10);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAspect_ratio_defined_cross_with_margin()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetAspectRatio(1);
        rootChild0.StyleSetMargin(Edge.Left, 10);
        rootChild0.StyleSetMargin(Edge.Right, 10);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    private static float BaselineFunc(Node<TestNode.Children> node, float width, float height)
    {
        return (float)((TestNode)node).Context!;
    }

    [Test]
    public void TestAlign_baseline_customer_func()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.Baseline);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        float baselineValue = 10;
        var rootChild1Child0 = new TestNode { Context = baselineValue };
        rootChild1Child0.StyleSetWidth(50);
        rootChild1Child0.SetBaselineFunc(BaselineFunc);
        rootChild1Child0.StyleSetHeight(20);
        rootChild1.InsertChild(rootChild1Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(40, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1Child0.LayoutGetHeight());
    }

    [Test]
    public void TestBorder_no_size()
    {
        var root = new TestNode();
        root.StyleSetBorder(Edge.Left, 10);
        root.StyleSetBorder(Edge.Top, 10);
        root.StyleSetBorder(Edge.Right, 10);
        root.StyleSetBorder(Edge.Bottom, 10);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(20, root.LayoutGetWidth());
        AssertFloatEqual(20, root.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(20, root.LayoutGetWidth());
        AssertFloatEqual(20, root.LayoutGetHeight());
    }

    [Test]
    public void TestBorder_container_match_child()
    {
        var root = new TestNode();
        root.StyleSetBorder(Edge.Left, 10);
        root.StyleSetBorder(Edge.Top, 10);
        root.StyleSetBorder(Edge.Right, 10);
        root.StyleSetBorder(Edge.Bottom, 10);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(30, root.LayoutGetWidth());
        AssertFloatEqual(30, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(30, root.LayoutGetWidth());
        AssertFloatEqual(30, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestBorder_flex_child()
    {
        var root = new TestNode();
        root.StyleSetBorder(Edge.Left, 10);
        root.StyleSetBorder(Edge.Top, 10);
        root.StyleSetBorder(Edge.Right, 10);
        root.StyleSetBorder(Edge.Bottom, 10);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestBorder_stretch_child()
    {
        var root = new TestNode();
        root.StyleSetBorder(Edge.Left, 10);
        root.StyleSetBorder(Edge.Top, 10);
        root.StyleSetBorder(Edge.Right, 10);
        root.StyleSetBorder(Edge.Bottom, 10);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestBorder_center_child()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetBorder(Edge.Start, 10);
        root.StyleSetBorder(Edge.End, 20);
        root.StyleSetBorder(Edge.Bottom, 20);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(40, rootChild0.LayoutGetLeft());
        AssertFloatEqual(35, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(35, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestComputed_layout_margin()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);
        root.StyleSetMarginPercent(Edge.Start, 10);

        Flex.CalculateLayout(root, 100, 100, Direction.LeftToRight);

        AssertFloatEqual(10, root.LayoutGetMargin(Edge.Left));
        AssertFloatEqual(0, root.LayoutGetMargin(Edge.Right));

        Flex.CalculateLayout(root, 100, 100, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetMargin(Edge.Left));
        AssertFloatEqual(10, root.LayoutGetMargin(Edge.Right));
    }

    [Test]
    public void TestComputed_layout_padding()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);
        root.StyleSetPaddingPercent(Edge.Start, 10);

        Flex.CalculateLayout(root, 100, 100, Direction.LeftToRight);

        AssertFloatEqual(10, root.LayoutGetPadding(Edge.Left));
        AssertFloatEqual(0, root.LayoutGetPadding(Edge.Right));

        Flex.CalculateLayout(root, 100, 100, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetPadding(Edge.Left));
        AssertFloatEqual(10, root.LayoutGetPadding(Edge.Right));
    }

    [Test]
    public void TestAssert_default_values()
    {
        var root = new TestNode();

        AssertEqual(0, root.ChildrenCount);
        Node? nilNode = null;
        AssertEqual(nilNode, root.GetChild(1));
        AssertEqual(nilNode, root.GetChild(0));

        AssertEqual(Direction.Inherit, root.StyleGetDirection());
        AssertEqual(FlexDirection.Column, root.StyleGetFlexDirection());
        AssertEqual(Justify.Start, root.StyleGetJustifyContent());
        AssertEqual(Align.Start, root.StyleGetAlignContent());
        AssertEqual(Align.Stretch, root.StyleGetAlignItems());
        AssertEqual(Align.Auto, root.StyleGetAlignSelf());
        AssertEqual(PositionType.Relative, root.StyleGetPositionType());
        AssertEqual(Wrap.NoWrap, root.StyleGetFlexWrap());
        AssertEqual(Overflow.Visible, root.StyleGetOverflow());
        AssertFloatEqual(0, root.StyleGetFlexGrow());
        AssertFloatEqual(0, root.StyleGetFlexShrink());
        AssertEqual(root.NodeStyleGetFlexBasis().Unit, Unit.Auto);

        AssertEqual(root.StyleGetPosition(Edge.Left).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetPosition(Edge.Top).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetPosition(Edge.Right).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetPosition(Edge.Bottom).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetPosition(Edge.Start).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetPosition(Edge.End).Unit, Unit.Undefined);

        AssertEqual(root.StyleGetMargin(Edge.Left).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetMargin(Edge.Top).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetMargin(Edge.Right).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetMargin(Edge.Bottom).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetMargin(Edge.Start).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetMargin(Edge.End).Unit, Unit.Undefined);

        AssertEqual(root.StyleGetPadding(Edge.Left).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetPadding(Edge.Top).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetPadding(Edge.Right).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetPadding(Edge.Bottom).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetPadding(Edge.Start).Unit, Unit.Undefined);
        AssertEqual(root.StyleGetPadding(Edge.End).Unit, Unit.Undefined);

        AssertTrue(float.IsNaN(root.StyleGetBorder(Edge.Left)));
        AssertTrue(float.IsNaN(root.StyleGetBorder(Edge.Top)));
        AssertTrue(float.IsNaN(root.StyleGetBorder(Edge.Right)));
        AssertTrue(float.IsNaN(root.StyleGetBorder(Edge.Bottom)));
        AssertTrue(float.IsNaN(root.StyleGetBorder(Edge.Start)));
        AssertTrue(float.IsNaN(root.StyleGetBorder(Edge.End)));

        AssertEqual(root.StyleGetWidth().Unit, Unit.Auto);
        AssertEqual(root.StyleGetHeight().Unit, Unit.Auto);
        AssertEqual(root.StyleGetMinWidth().Unit, Unit.Undefined);
        AssertEqual(root.StyleGetMinHeight().Unit, Unit.Undefined);
        AssertEqual(root.StyleGetMaxWidth().Unit, Unit.Undefined);
        AssertEqual(root.StyleGetMaxHeight().Unit, Unit.Undefined);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetRight());
        AssertFloatEqual(0, root.LayoutGetBottom());

        AssertFloatEqual(0, root.LayoutGetMargin(Edge.Left));
        AssertFloatEqual(0, root.LayoutGetMargin(Edge.Top));
        AssertFloatEqual(0, root.LayoutGetMargin(Edge.Right));
        AssertFloatEqual(0, root.LayoutGetMargin(Edge.Bottom));

        AssertFloatEqual(0, root.LayoutGetPadding(Edge.Left));
        AssertFloatEqual(0, root.LayoutGetPadding(Edge.Top));
        AssertFloatEqual(0, root.LayoutGetPadding(Edge.Right));
        AssertFloatEqual(0, root.LayoutGetPadding(Edge.Bottom));

        AssertFloatEqual(0, root.LayoutGetBorder(Edge.Left));
        AssertFloatEqual(0, root.LayoutGetBorder(Edge.Top));
        AssertFloatEqual(0, root.LayoutGetBorder(Edge.Right));
        AssertFloatEqual(0, root.LayoutGetBorder(Edge.Bottom));

        AssertTrue(float.IsNaN(root.LayoutGetWidth()));
        AssertTrue(float.IsNaN(root.LayoutGetHeight()));
        AssertEqual(Direction.Inherit, root.LayoutGetDirection());
    }

    [Test]
    public void TestWrap_child()
    {
        var root = new TestNode();

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetHeight(100);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_grandchild()
    {
        var root = new TestNode();

        var rootChild0 = new TestNode();
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(100);
        rootChild0Child0.StyleSetHeight(100);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestDirty_propagation()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        rootChild0.StyleSetWidth(20);

        AssertTrue(rootChild0.IsDirty);
        AssertFalse(rootChild1.IsDirty);
        AssertTrue(root.IsDirty);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFalse(rootChild0.IsDirty);
        AssertFalse(rootChild1.IsDirty);
        AssertFalse(root.IsDirty);
    }

    [Test]
    public void TestDirty_propagation_only_if_prop_changed()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        rootChild0.StyleSetWidth(50);

        AssertFalse(rootChild0.IsDirty);
        AssertFalse(rootChild1.IsDirty);
        AssertFalse(root.IsDirty);
    }

    [Test]
    public void TestDirty_mark_all_children_as_dirty_when_display_changes()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetHeight(100);

        var child0 = new TestNode();
        child0.StyleSetFlexGrow(1);
        var child1 = new TestNode();
        child1.StyleSetFlexGrow(1);

        var child1Child0 = new TestNode();
        var child1Child0Child0 = new TestNode();
        child1Child0Child0.StyleSetWidth(8);
        child1Child0Child0.StyleSetHeight(16);

        child1Child0.InsertChild(child1Child0Child0, 0);

        child1.InsertChild(child1Child0, 0);
        root.InsertChild(child0, 0);
        root.InsertChild(child1, 0);

        child0.StyleSetDisplay(Display.Flex);
        child1.StyleSetDisplay(Display.None);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(0, child1Child0Child0.LayoutGetWidth());
        AssertFloatEqual(0, child1Child0Child0.LayoutGetHeight());

        child0.StyleSetDisplay(Display.None);
        child1.StyleSetDisplay(Display.Flex);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(8, child1Child0Child0.LayoutGetWidth());
        AssertFloatEqual(16, child1Child0Child0.LayoutGetHeight());

        child0.StyleSetDisplay(Display.Flex);
        child1.StyleSetDisplay(Display.None);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(0, child1Child0Child0.LayoutGetWidth());
        AssertFloatEqual(0, child1Child0Child0.LayoutGetHeight());

        child0.StyleSetDisplay(Display.None);
        child1.StyleSetDisplay(Display.Flex);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(8, child1Child0Child0.LayoutGetWidth());
        AssertFloatEqual(16, child1Child0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestDirty_node_only_if_children_are_actually_removed()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(50);
        root.StyleSetHeight(50);

        var child0 = new TestNode();
        child0.StyleSetWidth(50);
        child0.StyleSetHeight(25);
        root.InsertChild(child0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        var child1 = new TestNode();
        root.RemoveChild(child1);
        AssertFalse(root.IsDirty);

        root.RemoveChild(child0);
        AssertTrue(root.IsDirty);
    }

    [Test]
    public void TestDisplay_none()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetDisplay(Display.None);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestDisplay_none_fixed_size()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(20);
        rootChild1.StyleSetHeight(20);
        rootChild1.StyleSetDisplay(Display.None);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestDisplay_none_with_margin()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMargin(Edge.Left, 10);
        rootChild0.StyleSetMargin(Edge.Top, 10);
        rootChild0.StyleSetMargin(Edge.Right, 10);
        rootChild0.StyleSetMargin(Edge.Bottom, 10);
        rootChild0.StyleSetWidth(20);
        rootChild0.StyleSetHeight(20);
        rootChild0.StyleSetDisplay(Display.None);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestDisplay_none_with_child()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexShrink(1);
        rootChild0.StyleSetFlexBasisPercent(0);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetFlexShrink(1);
        rootChild1.StyleSetFlexBasisPercent(0);
        rootChild1.StyleSetDisplay(Display.None);
        root.InsertChild(rootChild1, 1);

        var rootChild1Child0 = new TestNode();
        rootChild1Child0.StyleSetFlexGrow(1);
        rootChild1Child0.StyleSetFlexShrink(1);
        rootChild1Child0.StyleSetFlexBasisPercent(0);
        rootChild1Child0.StyleSetWidth(20);
        rootChild1Child0.StyleSetMinWidth(0);
        rootChild1Child0.StyleSetMinHeight(0);
        rootChild1.InsertChild(rootChild1Child0, 0);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        rootChild2.StyleSetFlexShrink(1);
        rootChild2.StyleSetFlexBasisPercent(0);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestDisplay_none_with_position()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetPosition(Edge.Top, 10);
        rootChild1.StyleSetDisplay(Display.None);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestStart_overrides()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Start, 10);
        rootChild0.StyleSetMargin(Edge.Left, 20);
        rootChild0.StyleSetMargin(Edge.Right, 20);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetRight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);
        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetRight());
    }

    [Test]
    public void TestEnd_overrides()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.End, 10);
        rootChild0.StyleSetMargin(Edge.Left, 20);
        rootChild0.StyleSetMargin(Edge.Right, 20);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetRight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);
        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetRight());
    }

    [Test]
    public void TestHorizontal_overridden()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Horizontal, 10);
        rootChild0.StyleSetMargin(Edge.Left, 20);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetRight());
    }

    [Test]
    public void TestVertical_overridden()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Column);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Vertical, 10);
        rootChild0.StyleSetMargin(Edge.Top, 20);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(20, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetBottom());
    }

    [Test]
    public void TestHorizontal_overrides_all()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Column);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Horizontal, 10);
        rootChild0.StyleSetMargin(Edge.All, 20);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetRight());
        AssertFloatEqual(20, rootChild0.LayoutGetBottom());
    }

    [Test]
    public void TestVertical_overrides_all()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Column);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Vertical, 10);
        rootChild0.StyleSetMargin(Edge.All, 20);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(20, rootChild0.LayoutGetRight());
        AssertFloatEqual(10, rootChild0.LayoutGetBottom());
    }

    [Test]
    public void TestAll_overridden()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Column);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Left, 10);
        rootChild0.StyleSetMargin(Edge.Top, 10);
        rootChild0.StyleSetMargin(Edge.Right, 10);
        rootChild0.StyleSetMargin(Edge.Bottom, 10);
        rootChild0.StyleSetMargin(Edge.All, 20);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetRight());
        AssertFloatEqual(10, rootChild0.LayoutGetBottom());
    }

    [Test]
    public void TestFlex_direction_column_no_height()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(30, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(30, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_direction_row_no_width()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(30, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(10, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(20, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(30, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(10, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_direction_column()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_direction_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(10, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(20, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(80, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(70, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_direction_column_reverse()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.ColumnReverse);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(90, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(80, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(70, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(90, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(80, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(70, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_direction_row_reverse()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.RowReverse);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(80, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(70, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(10, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(20, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_basis_flex_grow_column()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(75, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(75, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(25, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(75, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(75, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(25, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_basis_flex_grow_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(75, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(25, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(75, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(25, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_basis_flex_shrink_column()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexShrink(1);
        rootChild0.StyleSetFlexBasis(100);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexBasis(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_basis_flex_shrink_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexShrink(1);
        rootChild0.StyleSetFlexBasis(100);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexBasis(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_shrink_to_zero()
    {
        var root = new TestNode();
        root.StyleSetHeight(75);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexShrink(1);
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        rootChild2.StyleSetHeight(50);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(75, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(75, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_basis_overrides_main_size()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(50);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(60, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(60, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(80, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(20, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(60, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(60, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(80, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(20, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_shrink_at_most()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexGrow(1);
        rootChild0Child0.StyleSetFlexShrink(1);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_less_than_factor_one()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(500);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(0.2f);
        rootChild0.StyleSetFlexBasis(40);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(0.2f);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(0.4f);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(132, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(132, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(92, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(224, rootChild2.LayoutGetTop());
        AssertFloatEqual(200, rootChild2.LayoutGetWidth());
        AssertFloatEqual(184, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(132, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(132, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(92, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(224, rootChild2.LayoutGetTop());
        AssertFloatEqual(200, rootChild2.LayoutGetWidth());
        AssertFloatEqual(184, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_column()
    {
        var root = new TestNode();
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetHeight(30);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(30);
        rootChild1.StyleSetHeight(30);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(30);
        rootChild2.StyleSetHeight(30);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(30);
        rootChild3.StyleSetHeight(30);
        root.InsertChild(rootChild3, 3);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(60, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(30, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(30, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(30, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(60, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(30, rootChild3.LayoutGetLeft());
        AssertFloatEqual(0, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(30, rootChild3.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(60, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(30, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(30, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(30, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(30, rootChild1.LayoutGetHeight());

        AssertFloatEqual(30, rootChild2.LayoutGetLeft());
        AssertFloatEqual(60, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(0, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(30, rootChild3.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetHeight(30);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(30);
        rootChild1.StyleSetHeight(30);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(30);
        rootChild2.StyleSetHeight(30);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(30);
        rootChild3.StyleSetHeight(30);
        root.InsertChild(rootChild3, 3);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(60, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(30, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(30, rootChild1.LayoutGetHeight());

        AssertFloatEqual(60, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(30, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(30, rootChild3.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(60, root.LayoutGetHeight());

        AssertFloatEqual(70, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(30, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(30, rootChild1.LayoutGetHeight());

        AssertFloatEqual(10, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(70, rootChild3.LayoutGetLeft());
        AssertFloatEqual(30, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(30, rootChild3.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_row_align_items_flex_end()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.End);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(30);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(30);
        rootChild2.StyleSetHeight(30);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(30);
        rootChild3.StyleSetHeight(30);
        root.InsertChild(rootChild3, 3);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(60, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(60, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(30, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(30, rootChild3.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(60, root.LayoutGetHeight());

        AssertFloatEqual(70, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(10, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(70, rootChild3.LayoutGetLeft());
        AssertFloatEqual(30, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(30, rootChild3.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_row_align_items_center()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(30);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(30);
        rootChild2.StyleSetHeight(30);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(30);
        rootChild3.StyleSetHeight(30);
        root.InsertChild(rootChild3, 3);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(60, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(5, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(60, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(30, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(30, rootChild3.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(60, root.LayoutGetHeight());

        AssertFloatEqual(70, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(5, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(10, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(70, rootChild3.LayoutGetLeft());
        AssertFloatEqual(30, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(30, rootChild3.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_wrap_children_with_min_main_overriding_flex_basis()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexBasis(50);
        rootChild0.StyleSetMinWidth(55);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexBasis(50);
        rootChild1.StyleSetMinWidth(55);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(55, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(55, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(45, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(55, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(45, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(55, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_wrap_wrap_to_child_height()
    {
        var root = new TestNode();

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetAlignItems(Align.Start);
        rootChild0.StyleSetFlexWrap(Wrap.Wrap);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(100);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child0Child0 = new TestNode();
        rootChild0Child0Child0.StyleSetWidth(100);
        rootChild0Child0Child0.StyleSetHeight(100);
        rootChild0Child0.InsertChild(rootChild0Child0Child0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(100);
        rootChild1.StyleSetHeight(100);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0Child0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(100, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0Child0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(100, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_wrap_align_stretch_fits_one_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(150);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(150, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_reverse_row_align_content_flex_start()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetFlexWrap(Wrap.WrapReverse);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(30);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(30);
        rootChild2.StyleSetHeight(30);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(30);
        rootChild3.StyleSetHeight(40);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(30);
        rootChild4.StyleSetHeight(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(80, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(70, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(60, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(60, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(30, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(80, root.LayoutGetHeight());

        AssertFloatEqual(70, rootChild0.LayoutGetLeft());
        AssertFloatEqual(70, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(60, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(10, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(70, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(40, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_reverse_row_align_content_center()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Center);
        root.StyleSetFlexWrap(Wrap.WrapReverse);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(30);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(30);
        rootChild2.StyleSetHeight(30);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(30);
        rootChild3.StyleSetHeight(40);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(30);
        rootChild4.StyleSetHeight(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(80, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(70, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(60, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(60, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(30, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(80, root.LayoutGetHeight());

        AssertFloatEqual(70, rootChild0.LayoutGetLeft());
        AssertFloatEqual(70, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(60, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(10, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(70, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(40, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_reverse_row_single_line_different_size()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetFlexWrap(Wrap.WrapReverse);
        root.StyleSetWidth(300);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(30);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(30);
        rootChild2.StyleSetHeight(30);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(30);
        rootChild3.StyleSetHeight(40);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(30);
        rootChild4.StyleSetHeight(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(300, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(40, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(30, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(60, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(90, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(120, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(300, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(270, rootChild0.LayoutGetLeft());
        AssertFloatEqual(40, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(240, rootChild1.LayoutGetLeft());
        AssertFloatEqual(30, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(210, rootChild2.LayoutGetLeft());
        AssertFloatEqual(20, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(180, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(150, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_reverse_row_align_content_stretch()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.Stretch);
        root.StyleSetFlexWrap(Wrap.WrapReverse);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(30);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(30);
        rootChild2.StyleSetHeight(30);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(30);
        rootChild3.StyleSetHeight(40);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(30);
        rootChild4.StyleSetHeight(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(80, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(70, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(60, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(60, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(30, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(80, root.LayoutGetHeight());

        AssertFloatEqual(70, rootChild0.LayoutGetLeft());
        AssertFloatEqual(70, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(60, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(10, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(70, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(40, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_reverse_row_align_content_space_around()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignContent(Align.SpaceAround);
        root.StyleSetFlexWrap(Wrap.WrapReverse);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(30);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(30);
        rootChild2.StyleSetHeight(30);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(30);
        rootChild3.StyleSetHeight(40);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(30);
        rootChild4.StyleSetHeight(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(80, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(70, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild1.LayoutGetLeft());
        AssertFloatEqual(60, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(60, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(30, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(80, root.LayoutGetHeight());

        AssertFloatEqual(70, rootChild0.LayoutGetLeft());
        AssertFloatEqual(70, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(60, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(10, rootChild2.LayoutGetLeft());
        AssertFloatEqual(50, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(70, rootChild3.LayoutGetLeft());
        AssertFloatEqual(10, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(40, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_reverse_column_fixed_size()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetFlexWrap(Wrap.WrapReverse);
        root.StyleSetWidth(200);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(30);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(30);
        rootChild1.StyleSetHeight(20);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(30);
        rootChild2.StyleSetHeight(30);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetWidth(30);
        rootChild3.StyleSetHeight(40);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetWidth(30);
        rootChild4.StyleSetHeight(50);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(170, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(170, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(170, rootChild2.LayoutGetLeft());
        AssertFloatEqual(30, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(170, rootChild3.LayoutGetLeft());
        AssertFloatEqual(60, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(140, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(30, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(30, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(30, rootChild2.LayoutGetTop());
        AssertFloatEqual(30, rootChild2.LayoutGetWidth());
        AssertFloatEqual(30, rootChild2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild3.LayoutGetLeft());
        AssertFloatEqual(60, rootChild3.LayoutGetTop());
        AssertFloatEqual(30, rootChild3.LayoutGetWidth());
        AssertFloatEqual(40, rootChild3.LayoutGetHeight());

        AssertFloatEqual(30, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(30, rootChild4.LayoutGetWidth());
        AssertFloatEqual(50, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestWrapped_row_within_align_items_center()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetFlexWrap(Wrap.Wrap);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(150);
        rootChild0Child0.StyleSetHeight(80);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child1 = new TestNode();
        rootChild0Child1.StyleSetWidth(80);
        rootChild0Child1.StyleSetHeight(80);
        rootChild0.InsertChild(rootChild0Child1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(160, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(150, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(160, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(150, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(120, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetHeight());
    }

    [Test]
    public void TestWrapped_row_within_align_items_flex_start()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetFlexWrap(Wrap.Wrap);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(150);
        rootChild0Child0.StyleSetHeight(80);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child1 = new TestNode();
        rootChild0Child1.StyleSetWidth(80);
        rootChild0Child1.StyleSetHeight(80);
        rootChild0.InsertChild(rootChild0Child1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(160, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(150, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(160, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(150, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(120, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetHeight());
    }

    [Test]
    public void TestWrapped_row_within_align_items_flex_end()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.End);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetFlexWrap(Wrap.Wrap);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(150);
        rootChild0Child0.StyleSetHeight(80);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child1 = new TestNode();
        rootChild0Child1.StyleSetWidth(80);
        rootChild0Child1.StyleSetHeight(80);
        rootChild0.InsertChild(rootChild0Child1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(160, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(150, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(160, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(150, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(120, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0Child1.LayoutGetHeight());
    }

    [Test]
    public void TestWrapped_column_max_height()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetAlignContent(Align.Center);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(700);
        root.StyleSetHeight(500);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetHeight(500);
        rootChild0.StyleSetMaxHeight(200);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetMargin(Edge.Left, 20);
        rootChild1.StyleSetMargin(Edge.Top, 20);
        rootChild1.StyleSetMargin(Edge.Right, 20);
        rootChild1.StyleSetMargin(Edge.Bottom, 20);
        rootChild1.StyleSetWidth(200);
        rootChild1.StyleSetHeight(200);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(100);
        rootChild2.StyleSetHeight(100);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(700, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(250, rootChild0.LayoutGetLeft());
        AssertFloatEqual(30, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(200, rootChild1.LayoutGetLeft());
        AssertFloatEqual(250, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(200, rootChild1.LayoutGetHeight());

        AssertFloatEqual(420, rootChild2.LayoutGetLeft());
        AssertFloatEqual(200, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(700, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(350, rootChild0.LayoutGetLeft());
        AssertFloatEqual(30, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(300, rootChild1.LayoutGetLeft());
        AssertFloatEqual(250, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(200, rootChild1.LayoutGetHeight());

        AssertFloatEqual(180, rootChild2.LayoutGetLeft());
        AssertFloatEqual(200, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestWrapped_column_max_height_flex()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetAlignContent(Align.Center);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetFlexWrap(Wrap.Wrap);
        root.StyleSetWidth(700);
        root.StyleSetHeight(500);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexShrink(1);
        rootChild0.StyleSetFlexBasisPercent(0);
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetHeight(500);
        rootChild0.StyleSetMaxHeight(200);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetFlexShrink(1);
        rootChild1.StyleSetFlexBasisPercent(0);
        rootChild1.StyleSetMargin(Edge.Left, 20);
        rootChild1.StyleSetMargin(Edge.Top, 20);
        rootChild1.StyleSetMargin(Edge.Right, 20);
        rootChild1.StyleSetMargin(Edge.Bottom, 20);
        rootChild1.StyleSetWidth(200);
        rootChild1.StyleSetHeight(200);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(100);
        rootChild2.StyleSetHeight(100);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(700, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(300, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(180, rootChild0.LayoutGetHeight());

        AssertFloatEqual(250, rootChild1.LayoutGetLeft());
        AssertFloatEqual(200, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(180, rootChild1.LayoutGetHeight());

        AssertFloatEqual(300, rootChild2.LayoutGetLeft());
        AssertFloatEqual(400, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(700, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(300, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(180, rootChild0.LayoutGetHeight());

        AssertFloatEqual(250, rootChild1.LayoutGetLeft());
        AssertFloatEqual(200, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(180, rootChild1.LayoutGetHeight());

        AssertFloatEqual(300, rootChild2.LayoutGetLeft());
        AssertFloatEqual(400, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_nodes_with_content_sizing_overflowing_margin()
    {
        var root = new TestNode();
        root.StyleSetWidth(500);
        root.StyleSetHeight(500);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetFlexWrap(Wrap.Wrap);
        rootChild0.StyleSetWidth(85);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child0Child0 = new TestNode();
        rootChild0Child0Child0.StyleSetWidth(40);
        rootChild0Child0Child0.StyleSetHeight(40);
        rootChild0Child0.InsertChild(rootChild0Child0Child0, 0);

        var rootChild0Child1 = new TestNode();
        rootChild0Child1.StyleSetMargin(Edge.Right, 10);
        rootChild0.InsertChild(rootChild0Child1, 1);

        var rootChild0Child1Child0 = new TestNode();
        rootChild0Child1Child0.StyleSetWidth(40);
        rootChild0Child1Child0.StyleSetHeight(40);
        rootChild0Child1.InsertChild(rootChild0Child1Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(500, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(85, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child0Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(40, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child1Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child1Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child1Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(500, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(415, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(85, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        AssertFloatEqual(45, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child0Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child0Child0.LayoutGetHeight());

        AssertFloatEqual(35, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(40, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child1Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child1Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child1Child0.LayoutGetHeight());
    }

    [Test]
    public void TestWrap_nodes_with_content_sizing_margin_cross()
    {
        var root = new TestNode();
        root.StyleSetWidth(500);
        root.StyleSetHeight(500);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetFlexWrap(Wrap.Wrap);
        rootChild0.StyleSetWidth(70);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child0Child0 = new TestNode();
        rootChild0Child0Child0.StyleSetWidth(40);
        rootChild0Child0Child0.StyleSetHeight(40);
        rootChild0Child0.InsertChild(rootChild0Child0Child0, 0);

        var rootChild0Child1 = new TestNode();
        rootChild0Child1.StyleSetMargin(Edge.Top, 10);
        rootChild0.InsertChild(rootChild0Child1, 1);

        var rootChild0Child1Child0 = new TestNode();
        rootChild0Child1Child0.StyleSetWidth(40);
        rootChild0Child1Child0.StyleSetHeight(40);
        rootChild0Child1.InsertChild(rootChild0Child1Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(500, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(70, rootChild0.LayoutGetWidth());
        AssertFloatEqual(90, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child0Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child1Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child1Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child1Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(500, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(430, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(70, rootChild0.LayoutGetWidth());
        AssertFloatEqual(90, rootChild0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child0Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child0Child0.LayoutGetHeight());

        AssertFloatEqual(30, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child1Child0.LayoutGetTop());
        AssertFloatEqual(40, rootChild0Child1Child0.LayoutGetWidth());
        AssertFloatEqual(40, rootChild0Child1Child0.LayoutGetHeight());
    }

    private static void NewHadOverflowTests(out TestNode outNode)
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(100);
        root.StyleSetFlexDirection(FlexDirection.Column);
        root.StyleSetFlexWrap(Wrap.NoWrap);
        outNode = root;
    }

    [Test]
    public void TestChildren_overflow_no_wrap_and_no_flex_children()
    {
        NewHadOverflowTests(out var root);
        var child0 = new TestNode();
        child0.StyleSetWidth(80);
        child0.StyleSetHeight(40);
        child0.StyleSetMargin(Edge.Top, 10);
        child0.StyleSetMargin(Edge.Bottom, 15);
        root.InsertChild(child0, 0);
        var child1 = new TestNode();
        child1.StyleSetWidth(80);
        child1.StyleSetHeight(40);
        child1.StyleSetMargin(Edge.Bottom, 5);
        root.InsertChild(child1, 1);

        Flex.CalculateLayout(root, 200, 100, Direction.LeftToRight);

        AssertTrue(root.LayoutGetHadOverflow());
    }

    [Test]
    public void TestSpacing_overflow_no_wrap_and_no_flex_children()
    {
        NewHadOverflowTests(out var root);
        var child0 = new TestNode();
        child0.StyleSetWidth(80);
        child0.StyleSetHeight(40);
        child0.StyleSetMargin(Edge.Top, 10);
        child0.StyleSetMargin(Edge.Bottom, 10);
        root.InsertChild(child0, 0);
        var child1 = new TestNode();
        child1.StyleSetWidth(80);
        child1.StyleSetHeight(40);
        child1.StyleSetMargin(Edge.Bottom, 5);
        root.InsertChild(child1, 1);

        Flex.CalculateLayout(root, 200, 100, Direction.LeftToRight);

        AssertTrue(root.LayoutGetHadOverflow());
    }

    [Test]
    public void TestNo_overflow_no_wrap_and_flex_children()
    {
        NewHadOverflowTests(out var root);
        var child0 = new TestNode();
        child0.StyleSetWidth(80);
        child0.StyleSetHeight(40);
        child0.StyleSetMargin(Edge.Top, 10);
        child0.StyleSetMargin(Edge.Bottom, 10);
        root.InsertChild(child0, 0);
        var child1 = new TestNode();
        child1.StyleSetWidth(80);
        child1.StyleSetHeight(40);
        child1.StyleSetMargin(Edge.Bottom, 5);
        child1.StyleSetFlexShrink(1);
        root.InsertChild(child1, 1);

        Flex.CalculateLayout(root, 200, 100, Direction.LeftToRight);

        AssertFalse(root.LayoutGetHadOverflow());
    }

    [Test]
    public void TestHadOverflow_gets_reset_if_not_logger_valid()
    {
        NewHadOverflowTests(out var root);
        var child0 = new TestNode();
        child0.StyleSetWidth(80);
        child0.StyleSetHeight(40);
        child0.StyleSetMargin(Edge.Top, 10);
        child0.StyleSetMargin(Edge.Bottom, 10);
        root.InsertChild(child0, 0);
        var child1 = new TestNode();
        child1.StyleSetWidth(80);
        child1.StyleSetHeight(40);
        child1.StyleSetMargin(Edge.Bottom, 5);
        root.InsertChild(child1, 1);

        Flex.CalculateLayout(root, 200, 100, Direction.LeftToRight);

        AssertTrue(root.LayoutGetHadOverflow());

        child1.StyleSetFlexShrink(1);

        Flex.CalculateLayout(root, 200, 100, Direction.LeftToRight);

        AssertFalse(root.LayoutGetHadOverflow());
    }

    [Test]
    public void TestSpacing_overflow_in_nested_nodes()
    {
        NewHadOverflowTests(out var root);
        var child0 = new TestNode();
        child0.StyleSetWidth(80);
        child0.StyleSetHeight(40);
        child0.StyleSetMargin(Edge.Top, 10);
        child0.StyleSetMargin(Edge.Bottom, 10);
        root.InsertChild(child0, 0);
        var child1 = new TestNode();
        child1.StyleSetWidth(80);
        child1.StyleSetHeight(40);
        root.InsertChild(child1, 1);
        var child11 = new TestNode();
        child11.StyleSetWidth(80);
        child11.StyleSetHeight(40);
        child11.StyleSetMargin(Edge.Bottom, 5);
        child1.InsertChild(child11, 0);

        Flex.CalculateLayout(root, 200, 100, Direction.LeftToRight);

        AssertTrue(root.LayoutGetHadOverflow());
    }

    [Test]
    public void TestJustify_content_row_flex_start()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(102, rootChild0.LayoutGetHeight());

        AssertFloatEqual(10, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(102, rootChild1.LayoutGetHeight());

        AssertFloatEqual(20, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(102, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(92, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(102, rootChild0.LayoutGetHeight());

        AssertFloatEqual(82, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(102, rootChild1.LayoutGetHeight());

        AssertFloatEqual(72, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(102, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_row_flex_end()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetJustifyContent(Justify.End);
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(72, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(102, rootChild0.LayoutGetHeight());

        AssertFloatEqual(82, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(102, rootChild1.LayoutGetHeight());

        AssertFloatEqual(92, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(102, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(102, rootChild0.LayoutGetHeight());

        AssertFloatEqual(10, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(102, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(102, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_row_center()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(36, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(102, rootChild0.LayoutGetHeight());

        AssertFloatEqual(46, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(102, rootChild1.LayoutGetHeight());

        AssertFloatEqual(56, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(102, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(56, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(102, rootChild0.LayoutGetHeight());

        AssertFloatEqual(46, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(102, rootChild1.LayoutGetHeight());

        AssertFloatEqual(36, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(102, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_row_space_between()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetJustifyContent(Justify.SpaceBetween);
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(102, rootChild0.LayoutGetHeight());

        AssertFloatEqual(46, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(102, rootChild1.LayoutGetHeight());

        AssertFloatEqual(92, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(102, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(92, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(102, rootChild0.LayoutGetHeight());

        AssertFloatEqual(46, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(102, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(102, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_row_space_around()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetJustifyContent(Justify.SpaceAround);
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(12, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(102, rootChild0.LayoutGetHeight());

        AssertFloatEqual(46, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(102, rootChild1.LayoutGetHeight());

        AssertFloatEqual(80, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(102, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(102, rootChild0.LayoutGetHeight());

        AssertFloatEqual(46, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(102, rootChild1.LayoutGetHeight());

        AssertFloatEqual(12, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(102, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_row_space_between_new()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetJustifyContent(Justify.SpaceBetween);
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        Assert.AreEqual(0, root.LayoutGetLeft());
        Assert.AreEqual(0, root.LayoutGetTop());
        Assert.AreEqual(102, root.LayoutGetWidth());
        Assert.AreEqual(102, root.LayoutGetHeight());

        Assert.AreEqual(0, rootChild0.LayoutGetLeft());
        Assert.AreEqual(0, rootChild0.LayoutGetTop());
        Assert.AreEqual(10, rootChild0.LayoutGetWidth());
        Assert.AreEqual(102, rootChild0.LayoutGetHeight());

        Assert.AreEqual(46, rootChild1.LayoutGetLeft());
        Assert.AreEqual(0, rootChild1.LayoutGetTop());
        Assert.AreEqual(10, rootChild1.LayoutGetWidth());
        Assert.AreEqual(102, rootChild1.LayoutGetHeight());

        Assert.AreEqual(92, rootChild2.LayoutGetLeft());
        Assert.AreEqual(0, rootChild2.LayoutGetTop());
        Assert.AreEqual(10, rootChild2.LayoutGetWidth());
        Assert.AreEqual(102, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        Assert.AreEqual(0, root.LayoutGetLeft());
        Assert.AreEqual(0, root.LayoutGetTop());
        Assert.AreEqual(102, root.LayoutGetWidth());
        Assert.AreEqual(102, root.LayoutGetHeight());

        Assert.AreEqual(92, rootChild0.LayoutGetLeft());
        Assert.AreEqual(0, rootChild0.LayoutGetTop());
        Assert.AreEqual(10, rootChild0.LayoutGetWidth());
        Assert.AreEqual(102, rootChild0.LayoutGetHeight());

        Assert.AreEqual(46, rootChild1.LayoutGetLeft());
        Assert.AreEqual(0, rootChild1.LayoutGetTop());
        Assert.AreEqual(10, rootChild1.LayoutGetWidth());
        Assert.AreEqual(102, rootChild1.LayoutGetHeight());

        Assert.AreEqual(0, rootChild2.LayoutGetLeft());
        Assert.AreEqual(0, rootChild2.LayoutGetTop());
        Assert.AreEqual(10, rootChild2.LayoutGetWidth());
        Assert.AreEqual(102, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_column_flex_start()
    {
        var root = new TestNode();
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(102, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(102, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(10, rootChild2.LayoutGetTop());
        AssertFloatEqual(102, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(102, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(10, rootChild1.LayoutGetTop());
        AssertFloatEqual(102, rootChild1.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(10, rootChild2.LayoutGetTop());
        AssertFloatEqual(102, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_column_flex_end()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.End);
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(72, rootChild0.LayoutGetTop());
        AssertFloatEqual(102, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(82, rootChild1.LayoutGetTop());
        AssertFloatEqual(102, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(92, rootChild2.LayoutGetTop());
        AssertFloatEqual(102, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(72, rootChild0.LayoutGetTop());
        AssertFloatEqual(102, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(82, rootChild1.LayoutGetTop());
        AssertFloatEqual(102, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(92, rootChild2.LayoutGetTop());
        AssertFloatEqual(102, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_column_center()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(36, rootChild0.LayoutGetTop());
        AssertFloatEqual(102, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(46, rootChild1.LayoutGetTop());
        AssertFloatEqual(102, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(56, rootChild2.LayoutGetTop());
        AssertFloatEqual(102, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(36, rootChild0.LayoutGetTop());
        AssertFloatEqual(102, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(46, rootChild1.LayoutGetTop());
        AssertFloatEqual(102, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(56, rootChild2.LayoutGetTop());
        AssertFloatEqual(102, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_column_new_space_between()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.SpaceBetween);
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(102, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(46, rootChild1.LayoutGetTop());
        AssertFloatEqual(102, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(92, rootChild2.LayoutGetTop());
        AssertFloatEqual(102, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(102, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(46, rootChild1.LayoutGetTop());
        AssertFloatEqual(102, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(92, rootChild2.LayoutGetTop());
        AssertFloatEqual(102, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_column_space_around()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.SpaceAround);
        root.StyleSetWidth(102);
        root.StyleSetHeight(102);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(12, rootChild0.LayoutGetTop());
        AssertFloatEqual(102, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(46, rootChild1.LayoutGetTop());
        AssertFloatEqual(102, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(80, rootChild2.LayoutGetTop());
        AssertFloatEqual(102, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(102, root.LayoutGetWidth());
        AssertFloatEqual(102, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(12, rootChild0.LayoutGetTop());
        AssertFloatEqual(102, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(46, rootChild1.LayoutGetTop());
        AssertFloatEqual(102, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(80, rootChild2.LayoutGetTop());
        AssertFloatEqual(102, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_start()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMargin(Edge.Start, 10);
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_top()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMargin(Edge.Top, 10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_end()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetJustifyContent(Justify.End);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMargin(Edge.End, 10);
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_bottom()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.End);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMargin(Edge.Bottom, 10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_and_flex_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Start, 10);
        rootChild0.StyleSetMargin(Edge.End, 10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_and_flex_column()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Top, 10);
        rootChild0.StyleSetMargin(Edge.Bottom, 10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_and_stretch_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Top, 10);
        rootChild0.StyleSetMargin(Edge.Bottom, 10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_and_stretch_column()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Start, 10);
        rootChild0.StyleSetMargin(Edge.End, 10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_with_sibling_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.End, 10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(45, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(55, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(45, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(55, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(45, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(45, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_with_sibling_column()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMargin(Edge.Bottom, 10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(45, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(55, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(45, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(45, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(55, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(45, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_bottom()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Bottom);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(150, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(150, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_top()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Top);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(100, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(150, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(100, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(150, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_bottom_and_top()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Top);
        rootChild0.StyleSetMarginAuto(Edge.Bottom);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(50, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(150, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(50, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(150, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_bottom_and_top_justify_center()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Top);
        rootChild0.StyleSetMarginAuto(Edge.Bottom);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(50, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(150, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(150, rootChild0.LayoutGetLeft());
        AssertFloatEqual(50, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(150, rootChild1.LayoutGetLeft());
        AssertFloatEqual(150, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_mutiple_children_column()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Top);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetMarginAuto(Edge.Top);
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        rootChild2.StyleSetHeight(50);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(25, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(100, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(75, rootChild2.LayoutGetLeft());
        AssertFloatEqual(150, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(25, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(100, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(75, rootChild2.LayoutGetLeft());
        AssertFloatEqual(150, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_mutiple_children_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Right);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetMarginAuto(Edge.Right);
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        rootChild2.StyleSetHeight(50);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(75, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(75, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(150, rootChild2.LayoutGetLeft());
        AssertFloatEqual(75, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(125, rootChild0.LayoutGetLeft());
        AssertFloatEqual(75, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(75, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(75, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void Testargin_auto_left_and_right_column()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Left);
        rootChild0.StyleSetMarginAuto(Edge.Right);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(75, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(150, rootChild1.LayoutGetLeft());
        AssertFloatEqual(75, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(75, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(75, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_left_and_right()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Left);
        rootChild0.StyleSetMarginAuto(Edge.Right);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(150, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_start_and_end_column()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Start);
        rootChild0.StyleSetMarginAuto(Edge.End);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(75, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(150, rootChild1.LayoutGetLeft());
        AssertFloatEqual(75, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(75, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(75, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_start_and_end()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Start);
        rootChild0.StyleSetMarginAuto(Edge.End);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(150, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_left_and_right_column_and_center()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Left);
        rootChild0.StyleSetMarginAuto(Edge.Right);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_left()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Left);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(150, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(150, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_right()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Right);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_left_and_right_strech()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Left);
        rootChild0.StyleSetMarginAuto(Edge.Right);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(150, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_top_and_bottom_strech()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Top);
        rootChild0.StyleSetMarginAuto(Edge.Bottom);
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(50, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(150, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(150, rootChild0.LayoutGetLeft());
        AssertFloatEqual(50, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(150, rootChild1.LayoutGetLeft());
        AssertFloatEqual(150, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_should_not_be_part_of_max_height()
    {
        var root = new TestNode();
        root.StyleSetWidth(250);
        root.StyleSetHeight(250);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMargin(Edge.Top, 20);
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetHeight(100);
        rootChild0.StyleSetMaxHeight(100);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(250, root.LayoutGetWidth());
        AssertFloatEqual(250, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(250, root.LayoutGetWidth());
        AssertFloatEqual(250, root.LayoutGetHeight());

        AssertFloatEqual(150, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_should_not_be_part_of_max_width()
    {
        var root = new TestNode();
        root.StyleSetWidth(250);
        root.StyleSetHeight(250);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMargin(Edge.Left, 20);
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetMaxWidth(100);
        rootChild0.StyleSetHeight(100);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(250, root.LayoutGetWidth());
        AssertFloatEqual(250, root.LayoutGetHeight());

        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(250, root.LayoutGetWidth());
        AssertFloatEqual(250, root.LayoutGetHeight());

        AssertFloatEqual(150, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_left_right_child_bigger_than_parent()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetWidth(52);
        root.StyleSetHeight(52);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Left);
        rootChild0.StyleSetMarginAuto(Edge.Right);
        rootChild0.StyleSetWidth(72);
        rootChild0.StyleSetHeight(72);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(52, root.LayoutGetWidth());
        AssertFloatEqual(52, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-10, rootChild0.LayoutGetTop());
        AssertFloatEqual(72, rootChild0.LayoutGetWidth());
        AssertFloatEqual(72, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(52, root.LayoutGetWidth());
        AssertFloatEqual(52, root.LayoutGetHeight());

        AssertFloatEqual(-20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-10, rootChild0.LayoutGetTop());
        AssertFloatEqual(72, rootChild0.LayoutGetWidth());
        AssertFloatEqual(72, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_left_child_bigger_than_parent()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetWidth(52);
        root.StyleSetHeight(52);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Left);
        rootChild0.StyleSetWidth(72);
        rootChild0.StyleSetHeight(72);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(52, root.LayoutGetWidth());
        AssertFloatEqual(52, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-10, rootChild0.LayoutGetTop());
        AssertFloatEqual(72, rootChild0.LayoutGetWidth());
        AssertFloatEqual(72, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(52, root.LayoutGetWidth());
        AssertFloatEqual(52, root.LayoutGetHeight());

        AssertFloatEqual(-20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-10, rootChild0.LayoutGetTop());
        AssertFloatEqual(72, rootChild0.LayoutGetWidth());
        AssertFloatEqual(72, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_fix_left_auto_right_child_bigger_than_parent()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetWidth(52);
        root.StyleSetHeight(52);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMargin(Edge.Left, 10);
        rootChild0.StyleSetMarginAuto(Edge.Right);
        rootChild0.StyleSetWidth(72);
        rootChild0.StyleSetHeight(72);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(52, root.LayoutGetWidth());
        AssertFloatEqual(52, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-10, rootChild0.LayoutGetTop());
        AssertFloatEqual(72, rootChild0.LayoutGetWidth());
        AssertFloatEqual(72, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(52, root.LayoutGetWidth());
        AssertFloatEqual(52, root.LayoutGetHeight());

        AssertFloatEqual(-20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-10, rootChild0.LayoutGetTop());
        AssertFloatEqual(72, rootChild0.LayoutGetWidth());
        AssertFloatEqual(72, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMargin_auto_left_fix_right_child_bigger_than_parent()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetWidth(52);
        root.StyleSetHeight(52);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMarginAuto(Edge.Left);
        rootChild0.StyleSetMargin(Edge.Right, 10);
        rootChild0.StyleSetWidth(72);
        rootChild0.StyleSetHeight(72);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(52, root.LayoutGetWidth());
        AssertFloatEqual(52, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-10, rootChild0.LayoutGetTop());
        AssertFloatEqual(72, rootChild0.LayoutGetWidth());
        AssertFloatEqual(72, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(52, root.LayoutGetWidth());
        AssertFloatEqual(52, root.LayoutGetHeight());

        AssertFloatEqual(-30, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-10, rootChild0.LayoutGetTop());
        AssertFloatEqual(72, rootChild0.LayoutGetWidth());
        AssertFloatEqual(72, rootChild0.LayoutGetHeight());
    }

    private static Size MeasureMax(
        Node<TestNode.Children> node,
        float width,
        MeasureMode widthMode,
        float height,
        MeasureMode heightMode
    )
    {
        var measureCount = (int)((TestNode)node).Context!;
        measureCount++;
        ((TestNode)node).Context = measureCount;

        if (widthMode == MeasureMode.Undefined)
            width = 10;
        if (heightMode == MeasureMode.Undefined)
            height = 10;
        return new Size(width, height);
    }

    private static Size MeasureMin(
        Node<TestNode.Children> node,
        float width,
        MeasureMode widthMode,
        float height,
        MeasureMode heightMode
    )
    {
        var measureCount = (int)((TestNode)node).Context!;
        measureCount++;
        ((TestNode)node).Context = measureCount;

        if (widthMode == MeasureMode.Undefined || (widthMode == MeasureMode.AtMost && width > 10))
            width = 10;
        if (heightMode == MeasureMode.Undefined || (heightMode == MeasureMode.AtMost && height > 10))
            height = 10;
        return new Size(width, height);
    }

    private static Size Measure8449(
        Node<TestNode.Children> node,
        float width,
        MeasureMode widthMode,
        float height,
        MeasureMode heightMode
    )
    {
        if (((TestNode)node).Context != null)
        {
            var measureCount = 1;
            ((TestNode)node).Context = measureCount;
        }

        return new Size(84, 49);
    }

    [Test]
    public void TestMeasure_once_single_flexible_child()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        var measureCount = 0;
        rootChild0.Context = measureCount;
        rootChild0.SetMeasureFunc(MeasureMax);
        rootChild0.StyleSetFlexGrow(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        measureCount = (int)rootChild0.Context;
        AssertEqual(1, measureCount);
    }

    [Test]
    public void TestRemeasure_with_same_exact_width_larger_than_needed_height()
    {
        var root = new TestNode();

        var rootChild0 = new TestNode();
        var measureCount = 0;
        rootChild0.Context = measureCount;
        rootChild0.SetMeasureFunc(MeasureMin);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, 100, 100, Direction.LeftToRight);
        Flex.CalculateLayout(root, 100, 50, Direction.LeftToRight);

        measureCount = (int)rootChild0.Context;
        AssertEqual(1, measureCount);
    }

    [Test]
    public void TestRemeasure_with_same_atmost_width_larger_than_needed_height()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);

        var rootChild0 = new TestNode();
        var measureCount = 0;
        rootChild0.Context = measureCount;
        rootChild0.SetMeasureFunc(MeasureMin);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, 100, 100, Direction.LeftToRight);
        Flex.CalculateLayout(root, 100, 50, Direction.LeftToRight);

        measureCount = (int)rootChild0.Context;
        AssertEqual(1, measureCount);
    }

    [Test]
    public void TestRemeasure_with_computed_width_larger_than_needed_height()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);

        var rootChild0 = new TestNode();
        var measureCount = 0;
        rootChild0.Context = measureCount;
        rootChild0.SetMeasureFunc(MeasureMin);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, 100, 100, Direction.LeftToRight);
        root.StyleSetAlignItems(Align.Stretch);
        Flex.CalculateLayout(root, 10, 50, Direction.LeftToRight);

        measureCount = (int)rootChild0.Context;
        AssertEqual(1, measureCount);
    }

    [Test]
    public void TestRemeasure_with_atmost_computed_width_undefined_height()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);

        var rootChild0 = new TestNode();
        var measureCount = 0;
        rootChild0.Context = measureCount;
        rootChild0.SetMeasureFunc(MeasureMin);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, 100, float.NaN, Direction.LeftToRight);
        Flex.CalculateLayout(root, 10, float.NaN, Direction.LeftToRight);

        measureCount = (int)rootChild0.Context;
        AssertEqual(1, measureCount);
    }

    [Test]
    public void TestRemeasure_with_already_measured_value_smaller_but_still_float_equal()
    {
        var measureCount = 0;

        var root = new TestNode();
        root.StyleSetWidth(288);
        root.StyleSetHeight(288);
        root.StyleSetFlexDirection(FlexDirection.Row);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPadding(Edge.All, 2.88f);
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode { Context = measureCount };
        rootChild0Child0.SetMeasureFunc(Measure8449);
        rootChild0.InsertChild(rootChild0Child0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        measureCount = (int)rootChild0Child0.Context;
        AssertEqual(1, measureCount);
    }

    private struct TestMeasureConstraint
    {
        public readonly float Width;
        public readonly MeasureMode WidthMode;
        public readonly float Height;
        public readonly MeasureMode HeightMode;

        public TestMeasureConstraint(float w, MeasureMode wMode, float h, MeasureMode hMode)
        {
            Width = w;
            WidthMode = wMode;
            Height = h;
            HeightMode = hMode;
        }
    }

    private static List<TestMeasureConstraint> CreateMeasureConstraintList(int capacity)
    {
        return new List<TestMeasureConstraint>(capacity);
    }

    private static Size _measure2(
        Node<TestNode.Children> node,
        float width,
        MeasureMode widthMode,
        float height,
        MeasureMode heightMode
    )
    {
        var constraintList = (List<TestMeasureConstraint>)((TestNode)node).Context!;
        constraintList.Add(new TestMeasureConstraint(width, widthMode, height, heightMode));
        if (widthMode == MeasureMode.Undefined)
            width = 10;

        if (heightMode == MeasureMode.Undefined)
            height = 10;
        else
            height = width; // TODO:: is it a bug in tests ?
        return new Size(width, height);
    }

    [Test]
    public void TestExactly_measure_stretched_child_column()
    {
        var constraintList = CreateMeasureConstraintList(10);

        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode { Context = constraintList };
        rootChild0.SetMeasureFunc(_measure2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(1, constraintList.Count);

        AssertFloatEqual(100, constraintList[0].Width);
        AssertEqual(MeasureMode.Exactly, constraintList[0].WidthMode);
    }

    [Test]
    public void TestExactly_measure_stretched_child_row()
    {
        var constraintList = CreateMeasureConstraintList(10);

        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode { Context = constraintList };
        rootChild0.SetMeasureFunc(_measure2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(1, constraintList.Count);

        AssertFloatEqual(100, constraintList[0].Height);
        AssertEqual(MeasureMode.Exactly, constraintList[0].HeightMode);
    }

    [Test]
    public void TestAt_most_main_axis_column()
    {
        var constraintList = CreateMeasureConstraintList(10);

        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode { Context = constraintList };
        rootChild0.SetMeasureFunc(_measure2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(1, constraintList.Count);

        AssertFloatEqual(100, constraintList[0].Height);
        AssertEqual(MeasureMode.AtMost, constraintList[0].HeightMode);
    }

    [Test]
    public void TestAt_most_cross_axis_column()
    {
        var constraintList = CreateMeasureConstraintList(10);

        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode { Context = constraintList };
        rootChild0.SetMeasureFunc(_measure2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(1, constraintList.Count);

        AssertFloatEqual(100, constraintList[0].Width);
        AssertEqual(MeasureMode.AtMost, constraintList[0].WidthMode);
    }

    [Test]
    public void TestAt_most_main_axis_row()
    {
        var constraintList = CreateMeasureConstraintList(10);

        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode { Context = constraintList };
        rootChild0.SetMeasureFunc(_measure2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(1, constraintList.Count);

        AssertFloatEqual(100, constraintList[0].Width);
        AssertEqual(MeasureMode.AtMost, constraintList[0].WidthMode);
    }

    [Test]
    public void TestAt_most_cross_axis_row()
    {
        var constraintList = CreateMeasureConstraintList(10);

        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode { Context = constraintList };
        rootChild0.SetMeasureFunc(_measure2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(1, constraintList.Count);

        AssertFloatEqual(100, constraintList[0].Height);
        AssertEqual(MeasureMode.AtMost, constraintList[0].HeightMode);
    }

    [Test]
    public void TestFlex_child()
    {
        var constraintList = CreateMeasureConstraintList(10);

        var root = new TestNode();
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.Context = constraintList;
        rootChild0.SetMeasureFunc(_measure2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(2, constraintList.Count);

        AssertFloatEqual(100, constraintList[0].Height);
        AssertEqual(MeasureMode.AtMost, constraintList[0].HeightMode);

        AssertFloatEqual(100, constraintList[1].Height);
        AssertEqual(MeasureMode.Exactly, constraintList[1].HeightMode);
    }

    [Test]
    public void TestFlex_child_with_flex_basis()
    {
        var constraintList = CreateMeasureConstraintList(10);

        var root = new TestNode();
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(0);
        rootChild0.Context = constraintList;
        rootChild0.SetMeasureFunc(_measure2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(1, constraintList.Count);

        AssertFloatEqual(100, constraintList[0].Height);
        AssertEqual(MeasureMode.Exactly, constraintList[0].HeightMode);
    }

    [Test]
    public void TestOverflow_scroll_column()
    {
        var constraintList = CreateMeasureConstraintList(10);

        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetOverflow(Overflow.Scroll);
        root.StyleSetHeight(100);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode { Context = constraintList };
        rootChild0.SetMeasureFunc(_measure2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(1, constraintList.Count);

        AssertFloatEqual(100, constraintList[0].Width);
        AssertEqual(MeasureMode.AtMost, constraintList[0].WidthMode);

        AssertTrue(float.IsNaN(constraintList[0].Height));
        AssertEqual(MeasureMode.Undefined, constraintList[0].HeightMode);
    }

    [Test]
    public void TestOverflow_scroll_row()
    {
        var constraintList = CreateMeasureConstraintList(10);

        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetOverflow(Overflow.Scroll);
        root.StyleSetHeight(100);
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode { Context = constraintList };
        rootChild0.SetMeasureFunc(_measure2);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(1, constraintList.Count);

        AssertTrue(float.IsNaN(constraintList[0].Width));
        AssertEqual(MeasureMode.Undefined, constraintList[0].WidthMode);

        AssertFloatEqual(100, constraintList[0].Height);
        AssertEqual(MeasureMode.AtMost, constraintList[0].HeightMode);
    }

    [Test]
    public void TestMax_width()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMaxWidth(50);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMax_height()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetMaxHeight(50);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMin_height()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMinHeight(60);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(80, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(80, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(20, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMin_width()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMinWidth(60);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(80, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(20, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(20, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_min_max()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetWidth(100);
        root.StyleSetMinHeight(100);
        root.StyleSetMaxHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(60);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(60, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(40, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(60, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestAlign_items_min_max()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetMinWidth(100);
        root.StyleSetMaxWidth(200);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(60);
        rootChild0.StyleSetHeight(60);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(60, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(60, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestJustify_content_overflow_min_max()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetMinHeight(100);
        root.StyleSetMaxHeight(110);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(50);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(50);
        rootChild2.StyleSetHeight(50);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(110, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-20, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(30, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(80, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(110, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-20, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(30, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(80, rootChild2.LayoutGetTop());
        AssertFloatEqual(50, rootChild2.LayoutGetWidth());
        AssertFloatEqual(50, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_to_min()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetMinHeight(100);
        root.StyleSetMaxHeight(500);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexShrink(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_in_at_most_container()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexGrow(1);
        rootChild0Child0.StyleSetFlexBasis(0);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_child()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(0);
        rootChild0.StyleSetHeight(100);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_within_constrained_min_max_column()
    {
        var root = new TestNode();
        root.StyleSetMinHeight(100);
        root.StyleSetMaxHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_within_max_width()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetMaxWidth(100);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexGrow(1);
        rootChild0Child0.StyleSetHeight(20);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_within_constrained_max_width()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetMaxWidth(300);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexGrow(1);
        rootChild0Child0.StyleSetHeight(20);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(20, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_root_ignored()
    {
        var root = new TestNode();
        root.StyleSetFlexGrow(1);
        root.StyleSetWidth(100);
        root.StyleSetMinHeight(100);
        root.StyleSetMaxHeight(500);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(200);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(100);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(300, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(200, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(300, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(200, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_root_minimized()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetMinHeight(100);
        root.StyleSetMaxHeight(500);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMinHeight(100);
        rootChild0.StyleSetMaxHeight(500);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexGrow(1);
        rootChild0Child0.StyleSetFlexBasis(200);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child1 = new TestNode();
        rootChild0Child1.StyleSetHeight(100);
        rootChild0.InsertChild(rootChild0Child1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(300, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(300, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(200, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(300, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(300, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(200, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_height_maximized()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(500);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMinHeight(100);
        rootChild0.StyleSetMaxHeight(500);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexGrow(1);
        rootChild0Child0.StyleSetFlexBasis(200);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child1 = new TestNode();
        rootChild0Child1.StyleSetHeight(100);
        rootChild0.InsertChild(rootChild0Child1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(500, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(400, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(400, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(500, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(400, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(400, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_within_constrained_min_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetMinWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(50, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_within_constrained_min_column()
    {
        var root = new TestNode();
        root.StyleSetMinHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(0, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_within_constrained_max_row()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetMaxWidth(100);
        rootChild0.StyleSetHeight(100);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexShrink(1);
        rootChild0Child0.StyleSetFlexBasis(100);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child1 = new TestNode();
        rootChild0Child1.StyleSetWidth(50);
        rootChild0.InsertChild(rootChild0Child1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0Child1.LayoutGetHeight());
    }

    [Test]
    public void TestFlex_grow_within_constrained_max_column()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetMaxHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexShrink(1);
        rootChild0.StyleSetFlexBasis(100);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetHeight(50);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestChild_min_max_width_flexing()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(120);
        root.StyleSetHeight(50);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(0);
        rootChild0.StyleSetMinWidth(60);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetFlexBasisPercent(50);
        rootChild1.StyleSetMaxWidth(20);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(120, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(100, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(20, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(120, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(20, rootChild1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMin_width_overrides_width()
    {
        var root = new TestNode();
        root.StyleSetWidth(50);
        root.StyleSetMinWidth(100);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(0, root.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(0, root.LayoutGetHeight());
    }

    [Test]
    public void TestMax_width_overrides_width()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetMaxWidth(100);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(0, root.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(0, root.LayoutGetHeight());
    }

    [Test]
    public void TestMin_height_overrides_height()
    {
        var root = new TestNode();
        root.StyleSetHeight(50);
        root.StyleSetMinHeight(100);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());
    }

    [Test]
    public void TestMax_height_overrides_height()
    {
        var root = new TestNode();
        root.StyleSetHeight(200);
        root.StyleSetMaxHeight(100);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());
    }

    [Test]
    public void TestMin_max_percent_no_width_height()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMinWidthPercent(10);
        rootChild0.StyleSetMaxWidthPercent(10);
        rootChild0.StyleSetMinHeightPercent(10);
        rootChild0.StyleSetMaxHeightPercent(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestReset_layout_when_child_removed()
    {
        var root = new TestNode();

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetHeight(100);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        root.RemoveChild(rootChild0);

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertTrue(float.IsNaN(rootChild0.LayoutGetWidth()));
        AssertTrue(float.IsNaN(rootChild0.LayoutGetHeight()));
    }

    [Test]
    public void TestPadding_no_size()
    {
        var root = new TestNode();
        root.StyleSetPadding(Edge.Left, 10);
        root.StyleSetPadding(Edge.Top, 10);
        root.StyleSetPadding(Edge.Right, 10);
        root.StyleSetPadding(Edge.Bottom, 10);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(20, root.LayoutGetWidth());
        AssertFloatEqual(20, root.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(20, root.LayoutGetWidth());
        AssertFloatEqual(20, root.LayoutGetHeight());
    }

    [Test]
    public void TestPadding_container_match_child()
    {
        var root = new TestNode();
        root.StyleSetPadding(Edge.Left, 10);
        root.StyleSetPadding(Edge.Top, 10);
        root.StyleSetPadding(Edge.Right, 10);
        root.StyleSetPadding(Edge.Bottom, 10);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(30, root.LayoutGetWidth());
        AssertFloatEqual(30, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(30, root.LayoutGetWidth());
        AssertFloatEqual(30, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestPadding_flex_child()
    {
        var root = new TestNode();
        root.StyleSetPadding(Edge.Left, 10);
        root.StyleSetPadding(Edge.Top, 10);
        root.StyleSetPadding(Edge.Right, 10);
        root.StyleSetPadding(Edge.Bottom, 10);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(80, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestPadding_stretch_child()
    {
        var root = new TestNode();
        root.StyleSetPadding(Edge.Left, 10);
        root.StyleSetPadding(Edge.Top, 10);
        root.StyleSetPadding(Edge.Right, 10);
        root.StyleSetPadding(Edge.Bottom, 10);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(80, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestPadding_center_child()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetPadding(Edge.Start, 10);
        root.StyleSetPadding(Edge.End, 20);
        root.StyleSetPadding(Edge.Bottom, 20);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(40, rootChild0.LayoutGetLeft());
        AssertFloatEqual(35, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(35, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestChild_with_padding_align_end()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.End);
        root.StyleSetAlignItems(Align.End);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPadding(Edge.Left, 20);
        rootChild0.StyleSetPadding(Edge.Top, 20);
        rootChild0.StyleSetPadding(Edge.Right, 20);
        rootChild0.StyleSetPadding(Edge.Bottom, 20);
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetHeight(100);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(100, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(100, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_width_height()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidthPercent(30);
        rootChild0.StyleSetHeightPercent(30);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(60, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(140, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(60, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_position_left_top()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(400);
        root.StyleSetHeight(400);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionPercent(Edge.Left, 10);
        rootChild0.StyleSetPositionPercent(Edge.Top, 20);
        rootChild0.StyleSetWidthPercent(45);
        rootChild0.StyleSetHeightPercent(55);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(400, root.LayoutGetWidth());
        AssertFloatEqual(400, root.LayoutGetHeight());

        AssertFloatEqual(40, rootChild0.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0.LayoutGetTop());
        AssertFloatEqual(180, rootChild0.LayoutGetWidth());
        AssertFloatEqual(220, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(400, root.LayoutGetWidth());
        AssertFloatEqual(400, root.LayoutGetHeight());

        AssertFloatEqual(260, rootChild0.LayoutGetLeft());
        AssertFloatEqual(80, rootChild0.LayoutGetTop());
        AssertFloatEqual(180, rootChild0.LayoutGetWidth());
        AssertFloatEqual(220, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_position_bottom_right()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(500);
        root.StyleSetHeight(500);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionPercent(Edge.Right, 20);
        rootChild0.StyleSetPositionPercent(Edge.Bottom, 10);
        rootChild0.StyleSetWidthPercent(55);
        rootChild0.StyleSetHeightPercent(15);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(500, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(-100, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-50, rootChild0.LayoutGetTop());
        AssertFloatEqual(275, rootChild0.LayoutGetWidth());
        AssertFloatEqual(75, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(500, root.LayoutGetWidth());
        AssertFloatEqual(500, root.LayoutGetHeight());

        AssertFloatEqual(125, rootChild0.LayoutGetLeft());
        AssertFloatEqual(-50, rootChild0.LayoutGetTop());
        AssertFloatEqual(275, rootChild0.LayoutGetWidth());
        AssertFloatEqual(75, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_flex_basis()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasisPercent(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetFlexBasisPercent(25);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(125, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(125, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(75, rootChild1.LayoutGetWidth());
        AssertFloatEqual(200, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(75, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(125, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(75, rootChild1.LayoutGetWidth());
        AssertFloatEqual(200, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_flex_basis_cross()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasisPercent(50);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetFlexBasisPercent(25);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(125, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(125, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(75, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(125, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(125, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(75, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_flex_basis_cross_min_height()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMinHeightPercent(60);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(2);
        rootChild1.StyleSetMinHeightPercent(10);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(140, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(140, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(60, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(140, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(140, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(60, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_flex_basis_main_max_height()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasisPercent(10);
        rootChild0.StyleSetMaxHeightPercent(60);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(4);
        rootChild1.StyleSetFlexBasisPercent(10);
        rootChild1.StyleSetMaxHeightPercent(20);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(52, rootChild0.LayoutGetWidth());
        AssertFloatEqual(120, rootChild0.LayoutGetHeight());

        AssertFloatEqual(52, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(148, rootChild1.LayoutGetWidth());
        AssertFloatEqual(40, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(148, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(52, rootChild0.LayoutGetWidth());
        AssertFloatEqual(120, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(148, rootChild1.LayoutGetWidth());
        AssertFloatEqual(40, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_flex_basis_cross_max_height()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasisPercent(10);
        rootChild0.StyleSetMaxHeightPercent(60);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(4);
        rootChild1.StyleSetFlexBasisPercent(10);
        rootChild1.StyleSetMaxHeightPercent(20);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(120, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(120, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(40, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(120, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(120, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(40, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_flex_basis_main_max_width()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasisPercent(15);
        rootChild0.StyleSetMaxWidthPercent(60);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(4);
        rootChild1.StyleSetFlexBasisPercent(10);
        rootChild1.StyleSetMaxWidthPercent(20);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(120, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(120, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(40, rootChild1.LayoutGetWidth());
        AssertFloatEqual(200, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(120, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(40, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(40, rootChild1.LayoutGetWidth());
        AssertFloatEqual(200, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_flex_basis_cross_max_width()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasisPercent(10);
        rootChild0.StyleSetMaxWidthPercent(60);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(4);
        rootChild1.StyleSetFlexBasisPercent(15);
        rootChild1.StyleSetMaxWidthPercent(20);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(120, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(40, rootChild1.LayoutGetWidth());
        AssertFloatEqual(150, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(120, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(160, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(40, rootChild1.LayoutGetWidth());
        AssertFloatEqual(150, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_flex_basis_main_min_width()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasisPercent(15);
        rootChild0.StyleSetMinWidthPercent(60);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(4);
        rootChild1.StyleSetFlexBasisPercent(10);
        rootChild1.StyleSetMinWidthPercent(20);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(120, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(120, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(80, rootChild1.LayoutGetWidth());
        AssertFloatEqual(200, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(80, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(120, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(80, rootChild1.LayoutGetWidth());
        AssertFloatEqual(200, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_flex_basis_cross_min_width()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasisPercent(10);
        rootChild0.StyleSetMinWidthPercent(60);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(4);
        rootChild1.StyleSetFlexBasisPercent(15);
        rootChild1.StyleSetMinWidthPercent(20);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(150, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(50, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(150, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_multiple_nested_with_padding_margin_and_percentage_values()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasisPercent(10);
        rootChild0.StyleSetMargin(Edge.Left, 5);
        rootChild0.StyleSetMargin(Edge.Top, 5);
        rootChild0.StyleSetMargin(Edge.Right, 5);
        rootChild0.StyleSetMargin(Edge.Bottom, 5);
        rootChild0.StyleSetPadding(Edge.Left, 3);
        rootChild0.StyleSetPadding(Edge.Top, 3);
        rootChild0.StyleSetPadding(Edge.Right, 3);
        rootChild0.StyleSetPadding(Edge.Bottom, 3);
        rootChild0.StyleSetMinWidthPercent(60);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetMargin(Edge.Left, 5);
        rootChild0Child0.StyleSetMargin(Edge.Top, 5);
        rootChild0Child0.StyleSetMargin(Edge.Right, 5);
        rootChild0Child0.StyleSetMargin(Edge.Bottom, 5);
        rootChild0Child0.StyleSetPaddingPercent(Edge.Left, 3);
        rootChild0Child0.StyleSetPaddingPercent(Edge.Top, 3);
        rootChild0Child0.StyleSetPaddingPercent(Edge.Right, 3);
        rootChild0Child0.StyleSetPaddingPercent(Edge.Bottom, 3);
        rootChild0Child0.StyleSetWidthPercent(50);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child0Child0 = new TestNode();
        rootChild0Child0Child0.StyleSetMarginPercent(Edge.Left, 5);
        rootChild0Child0Child0.StyleSetMarginPercent(Edge.Top, 5);
        rootChild0Child0Child0.StyleSetMarginPercent(Edge.Right, 5);
        rootChild0Child0Child0.StyleSetMarginPercent(Edge.Bottom, 5);
        rootChild0Child0Child0.StyleSetPadding(Edge.Left, 3);
        rootChild0Child0Child0.StyleSetPadding(Edge.Top, 3);
        rootChild0Child0Child0.StyleSetPadding(Edge.Right, 3);
        rootChild0Child0Child0.StyleSetPadding(Edge.Bottom, 3);
        rootChild0Child0Child0.StyleSetWidthPercent(45);
        rootChild0Child0.InsertChild(rootChild0Child0Child0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(4);
        rootChild1.StyleSetFlexBasisPercent(15);
        rootChild1.StyleSetMinWidthPercent(20);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(5, rootChild0.LayoutGetLeft());
        AssertFloatEqual(5, rootChild0.LayoutGetTop());
        AssertFloatEqual(190, rootChild0.LayoutGetWidth());
        AssertFloatEqual(48, rootChild0.LayoutGetHeight());

        AssertFloatEqual(8, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(8, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(92, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(25, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(10, rootChild0Child0Child0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0Child0Child0.LayoutGetTop());
        AssertFloatEqual(36, rootChild0Child0Child0.LayoutGetWidth());
        AssertFloatEqual(6, rootChild0Child0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(58, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(142, rootChild1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(5, rootChild0.LayoutGetLeft());
        AssertFloatEqual(5, rootChild0.LayoutGetTop());
        AssertFloatEqual(190, rootChild0.LayoutGetWidth());
        AssertFloatEqual(48, rootChild0.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(8, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(92, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(25, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(46, rootChild0Child0Child0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0Child0Child0.LayoutGetTop());
        AssertFloatEqual(36, rootChild0Child0Child0.LayoutGetWidth());
        AssertFloatEqual(6, rootChild0Child0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(58, rootChild1.LayoutGetTop());
        AssertFloatEqual(200, rootChild1.LayoutGetWidth());
        AssertFloatEqual(142, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_margin_should_calculate_based_only_on_width()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetMarginPercent(Edge.Left, 10);
        rootChild0.StyleSetMarginPercent(Edge.Top, 10);
        rootChild0.StyleSetMarginPercent(Edge.Right, 10);
        rootChild0.StyleSetMarginPercent(Edge.Bottom, 10);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(10);
        rootChild0Child0.StyleSetHeight(10);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetTop());
        AssertFloatEqual(160, rootChild0.LayoutGetWidth());
        AssertFloatEqual(60, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(20, rootChild0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0.LayoutGetTop());
        AssertFloatEqual(160, rootChild0.LayoutGetWidth());
        AssertFloatEqual(60, rootChild0.LayoutGetHeight());

        AssertFloatEqual(150, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_padding_should_calculate_based_only_on_width()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetPaddingPercent(Edge.Left, 10);
        rootChild0.StyleSetPaddingPercent(Edge.Top, 10);
        rootChild0.StyleSetPaddingPercent(Edge.Right, 10);
        rootChild0.StyleSetPaddingPercent(Edge.Bottom, 10);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(10);
        rootChild0Child0.StyleSetHeight(10);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(20, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(170, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(20, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_absolute_position()
    {
        var root = new TestNode();
        root.StyleSetWidth(200);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPositionPercent(Edge.Left, 30);
        rootChild0.StyleSetPositionPercent(Edge.Top, 10);
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(60, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(60, rootChild0.LayoutGetLeft());
        AssertFloatEqual(10, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_width_height_undefined_parent_size()
    {
        var root = new TestNode();

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidthPercent(50);
        rootChild0.StyleSetHeightPercent(50);
        root.InsertChild(rootChild0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(0, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(0, root.LayoutGetWidth());
        AssertFloatEqual(0, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestPercent_within_flex_grow()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(350);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(100);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        root.InsertChild(rootChild1, 1);

        var rootChild1Child0 = new TestNode();
        rootChild1Child0.StyleSetWidthPercent(100);
        rootChild1.InsertChild(rootChild1Child0, 0);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetWidth(100);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(350, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(100, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(150, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(150, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetHeight());

        AssertFloatEqual(250, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(350, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(250, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(100, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(150, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(150, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestPercentage_container_in_wrapping_container()
    {
        var root = new TestNode();
        root.StyleSetJustifyContent(Justify.Center);
        root.StyleSetAlignItems(Align.Center);
        root.StyleSetWidth(200);
        root.StyleSetHeight(200);

        var rootChild0 = new TestNode();
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0Child0.StyleSetJustifyContent(Justify.Center);
        rootChild0Child0.StyleSetWidthPercent(100);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child0Child0 = new TestNode();
        rootChild0Child0Child0.StyleSetWidth(50);
        rootChild0Child0Child0.StyleSetHeight(50);
        rootChild0Child0.InsertChild(rootChild0Child0Child0, 0);

        var rootChild0Child0Child1 = new TestNode();
        rootChild0Child0Child1.StyleSetWidth(50);
        rootChild0Child0Child1.StyleSetHeight(50);
        rootChild0Child0.InsertChild(rootChild0Child0Child1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(75, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child0Child0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0Child0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0Child0Child1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0Child1.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child0Child1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0Child1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(200, root.LayoutGetWidth());
        AssertFloatEqual(200, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(75, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0Child0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0Child0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child0Child0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0Child1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0Child1.LayoutGetTop());
        AssertFloatEqual(50, rootChild0Child0Child1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0Child1.LayoutGetHeight());
    }

    [Test]
    public void TestPercent_absolute_position()
    {
        var root = new TestNode();
        root.StyleSetWidth(60);
        root.StyleSetHeight(50);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Row);
        rootChild0.StyleSetPositionType(PositionType.Absolute);
        rootChild0.StyleSetPositionPercent(Edge.Left, 50);
        rootChild0.StyleSetWidthPercent(100);
        rootChild0.StyleSetHeight(50);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidthPercent(100);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child1 = new TestNode();
        rootChild0Child1.StyleSetWidthPercent(100);
        rootChild0.InsertChild(rootChild0Child1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(60, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(30, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(60, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(60, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child1.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(60, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(30, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(60, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(-60, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(60, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0Child1.LayoutGetHeight());
    }

    [Test]
    public void TestRecalculate_resolvedDimonsion_onchange()
    {
        var root = new TestNode();

        var rootChild0 = new TestNode();
        rootChild0.StyleSetMinHeight(10);
        rootChild0.StyleSetMaxHeight(10);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        rootChild0.StyleSetMinHeight(float.NaN);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_value()
    {
        // Test that whole numbers are rounded to whole despite ceil/floor flags
        AssertFloatEqual(6.0f, Flex.RoundValueToPixelGrid(6.000001f, 2.0f, false, false));
        AssertFloatEqual(6.0f, Flex.RoundValueToPixelGrid(6.000001f, 2.0f, true, false));
        AssertFloatEqual(6.0f, Flex.RoundValueToPixelGrid(6.000001f, 2.0f, false, true));
        AssertFloatEqual(6.0f, Flex.RoundValueToPixelGrid(5.999999f, 2.0f, false, false));
        AssertFloatEqual(6.0f, Flex.RoundValueToPixelGrid(5.999999f, 2.0f, true, false));
        AssertFloatEqual(6.0f, Flex.RoundValueToPixelGrid(5.999999f, 2.0f, false, true));

        // Test that numbers with fraction are rounded correctly accounting for ceil/floor flags
        AssertFloatEqual(6.0f, Flex.RoundValueToPixelGrid(6.01f, 2.0f, false, false));
        AssertFloatEqual(6.5f, Flex.RoundValueToPixelGrid(6.01f, 2.0f, true, false));
        AssertFloatEqual(6.0f, Flex.RoundValueToPixelGrid(6.01f, 2.0f, false, true));
        AssertFloatEqual(6.0f, Flex.RoundValueToPixelGrid(5.99f, 2.0f, false, false));
        AssertFloatEqual(6.0f, Flex.RoundValueToPixelGrid(5.99f, 2.0f, true, false));
        AssertFloatEqual(5.5f, Flex.RoundValueToPixelGrid(5.99f, 2.0f, false, true));
    }

    [Test]
    public void TestRounding_flex_basis_flex_grow_row_width_of_100()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(33, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(33, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(34, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(67, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(33, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(67, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(33, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(33, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(34, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(33, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_flex_basis_flex_grow_row_prime_number_width()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(113);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        root.InsertChild(rootChild2, 2);

        var rootChild3 = new TestNode();
        rootChild3.StyleSetFlexGrow(1);
        root.InsertChild(rootChild3, 3);

        var rootChild4 = new TestNode();
        rootChild4.StyleSetFlexGrow(1);
        root.InsertChild(rootChild4, 4);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(113, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(23, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(23, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(22, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(45, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(23, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        AssertFloatEqual(68, rootChild3.LayoutGetLeft());
        AssertFloatEqual(0, rootChild3.LayoutGetTop());
        AssertFloatEqual(22, rootChild3.LayoutGetWidth());
        AssertFloatEqual(100, rootChild3.LayoutGetHeight());

        AssertFloatEqual(90, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(23, rootChild4.LayoutGetWidth());
        AssertFloatEqual(100, rootChild4.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(113, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(90, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(23, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(68, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(22, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(45, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(23, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        AssertFloatEqual(23, rootChild3.LayoutGetLeft());
        AssertFloatEqual(0, rootChild3.LayoutGetTop());
        AssertFloatEqual(22, rootChild3.LayoutGetWidth());
        AssertFloatEqual(100, rootChild3.LayoutGetHeight());

        AssertFloatEqual(0, rootChild4.LayoutGetLeft());
        AssertFloatEqual(0, rootChild4.LayoutGetTop());
        AssertFloatEqual(23, rootChild4.LayoutGetWidth());
        AssertFloatEqual(100, rootChild4.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_flex_basis_flex_shrink_row()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(101);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexShrink(1);
        rootChild0.StyleSetFlexBasis(100);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexBasis(25);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexBasis(25);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(101, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(51, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(51, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(25, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(76, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(25, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(101, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(50, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(51, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(25, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(25, rootChild1.LayoutGetWidth());
        AssertFloatEqual(100, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(25, rootChild2.LayoutGetWidth());
        AssertFloatEqual(100, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_flex_basis_overrides_main_size()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(113);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(50);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(113, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(64, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(64, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(25, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(24, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(113, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(64, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(64, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(25, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(24, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_total_fractial()
    {
        var root = new TestNode();
        root.StyleSetWidth(87.4f);
        root.StyleSetHeight(113.4f);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(0.7f);
        rootChild0.StyleSetFlexBasis(50.3f);
        rootChild0.StyleSetHeight(20.3f);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1.6f);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1.1f);
        rootChild2.StyleSetHeight(10.7f);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(87, root.LayoutGetWidth());
        AssertFloatEqual(113, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(87, rootChild0.LayoutGetWidth());
        AssertFloatEqual(59, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(59, rootChild1.LayoutGetTop());
        AssertFloatEqual(87, rootChild1.LayoutGetWidth());
        AssertFloatEqual(30, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(87, rootChild2.LayoutGetWidth());
        AssertFloatEqual(24, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(87, root.LayoutGetWidth());
        AssertFloatEqual(113, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(87, rootChild0.LayoutGetWidth());
        AssertFloatEqual(59, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(59, rootChild1.LayoutGetTop());
        AssertFloatEqual(87, rootChild1.LayoutGetWidth());
        AssertFloatEqual(30, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(87, rootChild2.LayoutGetWidth());
        AssertFloatEqual(24, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_total_fractial_nested()
    {
        var root = new TestNode();
        root.StyleSetWidth(87.4f);
        root.StyleSetHeight(113.4f);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(0.7f);
        rootChild0.StyleSetFlexBasis(50.3f);
        rootChild0.StyleSetHeight(20.3f);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetFlexGrow(1);
        rootChild0Child0.StyleSetFlexBasis(0.3f);
        rootChild0Child0.StyleSetPosition(Edge.Bottom, 13.3f);
        rootChild0Child0.StyleSetHeight(9.9f);
        rootChild0.InsertChild(rootChild0Child0, 0);

        var rootChild0Child1 = new TestNode();
        rootChild0Child1.StyleSetFlexGrow(4);
        rootChild0Child1.StyleSetFlexBasis(0.3f);
        rootChild0Child1.StyleSetPosition(Edge.Top, 13.3f);
        rootChild0Child1.StyleSetHeight(1.1f);
        rootChild0.InsertChild(rootChild0Child1, 1);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1.6f);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1.1f);
        rootChild2.StyleSetHeight(10.7f);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(87, root.LayoutGetWidth());
        AssertFloatEqual(113, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(87, rootChild0.LayoutGetWidth());
        AssertFloatEqual(59, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(-13, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(87, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(12, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(25, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(87, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(47, rootChild0Child1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(59, rootChild1.LayoutGetTop());
        AssertFloatEqual(87, rootChild1.LayoutGetWidth());
        AssertFloatEqual(30, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(87, rootChild2.LayoutGetWidth());
        AssertFloatEqual(24, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(87, root.LayoutGetWidth());
        AssertFloatEqual(113, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(87, rootChild0.LayoutGetWidth());
        AssertFloatEqual(59, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(-13, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(87, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(12, rootChild0Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child1.LayoutGetLeft());
        AssertFloatEqual(25, rootChild0Child1.LayoutGetTop());
        AssertFloatEqual(87, rootChild0Child1.LayoutGetWidth());
        AssertFloatEqual(47, rootChild0Child1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(59, rootChild1.LayoutGetTop());
        AssertFloatEqual(87, rootChild1.LayoutGetWidth());
        AssertFloatEqual(30, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(87, rootChild2.LayoutGetWidth());
        AssertFloatEqual(24, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_fractial_input_1()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(113.4f);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(50);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(113, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(64, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(64, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(25, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(24, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(113, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(64, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(64, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(25, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(24, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_fractial_input_2()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(113.6f);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(50);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(114, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(65, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(65, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(24, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(25, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(114, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(65, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(65, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(24, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(25, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_fractial_input_3()
    {
        var root = new TestNode();
        root.StyleSetPosition(Edge.Top, 0.3f);
        root.StyleSetWidth(100);
        root.StyleSetHeight(113.4f);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(50);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(114, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(65, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(64, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(24, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(25, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(114, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(65, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(64, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(24, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(25, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_fractial_input_4()
    {
        var root = new TestNode();
        root.StyleSetPosition(Edge.Top, 0.7f);
        root.StyleSetWidth(100);
        root.StyleSetHeight(113.4f);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexBasis(50);
        rootChild0.StyleSetHeight(20);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(1, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(113, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(64, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(64, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(25, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(24, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(1, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(113, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(64, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(64, rootChild1.LayoutGetTop());
        AssertFloatEqual(100, rootChild1.LayoutGetWidth());
        AssertFloatEqual(25, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(89, rootChild2.LayoutGetTop());
        AssertFloatEqual(100, rootChild2.LayoutGetWidth());
        AssertFloatEqual(24, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_inner_node_controversy_horizontal()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(320);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetHeight(10);
        root.InsertChild(rootChild1, 1);

        var rootChild1Child0 = new TestNode();
        rootChild1Child0.StyleSetFlexGrow(1);
        rootChild1Child0.StyleSetHeight(10);
        rootChild1.InsertChild(rootChild1Child0, 0);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        rootChild2.StyleSetHeight(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(320, root.LayoutGetWidth());
        AssertFloatEqual(10, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(107, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(107, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(106, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(106, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1Child0.LayoutGetHeight());

        AssertFloatEqual(213, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(107, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(320, root.LayoutGetWidth());
        AssertFloatEqual(10, root.LayoutGetHeight());

        AssertFloatEqual(213, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(107, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(107, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(106, rootChild1.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(106, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild1Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(107, rootChild2.LayoutGetWidth());
        AssertFloatEqual(10, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_inner_node_controversy_vertical()
    {
        var root = new TestNode();
        root.StyleSetHeight(320);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetWidth(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetWidth(10);
        root.InsertChild(rootChild1, 1);

        var rootChild1Child0 = new TestNode();
        rootChild1Child0.StyleSetFlexGrow(1);
        rootChild1Child0.StyleSetWidth(10);
        rootChild1.InsertChild(rootChild1Child0, 0);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        rootChild2.StyleSetWidth(10);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(10, root.LayoutGetWidth());
        AssertFloatEqual(320, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(107, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(107, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(106, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(10, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(106, rootChild1Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(213, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(107, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(10, root.LayoutGetWidth());
        AssertFloatEqual(320, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(107, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(107, rootChild1.LayoutGetTop());
        AssertFloatEqual(10, rootChild1.LayoutGetWidth());
        AssertFloatEqual(106, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(10, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(106, rootChild1Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(213, rootChild2.LayoutGetTop());
        AssertFloatEqual(10, rootChild2.LayoutGetWidth());
        AssertFloatEqual(107, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestRounding_inner_node_controversy_combined()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(640);
        root.StyleSetHeight(320);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetHeightPercent(100);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetFlexGrow(1);
        rootChild1.StyleSetHeightPercent(100);
        root.InsertChild(rootChild1, 1);

        var rootChild1Child0 = new TestNode();
        rootChild1Child0.StyleSetFlexGrow(1);
        rootChild1Child0.StyleSetWidthPercent(100);
        rootChild1.InsertChild(rootChild1Child0, 0);

        var rootChild1Child1 = new TestNode();
        rootChild1Child1.StyleSetFlexGrow(1);
        rootChild1Child1.StyleSetWidthPercent(100);
        rootChild1.InsertChild(rootChild1Child1, 1);

        var rootChild1Child1Child0 = new TestNode();
        rootChild1Child1Child0.StyleSetFlexGrow(1);
        rootChild1Child1Child0.StyleSetWidthPercent(100);
        rootChild1Child1.InsertChild(rootChild1Child1Child0, 0);

        var rootChild1Child2 = new TestNode();
        rootChild1Child2.StyleSetFlexGrow(1);
        rootChild1Child2.StyleSetWidthPercent(100);
        rootChild1.InsertChild(rootChild1Child2, 2);

        var rootChild2 = new TestNode();
        rootChild2.StyleSetFlexGrow(1);
        rootChild2.StyleSetHeightPercent(100);
        root.InsertChild(rootChild2, 2);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(640, root.LayoutGetWidth());
        AssertFloatEqual(320, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(213, rootChild0.LayoutGetWidth());
        AssertFloatEqual(320, rootChild0.LayoutGetHeight());

        AssertFloatEqual(213, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(214, rootChild1.LayoutGetWidth());
        AssertFloatEqual(320, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(214, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(107, rootChild1Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child1.LayoutGetLeft());
        AssertFloatEqual(107, rootChild1Child1.LayoutGetTop());
        AssertFloatEqual(214, rootChild1Child1.LayoutGetWidth());
        AssertFloatEqual(106, rootChild1Child1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child1Child0.LayoutGetTop());
        AssertFloatEqual(214, rootChild1Child1Child0.LayoutGetWidth());
        AssertFloatEqual(106, rootChild1Child1Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child2.LayoutGetLeft());
        AssertFloatEqual(213, rootChild1Child2.LayoutGetTop());
        AssertFloatEqual(214, rootChild1Child2.LayoutGetWidth());
        AssertFloatEqual(107, rootChild1Child2.LayoutGetHeight());

        AssertFloatEqual(427, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(213, rootChild2.LayoutGetWidth());
        AssertFloatEqual(320, rootChild2.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(640, root.LayoutGetWidth());
        AssertFloatEqual(320, root.LayoutGetHeight());

        AssertFloatEqual(427, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(213, rootChild0.LayoutGetWidth());
        AssertFloatEqual(320, rootChild0.LayoutGetHeight());

        AssertFloatEqual(213, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(214, rootChild1.LayoutGetWidth());
        AssertFloatEqual(320, rootChild1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child0.LayoutGetTop());
        AssertFloatEqual(214, rootChild1Child0.LayoutGetWidth());
        AssertFloatEqual(107, rootChild1Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child1.LayoutGetLeft());
        AssertFloatEqual(107, rootChild1Child1.LayoutGetTop());
        AssertFloatEqual(214, rootChild1Child1.LayoutGetWidth());
        AssertFloatEqual(106, rootChild1Child1.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child1Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1Child1Child0.LayoutGetTop());
        AssertFloatEqual(214, rootChild1Child1Child0.LayoutGetWidth());
        AssertFloatEqual(106, rootChild1Child1Child0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1Child2.LayoutGetLeft());
        AssertFloatEqual(213, rootChild1Child2.LayoutGetTop());
        AssertFloatEqual(214, rootChild1Child2.LayoutGetWidth());
        AssertFloatEqual(107, rootChild1Child2.LayoutGetHeight());

        AssertFloatEqual(0, rootChild2.LayoutGetLeft());
        AssertFloatEqual(0, rootChild2.LayoutGetTop());
        AssertFloatEqual(213, rootChild2.LayoutGetWidth());
        AssertFloatEqual(320, rootChild2.LayoutGetHeight());
    }

    [Test]
    public void TestNested_overflowing_child()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(200);
        rootChild0Child0.StyleSetHeight(200);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(-100, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestNested_overflowing_child_in_constraint_parent()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(100);
        rootChild0.StyleSetHeight(100);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(200);
        rootChild0Child0.StyleSetHeight(200);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(100, rootChild0.LayoutGetHeight());

        AssertFloatEqual(-100, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestParent_wrap_child_size_overflowing_parent()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetWidth(100);
        root.InsertChild(rootChild0, 0);

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetWidth(100);
        rootChild0Child0.StyleSetHeight(200);
        rootChild0.InsertChild(rootChild0Child0, 0);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetHeight());

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.RightToLeft);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(100, root.LayoutGetWidth());
        AssertFloatEqual(100, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0Child0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0Child0.LayoutGetTop());
        AssertFloatEqual(100, rootChild0Child0.LayoutGetWidth());
        AssertFloatEqual(200, rootChild0Child0.LayoutGetHeight());
    }

    [Test]
    public void TestCopy_style_same()
    {
        var node0 = new TestNode();
        var node1 = new TestNode();
        AssertFalse(node0.IsDirty);

        Flex.NodeCopyStyle(node0, node1);
        AssertFalse(node0.IsDirty);
    }

    [Test]
    public void TestCopy_style_modified()
    {
        var node0 = new TestNode();
        AssertFalse(node0.IsDirty);
        AssertEqual(FlexDirection.Column, node0.StyleGetFlexDirection());
        AssertFalse(node0.StyleGetMaxHeight().Unit != Unit.Undefined);

        var node1 = new TestNode();
        node1.StyleSetFlexDirection(FlexDirection.Row);
        node1.StyleSetMaxHeight(10);

        Flex.NodeCopyStyle(node0, node1);
        AssertTrue(node0.IsDirty);
        AssertEqual(FlexDirection.Row, node0.StyleGetFlexDirection());
        AssertFloatEqual(10, node0.StyleGetMaxHeight().Number);
    }

    [Test]
    public void TestCopy_style_modified_same()
    {
        var node0 = new TestNode();
        node0.StyleSetFlexDirection(FlexDirection.Row);
        node0.StyleSetMaxHeight(10);
        Flex.CalculateLayout(node0, float.NaN, float.NaN, Direction.LeftToRight);
        AssertFalse(node0.IsDirty);

        var node1 = new TestNode();
        node1.StyleSetFlexDirection(FlexDirection.Row);
        node1.StyleSetMaxHeight(10);

        Flex.NodeCopyStyle(node0, node1);
        AssertFalse(node0.IsDirty);
    }

    private class TestInteger
    {
        public int Count = 0;

        public void Increase()
        {
            Count++;
        }
    }

    private static Size _measure3(
        Node<TestNode.Children> node,
        float width,
        MeasureMode widthMode,
        float height,
        MeasureMode heightMode
    )
    {
        if (((TestNode)node).Context != null)
        {
            var ti = (TestInteger)((TestNode)node).Context!;
            ti.Increase();
        }

        return new Size(10, 10);
    }

    private static Size _simulate_wrapping_text(
        Node<TestNode.Children> node,
        float width,
        MeasureMode widthMode,
        float height,
        MeasureMode heightMode
    )
    {
        if (widthMode == MeasureMode.Undefined || width >= 68)
            return new Size(68, 16);
        return new Size(50, 32);
    }

    private static Size _measure_assert_negative(
        Node<TestNode.Children> node,
        float width,
        MeasureMode widthMode,
        float height,
        MeasureMode heightMode
    )
    {
        if (width < 0)
            throw new Exception($"width is {width} and should be >= 0");
        if (height < 0)
            throw new Exception($"height is {height} should be >= 0, height");
        // EXPECT_GE(width, 0);
        //EXPECT_GE(height, 0);

        return new Size(0, 0);
    }

    [Test]
    public void TestDont_measure_single_grow_shrink_child()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var measureIntegerCount = new TestInteger();

        var rootChild0 = new TestNode { Context = measureIntegerCount };
        rootChild0.SetMeasureFunc(_measure);
        rootChild0.StyleSetFlexGrow(1);
        rootChild0.StyleSetFlexShrink(1);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(0, measureIntegerCount.Count);
    }

    [Test]
    public void TestMeasure_absolute_child_with_no_constraints()
    {
        var root = new TestNode();

        var rootChild0 = new TestNode();
        root.InsertChild(rootChild0, 0);

        var measureIntegerCount = new TestInteger();

        var rootChild0Child0 = new TestNode();
        rootChild0Child0.StyleSetPositionType(PositionType.Absolute);
        rootChild0Child0.Context = measureIntegerCount;
        rootChild0Child0.SetMeasureFunc(_measure3);
        rootChild0.InsertChild(rootChild0Child0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(1, measureIntegerCount.Count);
    }

    [Test]
    public void TestDont_measure_when_min_equals_max()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var measureIntegerCount = new TestInteger();

        var rootChild0 = new TestNode { Context = measureIntegerCount };
        rootChild0.SetMeasureFunc(_measure3);
        rootChild0.StyleSetMinWidth(10);
        rootChild0.StyleSetMaxWidth(10);
        rootChild0.StyleSetMinHeight(10);
        rootChild0.StyleSetMaxHeight(10);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(0, measureIntegerCount.Count);
        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestDont_measure_when_min_equals_max_percentages()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var measureIntegerCount = new TestInteger();

        var rootChild0 = new TestNode { Context = measureIntegerCount };
        rootChild0.SetMeasureFunc(_measure3);
        rootChild0.StyleSetMinWidthPercent(10);
        rootChild0.StyleSetMaxWidthPercent(10);
        rootChild0.StyleSetMinHeightPercent(10);
        rootChild0.StyleSetMaxHeightPercent(10);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(0, measureIntegerCount.Count);
        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestDont_measure_when_min_equals_max_mixed_width_percent()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var measureIntegerCount = new TestInteger();

        var rootChild0 = new TestNode { Context = measureIntegerCount };
        rootChild0.SetMeasureFunc(_measure3);
        rootChild0.StyleSetMinWidthPercent(10);
        rootChild0.StyleSetMaxWidthPercent(10);
        rootChild0.StyleSetMinHeight(10);
        rootChild0.StyleSetMaxHeight(10);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(0, measureIntegerCount.Count);
        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestDont_measure_when_min_equals_max_mixed_height_percent()
    {
        var root = new TestNode();
        root.StyleSetAlignItems(Align.Start);
        root.StyleSetWidth(100);
        root.StyleSetHeight(100);

        var measureIntegerCount = new TestInteger();

        var rootChild0 = new TestNode { Context = measureIntegerCount };
        rootChild0.SetMeasureFunc(_measure3);
        rootChild0.StyleSetMinWidth(10);
        rootChild0.StyleSetMaxWidth(10);
        rootChild0.StyleSetMinHeightPercent(10);
        rootChild0.StyleSetMaxHeightPercent(10);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertEqual(0, measureIntegerCount.Count);
        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMeasure_enough_size_should_be_in_single_line()
    {
        var root = new TestNode();
        root.StyleSetWidth(100);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAlignSelf(Align.Start);
        rootChild0.SetMeasureFunc(_simulate_wrapping_text);

        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(68, rootChild0.LayoutGetWidth());
        AssertFloatEqual(16, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMeasure_not_enough_size_should_wrap()
    {
        var root = new TestNode();
        root.StyleSetWidth(55);

        var rootChild0 = new TestNode();
        rootChild0.StyleSetAlignSelf(Align.Start);
        rootChild0.SetMeasureFunc(_simulate_wrapping_text);

        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(32, rootChild0.LayoutGetHeight());
    }

    [Test]
    public void TestMeasure_zero_space_should_grow()
    {
        var root = new TestNode();
        root.StyleSetHeight(200);
        root.StyleSetFlexDirection(FlexDirection.Column);
        root.StyleSetFlexGrow(0);

        var measureIntegerCount = new TestInteger();

        var rootChild0 = new TestNode();
        rootChild0.StyleSetFlexDirection(FlexDirection.Column);
        rootChild0.StyleSetPadding(Edge.All, 100);
        rootChild0.Context = measureIntegerCount;
        rootChild0.SetMeasureFunc(_measure3);

        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, 282, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(282, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
    }

    [Test]
    public void TestMeasure_flex_direction_row_and_padding()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetPadding(Edge.Left, 25);
        root.StyleSetPadding(Edge.Top, 25);
        root.StyleSetPadding(Edge.Right, 25);
        root.StyleSetPadding(Edge.Bottom, 25);
        root.StyleSetWidth(50);
        root.StyleSetHeight(50);

        var rootChild0 = new TestNode();
        rootChild0.SetMeasureFunc(_simulate_wrapping_text);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(5);
        rootChild1.StyleSetHeight(5);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(0, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(25, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(75, rootChild1.LayoutGetLeft());
        AssertFloatEqual(25, rootChild1.LayoutGetTop());
        AssertFloatEqual(5, rootChild1.LayoutGetWidth());
        AssertFloatEqual(5, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMeasure_flex_direction_column_and_padding()
    {
        var root = new TestNode();
        root.StyleSetMargin(Edge.Top, 20);
        root.StyleSetPadding(Edge.All, 25);
        root.StyleSetWidth(50);
        root.StyleSetHeight(50);

        var rootChild0 = new TestNode();
        rootChild0.SetMeasureFunc(_simulate_wrapping_text);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(5);
        rootChild1.StyleSetHeight(5);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(20, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(25, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(32, rootChild0.LayoutGetHeight());

        AssertFloatEqual(25, rootChild1.LayoutGetLeft());
        AssertFloatEqual(57, rootChild1.LayoutGetTop());
        AssertFloatEqual(5, rootChild1.LayoutGetWidth());
        AssertFloatEqual(5, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMeasure_flex_direction_row_no_padding()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetMargin(Edge.Top, 20);
        root.StyleSetWidth(50);
        root.StyleSetHeight(50);

        var rootChild0 = new TestNode();
        rootChild0.SetMeasureFunc(_simulate_wrapping_text);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(5);
        rootChild1.StyleSetHeight(5);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(20, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(50, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(5, rootChild1.LayoutGetWidth());
        AssertFloatEqual(5, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMeasure_flex_direction_row_no_padding_align_items_flexstart()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetMargin(Edge.Top, 20);
        root.StyleSetWidth(50);
        root.StyleSetHeight(50);
        root.StyleSetAlignItems(Align.Start);

        var rootChild0 = new TestNode();
        rootChild0.SetMeasureFunc(_simulate_wrapping_text);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(5);
        rootChild1.StyleSetHeight(5);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(20, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(32, rootChild0.LayoutGetHeight());

        AssertFloatEqual(50, rootChild1.LayoutGetLeft());
        AssertFloatEqual(0, rootChild1.LayoutGetTop());
        AssertFloatEqual(5, rootChild1.LayoutGetWidth());
        AssertFloatEqual(5, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMeasure_with_fixed_size()
    {
        var root = new TestNode();
        root.StyleSetMargin(Edge.Top, 20);
        root.StyleSetPadding(Edge.All, 25);
        root.StyleSetWidth(50);
        root.StyleSetHeight(50);

        var rootChild0 = new TestNode();
        rootChild0.SetMeasureFunc(_simulate_wrapping_text);
        rootChild0.StyleSetWidth(10);
        rootChild0.StyleSetHeight(10);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(5);
        rootChild1.StyleSetHeight(5);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(20, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(25, rootChild0.LayoutGetTop());
        AssertFloatEqual(10, rootChild0.LayoutGetWidth());
        AssertFloatEqual(10, rootChild0.LayoutGetHeight());

        AssertFloatEqual(25, rootChild1.LayoutGetLeft());
        AssertFloatEqual(35, rootChild1.LayoutGetTop());
        AssertFloatEqual(5, rootChild1.LayoutGetWidth());
        AssertFloatEqual(5, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMeasure_with_flex_shrink()
    {
        var root = new TestNode();
        root.StyleSetMargin(Edge.Top, 20);
        root.StyleSetPadding(Edge.All, 25);
        root.StyleSetWidth(50);
        root.StyleSetHeight(50);

        var rootChild0 = new TestNode();
        rootChild0.SetMeasureFunc(_simulate_wrapping_text);
        rootChild0.StyleSetFlexShrink(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(5);
        rootChild1.StyleSetHeight(5);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(20, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(25, rootChild0.LayoutGetLeft());
        AssertFloatEqual(25, rootChild0.LayoutGetTop());
        AssertFloatEqual(0, rootChild0.LayoutGetWidth());
        AssertFloatEqual(0, rootChild0.LayoutGetHeight());

        AssertFloatEqual(25, rootChild1.LayoutGetLeft());
        AssertFloatEqual(25, rootChild1.LayoutGetTop());
        AssertFloatEqual(5, rootChild1.LayoutGetWidth());
        AssertFloatEqual(5, rootChild1.LayoutGetHeight());
    }

    [Test]
    public void TestMeasure_no_padding()
    {
        var root = new TestNode();
        root.StyleSetMargin(Edge.Top, 20);
        root.StyleSetWidth(50);
        root.StyleSetHeight(50);

        var rootChild0 = new TestNode();
        rootChild0.SetMeasureFunc(_simulate_wrapping_text);
        rootChild0.StyleSetFlexShrink(1);
        root.InsertChild(rootChild0, 0);

        var rootChild1 = new TestNode();
        rootChild1.StyleSetWidth(5);
        rootChild1.StyleSetHeight(5);
        root.InsertChild(rootChild1, 1);
        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);

        AssertFloatEqual(0, root.LayoutGetLeft());
        AssertFloatEqual(20, root.LayoutGetTop());
        AssertFloatEqual(50, root.LayoutGetWidth());
        AssertFloatEqual(50, root.LayoutGetHeight());

        AssertFloatEqual(0, rootChild0.LayoutGetLeft());
        AssertFloatEqual(0, rootChild0.LayoutGetTop());
        AssertFloatEqual(50, rootChild0.LayoutGetWidth());
        AssertFloatEqual(32, rootChild0.LayoutGetHeight());

        AssertFloatEqual(0, rootChild1.LayoutGetLeft());
        AssertFloatEqual(32, rootChild1.LayoutGetTop());
        AssertFloatEqual(5, rootChild1.LayoutGetWidth());
        AssertFloatEqual(5, rootChild1.LayoutGetHeight());
    }

    /*
#if GTEST_HAS_DEATH_TEST
    TEST(YogaDeathTest, cannot_add_child_to_node_with_measure_func) {
      root := YGNodeNew();
      YGroot.SetMeasureFunc(_measure3);

      rootChild0 := YGNodeNew();
      ASSERT_DEATH(YGroot.InsertChild(rootChild0, 0), "Cannot add child.*");
      YGNodeFree(rootChild0);
      ;
    }

    TEST(YogaDeathTest, cannot_add_nonnull_measure_func_to_non_leaf_node) {
      root := YGNodeNew();
      rootChild0 := YGNodeNew();
      YGroot.InsertChild(rootChild0, 0);

      ASSERT_DEATH(YGroot.SetMeasureFunc(_measure3), "Cannot set measure function.*");
      ;
    }
#endif
    */

    [Test]
    public void TestCan_nullify_measure_func_on_any_node()
    {
        var root = new TestNode();
        root.InsertChild(new TestNode(), 0);

        root.SetMeasureFunc(null);
        AssertTrue(root.GetMeasureFunc() == null);
    }

    [Test]
    public void TestCant_call_negative_measure()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Column);
        root.StyleSetWidth(50);
        root.StyleSetHeight(10);

        var rootChild0 = new TestNode();
        rootChild0.SetMeasureFunc(_measure_assert_negative);
        rootChild0.StyleSetMargin(Edge.Top, 20);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
    }

    [Test]
    public void TestCant_call_negative_measure_horizontal()
    {
        var root = new TestNode();
        root.StyleSetFlexDirection(FlexDirection.Row);
        root.StyleSetWidth(10);
        root.StyleSetHeight(20);

        var rootChild0 = new TestNode();
        rootChild0.SetMeasureFunc(_measure_assert_negative);
        rootChild0.StyleSetMargin(Edge.Start, 20);
        root.InsertChild(rootChild0, 0);

        Flex.CalculateLayout(root, float.NaN, float.NaN, Direction.LeftToRight);
    }

    public void RunTest()
    {
        var m = typeof(TestUnit).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var method in m)
            if (method.Name.StartsWith("Test"))
                try
                {
                    Console.Write($"Test: {method.Name} -> ");
                    method.Invoke(this, null);
                    Console.WriteLine("passed");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.StackTrace}");
                }
            else
                Console.WriteLine($"ignore: {method.Name}");
    }
}
