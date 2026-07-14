using System;
using System.Text.Json.Serialization.Metadata;

namespace Haiyu.Analyzers;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class SettingsAttribute<T>:Attribute
{
    public string? Name { get; set; }
    public Type? Type { get; set; }
    public bool Nullable { get; set; }
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 类型为object的时候使用这个属性进行序列化和反序列化
    /// </summary>
    public JsonTypeInfo<T> JsonTypeInfo { get; set;  }
}
