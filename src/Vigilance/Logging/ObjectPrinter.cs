using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using LinkDotNet.StringBuilder;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Logging;

public static class ObjectPrinter
{
    public enum FilterType
    {
        Include,
        Exclude,
    }

    private static readonly Dictionary<Type, PropertyInfo[]> _properties = new();

    public static string Print<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        in T obj,
        Filter? filter = null,
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
            ref var props = ref CollectionsMarshal.GetValueRefOrAddDefault(_properties, type, out var exists);
            if (!exists)
                props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .AsValueEnumerable()
                    .Where(prop =>
                        prop.CanRead
                        && prop.GetIndexParameters().Length == 0
                        && !prop.PropertyType.IsByRefLike
                        && !prop.GetMethod!.ReturnType.IsByRef
                    )
                    .ToArray();
            var i = 0;
            if (filter.HasValue)
                foreach (var prop in filter.Value.Apply(props!))
                    PrintProperty(ref sb, boxed, prop, ref i, removeNulls);
            else
                foreach (var prop in props!)
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

    public static Filter Include(params string[] propertyNames)
    {
        return new Filter(FilterType.Include, propertyNames);
    }

    public static Filter Exclude(params string[] propertyNames)
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

    public readonly record struct Filter(FilterType Type, string[] PropertyNames)
    {
        public ValueEnumerable<ArrayWhere<PropertyInfo>, PropertyInfo> Apply(PropertyInfo[] properties)
        {
            var propertyNames = PropertyNames;
            return Type switch
            {
                FilterType.Include => properties.AsValueEnumerable().Where(p => propertyNames.Contains(p.Name)),
                FilterType.Exclude => properties.AsValueEnumerable().Where(p => !propertyNames.Contains(p.Name)),
                _ => throw new InvalidEnumArgumentException(nameof(Type), (int)Type, typeof(FilterType)),
            };
        }
    }
}
