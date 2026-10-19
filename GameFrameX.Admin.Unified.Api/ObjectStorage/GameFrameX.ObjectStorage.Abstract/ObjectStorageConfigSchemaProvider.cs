using System.Reflection;

namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 默认 Schema 提供者。基于 <see cref="ObjectStorageProviderRegistry"/> 反射 Option 类型的
/// 公共可写属性（跳过标 Ignore / 只读 / 索引属性），结合 <see cref="ObjectStorageFieldAttribute"/>
/// 生成字段表单结构。敏感字段映射为 secret，多行为 multiline，带 Options 为 select，
/// bool/数值类型对应映射，其余为 text。
/// </summary>
public class ObjectStorageConfigSchemaProvider : IObjectStorageConfigSchemaProvider
{
    /// <inheritdoc />
    public List<ObjectStorageProviderItem> GetProviders()
    {
        return ObjectStorageProviderRegistry.List()
                                            .OrderBy(b => b.Provider, StringComparer.Ordinal)
                                            .Select(b => new ObjectStorageProviderItem { Value = b.Provider, Label = b.Label, })
                                            .ToList();
    }

    /// <inheritdoc />
    public ObjectStorageConfigSchema GetSchema(string provider)
    {
        if (!ObjectStorageProviderRegistry.TryGet(provider, out var binding))
        {
#pragma warning disable CS8603 // 跨项目接口 IObjectStorageConfigSchemaProvider.GetSchema 返回非空 ObjectStorageConfigSchema；未注册 Provider 返回 null 是既定查找语义，调用方（ObjectStorageConfigNormalizer）已做 null 检查，签名不改 T?。
            return null;
#pragma warning restore CS8603
        }

        var schema = new ObjectStorageConfigSchema
        {
            Provider = binding.Provider,
            Label = binding.Label,
            Fields = new List<ObjectStorageConfigField>(),
        };

        object? defaults = null;
        try
        {
            defaults = Activator.CreateInstance(binding.OptionType);
        }
        catch
        {
            defaults = null;
        }

        foreach (var prop in binding.OptionType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanWrite || prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var attr = prop.GetCustomAttribute<ObjectStorageFieldAttribute>();
            if (attr != null && attr.Ignore)
            {
                continue;
            }

#pragma warning disable CS8601 // ObjectStorageConfigField.Help/Options 是 DTO 契约属性（= null!; 声明非空），运行时可为 null 以表达前端表单的"无帮助文本/无可选项"语义；Default 来自 ReadDefault 反射，也可能为 null。属性类型保持非空以避免级联修改跨项目消费者。
            schema.Fields.Add(new ObjectStorageConfigField
            {
                Key = prop.Name,
                Label = attr != null && !string.IsNullOrEmpty(attr.Label) ? attr.Label : prop.Name,
                Type = ResolveType(prop, attr),
                Required = attr != null && attr.Required,
                Help = attr != null ? attr.Help : null,
                Default = ReadDefault(defaults, prop),
                Options = attr != null && attr.Options != null && attr.Options.Length > 0 ? attr.Options : null,
                Sensitive = attr != null && attr.Sensitive,
            });
#pragma warning restore CS8601
        }

        return schema;
    }

    private static string ResolveType(PropertyInfo prop, ObjectStorageFieldAttribute? attr)
    {
        if (attr != null && attr.Options != null && attr.Options.Length > 0)
        {
            return "select";
        }

        if (attr != null && attr.Sensitive)
        {
            return "secret";
        }

        if (attr != null && attr.Multiline)
        {
            return "multiline";
        }

        var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        if (type == typeof(bool))
        {
            return "bool";
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte)
            || type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            return "number";
        }

        return "text";
    }

    private static object? ReadDefault(object? instance, PropertyInfo prop)
    {
        if (instance == null)
        {
            return null;
        }

        try
        {
            return prop.GetValue(instance);
        }
        catch
        {
            return null;
        }
    }
}