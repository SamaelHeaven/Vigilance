using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LinkDotNet.StringBuilder;
using Vigilance.Collections;
using ZLinq;

namespace Vigilance.Logging;

public static class ObjectPrinter
{
    public enum FilterType : sbyte
    {
        None,
        Include,
        Exclude,
    }

    private static ValueDictionary<Type, PropertyInfo[]> _properties = [];

    public static string Print<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        in T obj,
        Filter filter = default,
        bool removeNulls = false
    )
        where T : notnull
    {
        var type = typeof(T);
        var sb = new ValueStringBuilder(stackalloc char[256]);
        object boxed = obj;
        try
        {
            sb.Append(type.Name);
            sb.Append(" { ");
            ref var props = ref _properties.GetValueRefOrAddDefault(type, out var exists)!;
            if (!exists)
                props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .AsValueEnumerable()
                    .Where(prop =>
                        prop is { CanRead: true, PropertyType.IsByRefLike: false }
                        && !prop.GetMethod!.ReturnType.IsByRef
                        && prop.GetIndexParameters().Length == 0
                    )
                    .ToArray();
            var i = 0;
            foreach (var prop in props)
                if (filter.Matches(prop))
                    PrintProperty(ref sb, boxed, prop, ref i, removeNulls);
            if (i > 0)
                sb.Append(' ');
            sb.Append('}');
            return sb.ToString();
        }
        finally
        {
            sb.Dispose();
        }
    }

    public static Filter Include(in ReadOnlySpan<string> propertyNames)
    {
        return new Filter(FilterType.Include, propertyNames);
    }

    public static Filter Exclude(in ReadOnlySpan<string> propertyNames)
    {
        return new Filter(FilterType.Exclude, propertyNames);
    }

    private static void PrintProperty(
        ref ValueStringBuilder sb,
        object obj,
        PropertyInfo prop,
        ref int i,
        bool removeNulls
    )
    {
        var value = prop.GetValue(obj);
        if (removeNulls && value is null)
            return;
        if (i++ > 0)
            sb.Append(", ");
        sb.Append(prop.Name);
        sb.Append(" = ");
        sb.Append(value?.ToString());
    }

    public readonly ref struct Filter
    {
        public FilterType Type { get; }
        public ReadOnlySpan<string> PropertyNames { get; }

        public Filter(FilterType type, in ReadOnlySpan<string> propertyNames)
        {
            Type = type;
            PropertyNames = propertyNames;
        }

        public bool Matches(PropertyInfo property)
        {
            return Type switch
            {
                FilterType.None => true,
                FilterType.Include => PropertyNames.Contains(property.Name),
                FilterType.Exclude => !PropertyNames.Contains(property.Name),
                _ => throw new InvalidEnumArgumentException(nameof(Type), (int)Type, typeof(FilterType)),
            };
        }
    }
}
