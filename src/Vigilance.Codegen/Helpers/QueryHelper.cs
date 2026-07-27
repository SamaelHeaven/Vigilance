namespace Vigilance.Codegen.Helpers;

public static class QueryHelper
{
    public static string NamedTuple(bool hasEntity, IReadOnlyList<string> componentTypes)
    {
        var parts = new List<string>();
        if (hasEntity)
            parts.Add("Entity Entity");
        parts.AddRange(
            componentTypes.Select((t, n) => $"{t} Component{(componentTypes.Count == 1 ? "" : (n + 1).ToString())}")
        );
        return $"({string.Join(", ", parts)})";
    }
}
