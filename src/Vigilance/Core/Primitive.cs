using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Box2D.NET;

namespace Vigilance.Core;

[StructLayout(LayoutKind.Explicit)]
[Union]
public readonly record struct Primitive : IUnion
{
    private string InvalidMessage => $"{nameof(Primitive)} contains a {Type}.";

    public static Primitive None => new() { Type = PrimitiveType.None };

    [field: FieldOffset(0)]
    public bool Bool
    {
        get => Type == PrimitiveType.Bool ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.Bool ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public sbyte SByte
    {
        get => Type == PrimitiveType.SByte ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.SByte ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public byte Byte
    {
        get => Type == PrimitiveType.Byte ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.Byte ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public short Short
    {
        get => Type == PrimitiveType.Short ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.Short ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public ushort UShort
    {
        get => Type == PrimitiveType.UShort ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.UShort ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public int Int
    {
        get => Type == PrimitiveType.Int ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.Int ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public uint UInt
    {
        get => Type == PrimitiveType.UInt ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.UInt ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public long Long
    {
        get => Type == PrimitiveType.Long ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.Long ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public ulong ULong
    {
        get => Type == PrimitiveType.ULong ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.ULong ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public float Float
    {
        get => Type == PrimitiveType.Float ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.Float ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public double Double
    {
        get => Type == PrimitiveType.Double ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.Double ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public nint NInt
    {
        get => Type == PrimitiveType.NInt ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.NInt ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(0)]
    public nuint NUInt
    {
        get => Type == PrimitiveType.NUInt ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.NUInt ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(8)]
    public object? Object
    {
        get => Type == PrimitiveType.Object ? field : throw new InvalidOperationException(InvalidMessage);
        init => field = Type == PrimitiveType.Object ? value : throw new InvalidOperationException(InvalidMessage);
    }

    [field: FieldOffset(16)]
    public PrimitiveType Type { get; init; }

    public Primitive(bool value)
    {
        Type = PrimitiveType.Bool;
        Bool = value;
    }

    public Primitive(sbyte value)
    {
        Type = PrimitiveType.SByte;
        SByte = value;
    }

    public Primitive(byte value)
    {
        Type = PrimitiveType.Byte;
        Byte = value;
    }

    public Primitive(short value)
    {
        Type = PrimitiveType.Short;
        Short = value;
    }

    public Primitive(ushort value)
    {
        Type = PrimitiveType.UShort;
        UShort = value;
    }

    public Primitive(int value)
    {
        Type = PrimitiveType.Int;
        Int = value;
    }

    public Primitive(uint value)
    {
        Type = PrimitiveType.UInt;
        UInt = value;
    }

    public Primitive(long value)
    {
        Type = PrimitiveType.Long;
        Long = value;
    }

    public Primitive(ulong value)
    {
        Type = PrimitiveType.ULong;
        ULong = value;
    }

    public Primitive(float value)
    {
        Type = PrimitiveType.Float;
        Float = value;
    }

    public Primitive(double value)
    {
        Type = PrimitiveType.Double;
        Double = value;
    }

    public Primitive(nint value)
    {
        Type = PrimitiveType.NInt;
        NInt = value;
    }

    public Primitive(nuint value)
    {
        Type = PrimitiveType.NUInt;
        NUInt = value;
    }

    public static Primitive From<T>(T value)
    {
        if (typeof(T) == typeof(bool))
            return new Primitive(Unsafe.As<T, bool>(ref value));
        if (typeof(T) == typeof(sbyte))
            return new Primitive(Unsafe.As<T, sbyte>(ref value));
        if (typeof(T) == typeof(byte))
            return new Primitive(Unsafe.As<T, byte>(ref value));
        if (typeof(T) == typeof(short))
            return new Primitive(Unsafe.As<T, short>(ref value));
        if (typeof(T) == typeof(ushort))
            return new Primitive(Unsafe.As<T, ushort>(ref value));
        if (typeof(T) == typeof(int))
            return new Primitive(Unsafe.As<T, int>(ref value));
        if (typeof(T) == typeof(uint))
            return new Primitive(Unsafe.As<T, uint>(ref value));
        if (typeof(T) == typeof(long))
            return new Primitive(Unsafe.As<T, long>(ref value));
        if (typeof(T) == typeof(ulong))
            return new Primitive(Unsafe.As<T, ulong>(ref value));
        if (typeof(T) == typeof(float))
            return new Primitive(Unsafe.As<T, float>(ref value));
        if (typeof(T) == typeof(double))
            return new Primitive(Unsafe.As<T, double>(ref value));
        if (typeof(T) == typeof(nint))
            return new Primitive(Unsafe.As<T, nint>(ref value));
        return typeof(T) == typeof(nuint)
            ? new Primitive(Unsafe.As<T, nuint>(ref value))
            : new Primitive { Type = PrimitiveType.Object, Object = value };
    }

    public static implicit operator Primitive(bool value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(sbyte value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(byte value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(short value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(ushort value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(int value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(uint value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(long value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(ulong value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(float value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(double value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(nint value)
    {
        return new Primitive(value);
    }

    public static implicit operator Primitive(nuint value)
    {
        return new Primitive(value);
    }

    public object? Value
    {
        get
        {
            return Type switch
            {
                PrimitiveType.Bool => Bool,
                PrimitiveType.SByte => SByte,
                PrimitiveType.Byte => Byte,
                PrimitiveType.Short => Short,
                PrimitiveType.UShort => UShort,
                PrimitiveType.Int => Int,
                PrimitiveType.UInt => UInt,
                PrimitiveType.Long => Long,
                PrimitiveType.ULong => ULong,
                PrimitiveType.Float => Float,
                PrimitiveType.Double => Double,
                PrimitiveType.NInt => NInt,
                PrimitiveType.NUInt => NUInt,
                PrimitiveType.Object => Object,
                _ => null,
            };
        }
    }

    public Type RuntimeType => Type.RuntimeType;

    public Type UnderlyingType
    {
        get
        {
            return Type switch
            {
                PrimitiveType.Bool => typeof(bool),
                PrimitiveType.SByte => typeof(sbyte),
                PrimitiveType.Byte => typeof(byte),
                PrimitiveType.Short => typeof(short),
                PrimitiveType.UShort => typeof(ushort),
                PrimitiveType.Int => typeof(int),
                PrimitiveType.UInt => typeof(uint),
                PrimitiveType.Long => typeof(long),
                PrimitiveType.ULong => typeof(ulong),
                PrimitiveType.Float => typeof(float),
                PrimitiveType.Double => typeof(double),
                PrimitiveType.NInt => typeof(nint),
                PrimitiveType.NUInt => typeof(nuint),
                PrimitiveType.Object => Object?.GetType()!,
                _ => null!,
            };
        }
    }

    public bool TryGetValue(out bool value)
    {
        if (Type == PrimitiveType.Bool)
        {
            value = Bool;
            return true;
        }

        value = false;
        return false;
    }

    public bool TryGetValue(out sbyte value)
    {
        if (Type == PrimitiveType.SByte)
        {
            value = SByte;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out byte value)
    {
        if (Type == PrimitiveType.Byte)
        {
            value = Byte;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out short value)
    {
        if (Type == PrimitiveType.Short)
        {
            value = Short;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out ushort value)
    {
        if (Type == PrimitiveType.UShort)
        {
            value = UShort;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out int value)
    {
        if (Type == PrimitiveType.Int)
        {
            value = Int;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out uint value)
    {
        if (Type == PrimitiveType.UInt)
        {
            value = UInt;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out long value)
    {
        if (Type == PrimitiveType.Long)
        {
            value = Long;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out ulong value)
    {
        if (Type == PrimitiveType.ULong)
        {
            value = ULong;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out float value)
    {
        if (Type == PrimitiveType.Float)
        {
            value = Float;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out double value)
    {
        if (Type == PrimitiveType.Double)
        {
            value = Double;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out nint value)
    {
        if (Type == PrimitiveType.NInt)
        {
            value = NInt;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue(out nuint value)
    {
        if (Type == PrimitiveType.NUInt)
        {
            value = NUInt;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetValue([MaybeNullWhen(false)] out object value)
    {
        if (Type == PrimitiveType.Object)
        {
            value = Object!;
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetValue<T>([MaybeNullWhen(false)] out T value)
    {
        Unsafe.SkipInit(out value);
        switch (Type)
        {
            case PrimitiveType.Bool when typeof(T) == typeof(bool):
                Unsafe.As<T, bool>(ref value) = Bool;
                return true;
            case PrimitiveType.SByte when typeof(T) == typeof(sbyte):
                Unsafe.As<T, sbyte>(ref value) = SByte;
                return true;
            case PrimitiveType.Byte when typeof(T) == typeof(byte):
                Unsafe.As<T, byte>(ref value) = Byte;
                return true;
            case PrimitiveType.Short when typeof(T) == typeof(short):
                Unsafe.As<T, short>(ref value) = Short;
                return true;
            case PrimitiveType.UShort when typeof(T) == typeof(ushort):
                Unsafe.As<T, ushort>(ref value) = UShort;
                return true;
            case PrimitiveType.Int when typeof(T) == typeof(int):
                Unsafe.As<T, int>(ref value) = Int;
                return true;
            case PrimitiveType.UInt when typeof(T) == typeof(uint):
                Unsafe.As<T, uint>(ref value) = UInt;
                return true;
            case PrimitiveType.Long when typeof(T) == typeof(long):
                Unsafe.As<T, long>(ref value) = Long;
                return true;
            case PrimitiveType.ULong when typeof(T) == typeof(ulong):
                Unsafe.As<T, ulong>(ref value) = ULong;
                return true;
            case PrimitiveType.Float when typeof(T) == typeof(float):
                Unsafe.As<T, float>(ref value) = Float;
                return true;
            case PrimitiveType.Double when typeof(T) == typeof(double):
                Unsafe.As<T, double>(ref value) = Double;
                return true;
            case PrimitiveType.NInt when typeof(T) == typeof(nint):
                Unsafe.As<T, nint>(ref value) = NInt;
                return true;
            case PrimitiveType.NUInt when typeof(T) == typeof(nuint):
                Unsafe.As<T, nuint>(ref value) = NUInt;
                return true;
            case PrimitiveType.Object:
                var obj = Object;
                if (obj is not T tValue)
                    return false;
                value = tValue;
                return true;
            default:
                return false;
        }
    }

    public bool Equals(Primitive other)
    {
        return Type switch
        {
            PrimitiveType.Bool => other.Type == PrimitiveType.Bool && other.Bool == Bool,
            PrimitiveType.SByte => other.Type == PrimitiveType.SByte && other.SByte == SByte,
            PrimitiveType.Byte => other.Type == PrimitiveType.Byte && other.Byte == Byte,
            PrimitiveType.Short => other.Type == PrimitiveType.Short && other.Short == Short,
            PrimitiveType.UShort => other.Type == PrimitiveType.UShort && other.UShort == UShort,
            PrimitiveType.Int => other.Type == PrimitiveType.Int && other.Int == Int,
            PrimitiveType.UInt => other.Type == PrimitiveType.UInt && other.UInt == UInt,
            PrimitiveType.Long => other.Type == PrimitiveType.Long && other.Long == Long,
            PrimitiveType.ULong => other.Type == PrimitiveType.ULong && other.ULong == ULong,
            PrimitiveType.Float => other.Type == PrimitiveType.Float && other.Float.Equals(Float),
            PrimitiveType.Double => other.Type == PrimitiveType.Double && other.Double.Equals(Double),
            PrimitiveType.NInt => other.Type == PrimitiveType.NInt && other.NInt == NInt,
            PrimitiveType.NUInt => other.Type == PrimitiveType.NUInt && other.NUInt == NUInt,
            PrimitiveType.Object => other.Type == PrimitiveType.Object && Equals(other.Object, Object),
            _ => other.Type == Type,
        };
    }

    public override int GetHashCode()
    {
        return Type switch
        {
            PrimitiveType.Bool => HashCode.Combine(Type, Bool),
            PrimitiveType.SByte => HashCode.Combine(Type, SByte),
            PrimitiveType.Byte => HashCode.Combine(Type, Byte),
            PrimitiveType.Short => HashCode.Combine(Type, Short),
            PrimitiveType.UShort => HashCode.Combine(Type, UShort),
            PrimitiveType.Int => HashCode.Combine(Type, Int),
            PrimitiveType.UInt => HashCode.Combine(Type, UInt),
            PrimitiveType.Long => HashCode.Combine(Type, Long),
            PrimitiveType.ULong => HashCode.Combine(Type, ULong),
            PrimitiveType.Float => HashCode.Combine(Type, Float),
            PrimitiveType.Double => HashCode.Combine(Type, Double),
            PrimitiveType.NInt => HashCode.Combine(Type, NInt),
            PrimitiveType.NUInt => HashCode.Combine(Type, NUInt),
            PrimitiveType.Object => HashCode.Combine(Type, Object),
            _ => Type.GetHashCode(),
        };
    }

    public override string ToString()
    {
        return Type switch
        {
            PrimitiveType.Bool => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(Bool)])),
            PrimitiveType.SByte => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(SByte)])),
            PrimitiveType.Byte => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(Byte)])),
            PrimitiveType.Short => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(Short)])),
            PrimitiveType.UShort => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(UShort)])),
            PrimitiveType.Int => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(Int)])),
            PrimitiveType.UInt => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(UInt)])),
            PrimitiveType.Long => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(Long)])),
            PrimitiveType.ULong => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(ULong)])),
            PrimitiveType.Float => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(Float)])),
            PrimitiveType.Double => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(Double)])),
            PrimitiveType.NInt => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(NInt)])),
            PrimitiveType.NUInt => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type), nameof(NUInt)])),
            PrimitiveType.Object => ObjectPrinter.Print(
                this,
                ObjectPrinter.Include([nameof(Type), nameof(Object), nameof(UnderlyingType)])
            ),
            _ => ObjectPrinter.Print(this, ObjectPrinter.Include([nameof(Type)])),
        };
    }
}

public enum PrimitiveType : sbyte
{
    None = (sbyte)B2UserDataType.None,
    Long = (sbyte)B2UserDataType.Signed,
    ULong = (sbyte)B2UserDataType.Unsigned,
    Double = (sbyte)B2UserDataType.Double,
    Object = (sbyte)B2UserDataType.Ref,

    Bool = sbyte.MaxValue - 9,
    SByte = sbyte.MaxValue - 8,
    Byte = sbyte.MaxValue - 7,
    Short = sbyte.MaxValue - 6,
    UShort = sbyte.MaxValue - 5,
    Int = sbyte.MaxValue - 4,
    UInt = sbyte.MaxValue - 3,
    Float = sbyte.MaxValue - 2,
    NInt = sbyte.MaxValue - 1,
    NUInt = sbyte.MaxValue,
}

public static class PrimitiveTypeExtensions
{
    extension(PrimitiveType type)
    {
        public Type RuntimeType
        {
            get
            {
                return type switch
                {
                    PrimitiveType.Bool => typeof(bool),
                    PrimitiveType.SByte => typeof(sbyte),
                    PrimitiveType.Byte => typeof(byte),
                    PrimitiveType.Short => typeof(short),
                    PrimitiveType.UShort => typeof(ushort),
                    PrimitiveType.Int => typeof(int),
                    PrimitiveType.UInt => typeof(uint),
                    PrimitiveType.Long => typeof(long),
                    PrimitiveType.ULong => typeof(ulong),
                    PrimitiveType.Float => typeof(float),
                    PrimitiveType.Double => typeof(double),
                    PrimitiveType.NInt => typeof(nint),
                    PrimitiveType.NUInt => typeof(nuint),
                    PrimitiveType.Object => typeof(object),
                    _ => null!,
                };
            }
        }
    }
}
