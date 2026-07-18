using System.Text.Json.Serialization.Metadata;

namespace Haiyu.Analyzers;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class SettingsAttribute<T> : Attribute
{
    public string? Name { get; set; }
    public Type? Type { get; set; }
    public bool Nullable { get; set; }
    public string? DefaultValue { get; set; }
    public JsonTypeInfo<T>? JsonTypeInfo { get; set; }
    public Type? JsonTypeInfoContextType { get; set; }
    public string? JsonTypeInfoPropertyName { get; set; }
}
