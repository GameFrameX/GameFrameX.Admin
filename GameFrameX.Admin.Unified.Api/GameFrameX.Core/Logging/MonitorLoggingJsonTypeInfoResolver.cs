using System.Text.Json.Serialization.Metadata;

namespace GameFrameX.Core.Logging;

/// <summary>
/// Monitor 参数/返回值序列化的忽略解析器（语义对齐历史框架 LoggingMonitorJsonTypeInfoResolver）：
/// 按 <see cref="ProjectLoggingOptions.IgnorePropertyNames"/> 与 <see cref="ProjectLoggingOptions.IgnorePropertyTypes"/>
/// 把命中属性从序列化面剔除（如嵌套在 DTO 里的 byte[] / Byte 属性），避免二进制大字段灌爆日志。
/// </summary>
internal sealed class MonitorLoggingJsonTypeInfoResolver : IJsonTypeInfoResolver
{
    private readonly IJsonTypeInfoResolver _defaultResolver = new DefaultJsonTypeInfoResolver();
    private readonly HashSet<string> _ignorePropertyNames;
    private readonly List<Type> _ignorePropertyTypes;

    public MonitorLoggingJsonTypeInfoResolver(IEnumerable<string> ignorePropertyNames, IEnumerable<Type> ignorePropertyTypes)
    {
        _ignorePropertyNames = new HashSet<string>(ignorePropertyNames, StringComparer.Ordinal);
        _ignorePropertyTypes = new List<Type>(ignorePropertyTypes);
    }

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = _defaultResolver.GetTypeInfo(type, options);
        if (typeInfo?.Kind == JsonTypeInfoKind.Object)
        {
            foreach (var property in typeInfo.Properties)
            {
                if (_ignorePropertyNames.Contains(property.Name) || IgnoreTypeMatches(property.PropertyType))
                {
                    property.ShouldSerialize = static (_, _) => false;
                }
            }
        }

        return typeInfo;
    }

    private bool IgnoreTypeMatches(Type propertyType)
    {
        foreach (var ignoreType in _ignorePropertyTypes)
        {
            if (ignoreType.IsAssignableFrom(propertyType))
            {
                return true;
            }
        }

        return false;
    }
}
