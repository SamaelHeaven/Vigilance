using System.Runtime.CompilerServices;
using Vigilance.Collections;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Input;

[CollectionBuilder(typeof(AxisInputsBuilder), nameof(AxisInputsBuilder.Create))]
public sealed class AxisInputs : IList<Axis>, IValueListView<Axis>
{
    private ValueList<Axis> _axes = [];

    public int Position
    {
        get
        {
            var negative = _axes.AsValueEnumerable().Any(axis => axis.Position < 0);
            var positive = _axes.AsValueEnumerable().Any(axis => axis.Position > 0);
            if (negative && !positive)
                return -1;
            if (positive && !negative)
                return 1;
            return 0;
        }
    }

    public float Magnitude
    {
        get
        {
            var negative = _axes
                .AsValueEnumerable()
                .Select(axis => axis.Magnitude)
                .Where(value => value < 0)
                .Prepend(0)
                .Min();
            var positive = _axes
                .AsValueEnumerable()
                .Select(axis => axis.Magnitude)
                .Where(value => value > 0)
                .Prepend(0)
                .Max();
            if (negative.Abs() > positive.Abs())
                return negative;
            if (positive.Abs() > negative.Abs())
                return positive;
            return 0;
        }
    }

    public float RawMagnitude
    {
        get
        {
            var negative = _axes
                .AsValueEnumerable()
                .Select(axis => axis.RawMagnitude)
                .Where(value => value < 0)
                .Prepend(0)
                .Min();
            var positive = _axes
                .AsValueEnumerable()
                .Select(axis => axis.RawMagnitude)
                .Where(value => value > 0)
                .Prepend(0)
                .Max();
            if (negative.Abs() > positive.Abs())
                return negative;
            if (positive.Abs() > negative.Abs())
                return positive;
            return 0;
        }
    }

    public void Add(Axis item)
    {
        _axes.Add(item);
    }

    public void Clear()
    {
        _axes.Clear();
    }

    public bool Contains(Axis item)
    {
        return _axes.Contains(item);
    }

    public void CopyTo(Axis[] array, int arrayIndex)
    {
        _axes.CopyTo(array, arrayIndex);
    }

    public bool Remove(Axis item)
    {
        return _axes.Remove(item);
    }

    public int Count => _axes.Count;

    public bool IsReadOnly => false;

    public int IndexOf(Axis item)
    {
        return _axes.IndexOf(item);
    }

    public void Insert(int index, Axis item)
    {
        _axes.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _axes.RemoveAt(index);
    }

    public Axis this[int index]
    {
        get => _axes[index];
        set => _axes[index] = value;
    }

    public ValueList<Axis>.Enumerator GetEnumerator()
    {
        return _axes.GetEnumerator();
    }

    public ValueEnumerable<ValueList<Axis>.Enumerator, Axis> AsValueEnumerable()
    {
        return _axes.AsValueEnumerable();
    }

    public static implicit operator AxisInputs(Axis axis)
    {
        return new AxisInputs { _axes = [axis] };
    }

    public static implicit operator AxisInputs(in ReadOnlySpan<Axis> axes)
    {
        return new AxisInputs { _axes = axes.AsValueEnumerable().ToValueList() };
    }
}

public static class AxisInputsBuilder
{
    public static AxisInputs Create(ReadOnlySpan<Axis> axes)
    {
        return axes;
    }
}
