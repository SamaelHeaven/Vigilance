namespace Vigilance.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class ValueWrapperAttribute(Type value, string fieldName = "Value", Type?[]? typeParams = null)
    : Attribute
{
    public Type Value { get; } = value;
    public string FieldName { get; } = fieldName;
    public Type?[]? TypeParams { get; } = typeParams;
}
