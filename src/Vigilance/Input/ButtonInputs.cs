using System.Runtime.CompilerServices;
using Vigilance.Collections;
using ZLinq;

namespace Vigilance.Input;

[CollectionBuilder(typeof(ButtonInputsBuilder), nameof(ButtonInputsBuilder.Create))]
public sealed class ButtonInputs : IList<Button>, IValueListView<Button>
{
    private ValueList<Button> _buttons = [];

    public bool IsDown => _buttons.AsValueEnumerable().Any(button => button.IsDown);

    public bool IsUp => !IsDown;

    public bool IsPressed => _buttons.AsValueEnumerable().Any(button => button.IsPressed);

    public bool IsReleased => _buttons.AsValueEnumerable().Any(button => button.IsReleased);

    public void Add(Button item)
    {
        _buttons.Add(item);
    }

    public void Clear()
    {
        _buttons.Clear();
    }

    public bool Contains(Button item)
    {
        return _buttons.Contains(item);
    }

    public void CopyTo(Button[] array, int arrayIndex)
    {
        _buttons.CopyTo(array, arrayIndex);
    }

    public bool Remove(Button item)
    {
        return _buttons.Remove(item);
    }

    public bool IsReadOnly => false;

    public int IndexOf(Button item)
    {
        return _buttons.IndexOf(item);
    }

    public void Insert(int index, Button item)
    {
        _buttons.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _buttons.RemoveAt(index);
    }

    public int Count => _buttons.Count;

    public Button this[int index]
    {
        get => _buttons[index];
        set => _buttons[index] = value;
    }

    public ValueList<Button>.Enumerator GetEnumerator()
    {
        return _buttons.GetEnumerator();
    }

    public ValueEnumerable<ValueList<Button>.Enumerator, Button> AsValueEnumerable()
    {
        return _buttons.AsValueEnumerable();
    }

    public static implicit operator ButtonInputs(Key key)
    {
        return (Button)key;
    }

    public static implicit operator ButtonInputs(MouseButton mouseButton)
    {
        return (Button)mouseButton;
    }

    public static implicit operator ButtonInputs(GamepadButton gamepadButton)
    {
        return (Button)gamepadButton;
    }

    public static implicit operator ButtonInputs(Button button)
    {
        return new ButtonInputs { _buttons = [button] };
    }

    public static implicit operator ButtonInputs(in ReadOnlySpan<Button> buttons)
    {
        return new ButtonInputs { _buttons = buttons.AsValueEnumerable().ToValueList() };
    }
}

public static class ButtonInputsBuilder
{
    public static ButtonInputs Create(ReadOnlySpan<Button> buttons)
    {
        return buttons;
    }
}
